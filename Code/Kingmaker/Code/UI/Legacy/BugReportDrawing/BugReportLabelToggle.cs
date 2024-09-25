using System;
using Kingmaker.UI.Workarounds;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.Legacy.BugReportDrawing;

public class BugReportLabelToggle : MonoBehaviour, IDisposable, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	public ToggleWorkaround WorkToggle;

	public TMP_Text TMP_Text;

	private string m_VersionLabelName;

	public GameObject m_Selection;

	public void Initiate(Action<string, bool> labelSelectedAction, string labelName, bool isOn)
	{
		m_VersionLabelName = labelName;
		WorkToggle.isOn = isOn;
		WorkToggle.onValueChanged.AddListener(delegate
		{
			labelSelectedAction(m_VersionLabelName, WorkToggle.isOn);
		});
		m_Selection.SetActive(value: false);
		TMP_Text.text = labelName;
	}

	public void Dispose()
	{
		WorkToggle.onValueChanged.RemoveAllListeners();
	}

	void IConsoleNavigationEntity.SetFocus(bool value)
	{
		m_Selection.SetActive(value);
	}

	bool IConsoleNavigationEntity.IsValid()
	{
		return true;
	}

	bool IConfirmClickHandler.CanConfirmClick()
	{
		return true;
	}

	void IConfirmClickHandler.OnConfirmClick()
	{
		WorkToggle.isOn = !WorkToggle.isOn;
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
