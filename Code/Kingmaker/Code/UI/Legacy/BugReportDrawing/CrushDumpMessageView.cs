using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.Legacy.BugReportDrawing;

public class CrushDumpMessageView : MonoBehaviour
{
	[SerializeField]
	private GameObject m_MessageBox;

	[SerializeField]
	private TextMeshProUGUI m_MessageLabel;

	[SerializeField]
	private BugReportFromFallbackHandler BugReportFromFallbackHandler;

	[SerializeField]
	private BugReportCanvas m_BugReportCanvas;

	public void Awake()
	{
		if (CrushDumpMessage.Exception != null)
		{
			m_MessageBox.SetActive(value: true);
			try
			{
				BugReportFromFallbackHandler.LastException = CrushDumpMessage.Exception;
				string[] array = new string[2]
				{
					CrushDumpMessage.Exception.Message,
					CrushDumpMessage.Exception.StackTrace
				};
				m_MessageLabel.text = array[0] + array[1].Split('\n').FirstOrDefault();
			}
			catch (Exception arg)
			{
				PFLog.System.Log($"CrushDumpMessage: {arg}");
			}
			CrushDumpMessage.Exception = null;
		}
		else
		{
			m_MessageBox.SetActive(value: false);
		}
	}

	public void Start()
	{
		if (ReportingUtils.Instance.CheckCrashDumpFound())
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
			{
				w.HandleOpen(UIStrings.Instance.MainMenu.CrashDumpFoundDialogue, DialogMessageBoxBase.BoxType.Dialog, OnCrashDumpReport);
			});
		}
	}

	private void OnCrashDumpReport(DialogMessageBoxBase.BoxButton button)
	{
		if (button == DialogMessageBoxBase.BoxButton.Yes)
		{
			m_BugReportCanvas.OnCrashDumpReportOpen();
		}
	}
}
