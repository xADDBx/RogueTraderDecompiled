using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class ItemsFilterSearchConsoleView : ItemsFilterSearchBaseView
{
	[SerializeField]
	private ConsoleInputField m_ConsoleInputField;

	[SerializeField]
	private ConsoleHint m_SearchHint;

	private readonly BoolReactiveProperty m_IsActive = new BoolReactiveProperty();

	public bool IsActive => m_IsActive.Value;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ConsoleInputField.SetPlaceholderText(UIStrings.Instance.CharGen.EnterSearchTextHere);
		m_ConsoleInputField.Bind(null, base.OnSearchStringEdit);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		AddDisposable(m_SearchHint.Bind(inputLayer.AddButton(delegate
		{
			m_ConsoleInputField.Select();
		}, 16, enabledHints)));
	}

	public IDisposable AddInputDisposable(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		return m_SearchHint.Bind(inputLayer.AddButton(delegate
		{
			m_ConsoleInputField.Select();
		}, 16, enabledHints));
	}

	public override void SetActive(bool value)
	{
		m_IsActive.Value = value;
		base.gameObject.SetActive(value);
		m_ConsoleInputField.Text = string.Empty;
		ContextMenuHelper.HideContextMenu();
		if (value)
		{
			m_ConsoleInputField.OnConfirmClick();
			return;
		}
		m_ConsoleInputField.Abort();
		OnSearchStringEdit(null);
	}

	private void ShowDropdownMenu(InputActionEventData data)
	{
		TooltipHelper.HideTooltip();
	}
}
