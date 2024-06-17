using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.Legacy.BugReportDrawing;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.QA;

public class QAModeExceptionReporter : MonoBehaviour
{
	private static BugReportCanvas s_ReportCanvas;

	private static readonly HashSet<string> SeenMessages = new HashSet<string>();

	private static readonly List<string> CurrentMessages = new List<string>();

	private static Exception LastException;

	private static bool s_PrevCursorState;

	public static bool MaybeShowError(string message, Exception ex = null, UnityEngine.Object ctx = null)
	{
		int num;
		if (BuildModeUtility.IsDevelopment)
		{
			if (!CheatsJira.QaMode)
			{
				num = ((ex is SpamDetectingException) ? 1 : 0);
				if (num == 0)
				{
					goto IL_006d;
				}
			}
			else
			{
				num = 1;
			}
			if (ex != null)
			{
				LastException = ex;
				message = ((!string.IsNullOrEmpty(message)) ? (message + "\n" + ex.ToStringText()) : ex.ToStringText());
			}
			if (!SeenMessages.Contains(message))
			{
				SeenMessages.Add(message);
				ShowError(message);
			}
		}
		else
		{
			num = 0;
		}
		goto IL_006d;
		IL_006d:
		return (byte)num != 0;
	}

	private static void ShowError(string message)
	{
		if (BuildModeUtility.IsDevelopment)
		{
			s_PrevCursorState = Cursor.visible;
			Cursor.visible = true;
			CurrentMessages.Add(message);
			if (LoadingProcess.Instance.CurrentProcessTag != LoadingProcessTag.ExceptionReporter)
			{
				LoadingProcess.Instance.StartLoadingProcess(PauseForError(), null, LoadingProcessTag.ExceptionReporter);
			}
		}
	}

	private static IEnumerator PauseForError()
	{
		while (BuildModeUtility.IsDevelopment && CurrentMessages.Count > 0 && (bool)UnityEngine.Object.FindObjectOfType<QAModeExceptionReporter>())
		{
			yield return null;
		}
	}

	private void OnGUI()
	{
		if (!BuildModeUtility.IsDevelopment || CurrentMessages.Count == 0)
		{
			return;
		}
		if (s_ReportCanvas == null)
		{
			s_ReportCanvas = UnityEngine.Object.FindObjectOfType<BugReportCanvas>();
		}
		int num = Mathf.Max(Screen.width / 4, 450);
		int num2 = Mathf.Max(Screen.height / 2, 600);
		GUILayout.BeginArea(new Rect((Screen.width - num) / 2, (Screen.height - num2) / 2, num, num2));
		GUI.DrawTexture(new Rect(0f, 0f, num, num2), Texture2D.whiteTexture);
		GUI.contentColor = Color.black;
		if (GUI.Button(new Rect(10f, 0f, num / 2 - 20, 20f), "Close"))
		{
			Cursor.visible = s_PrevCursorState;
			CurrentMessages.Clear();
			LastException = null;
			ReportingUtils.Instance.ExceptionSource = null;
		}
		if (GUI.Button(new Rect(10 + num / 2, 0f, num / 2 - 20, 20f), "Report"))
		{
			Cursor.visible = s_PrevCursorState;
			string[] messages = CurrentMessages.ToArray();
			Exception lastException = LastException;
			CurrentMessages.Clear();
			LastException = null;
			s_ReportCanvas.Or(null)?.OnHotKeyBugReportOpen();
			EventBus.RaiseEvent(delegate(IBugReportDescriptionUIHandler h)
			{
				h.HandleErrorMessages(messages);
				if (lastException != null)
				{
					h.HandleException(lastException);
				}
			});
		}
		GUILayout.BeginVertical();
		GUILayout.Space(20f);
		foreach (string currentMessage in CurrentMessages)
		{
			GUILayout.Label(currentMessage);
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
