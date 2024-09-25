using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.IngameMenu.Console;

public class IngameMenuItemConsoleView : MonoBehaviour, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	private Action m_Action;

	public void Initialize(string text)
	{
		base.gameObject.SetActive(value: false);
		m_Label.text = text;
	}

	public void Bind(Action action)
	{
		m_Action = action;
		base.gameObject.SetActive(value: true);
	}

	public void OnConfirmClick()
	{
		m_Action?.Invoke();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
