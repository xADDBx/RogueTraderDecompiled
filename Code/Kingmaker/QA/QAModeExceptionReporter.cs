using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Cheats;
using Kingmaker.Code.UI.Legacy.BugReportDrawing;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Rewired;
using UnityEngine;

namespace Kingmaker.QA;

public class QAModeExceptionReporter : MonoBehaviour
{
	private static BugReportCanvas s_ReportCanvas;

	private static readonly HashSet<string> SeenMessages = new HashSet<string>();

	private static readonly List<(string message, bool addToReport)> CurrentMessages = new List<(string, bool)>();

	private static Exception LastException;

	private static bool s_PrevCursorState;

	public static bool MaybeShowError(string message, Exception ex = null, UnityEngine.Object ctx = null, bool addMessageToReport = true)
	{
		int num;
		if (!BuildModeUtility.IsPlayTest && BuildModeUtility.IsDevelopment)
		{
			if (!CheatsJira.QaMode)
			{
				num = ((ex is SpamDetectionException) ? 1 : 0);
				if (num == 0)
				{
					goto IL_0075;
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
				ShowError(message, addMessageToReport);
			}
		}
		else
		{
			num = 0;
		}
		goto IL_0075;
		IL_0075:
		return (byte)num != 0;
	}

	private static void ShowError(string message, bool addMessageToReport)
	{
		if (BuildModeUtility.IsDevelopment)
		{
			s_PrevCursorState = Cursor.visible;
			Cursor.visible = true;
			CurrentMessages.Add((message, addMessageToReport));
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
		bool flag = Game.Instance.IsControllerGamepad && (ReInput.players.GetPlayer(0)?.GetButtonDown(9) ?? false);
		if (GUI.Button(new Rect(10f, 0f, num / 2 - 20, 20f), "Close (B)") || flag)
		{
			Cursor.visible = s_PrevCursorState;
			CurrentMessages.Clear();
			LastException = null;
			ReportingUtils.Instance.ExceptionSource = null;
		}
		bool flag2 = Game.Instance.IsControllerGamepad && (ReInput.players.GetPlayer(0)?.GetButtonDown(8) ?? false);
		if (GUI.Button(new Rect(10 + num / 2, 0f, num / 2 - 20, 20f), "Report (A)") || flag2)
		{
			Cursor.visible = s_PrevCursorState;
			string[] messages = (from x in CurrentMessages
				where x.addToReport
				select x.message).ToArray();
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
		foreach (var currentMessage in CurrentMessages)
		{
			GUILayout.Label(currentMessage.message);
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
