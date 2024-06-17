using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Utility.DisposableExtension;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Selectable;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;

public class SettingsEntityDropdownConsoleView : SettingsEntityWithValueConsoleView<SettingsEntityDropdownVM>
{
	[SerializeField]
	private OwlcatMultiButton m_SelectableMultiButton;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private OwlcatDropdown m_Dropdown;

	private readonly DisposableBooleanFlag m_ChangingFromUI = new DisposableBooleanFlag();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetupDropdown();
		AddDisposable(base.ViewModel.TempIndexValue.Subscribe(SetValueFromSettings));
		if (m_Dropdown != null)
		{
			AddDisposable(m_Dropdown.Index.Subscribe(SetValueFromUI));
		}
	}

	private void SetupDropdown()
	{
		m_Dropdown.Bind(base.ViewModel.GetSorterDropDownVM());
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_SelectableMultiButton.SetFocus(value);
		m_SelectableMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	protected virtual void SetValueFromUI(int value)
	{
		if (base.ViewModel.GetTempValue() == value)
		{
			return;
		}
		using (m_ChangingFromUI.Retain())
		{
			base.ViewModel.SetTempValue(value);
		}
	}

	private void SetValueFromSettings(int value)
	{
		if (!m_ChangingFromUI)
		{
			m_Dropdown.SetIndex(value);
		}
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
		base.OnModificationChanged(reason, allowed);
		m_Dropdown.SetInteractable(allowed);
		if (!(m_MultiSelectable == null))
		{
			m_MultiSelectable.Interactable = allowed;
			m_MultiSelectable.SetActiveLayer(allowed ? "On" : "Off");
		}
	}

	public override bool HandleLeft()
	{
		if (base.ViewModel.ModificationAllowed.Value)
		{
			m_Dropdown.SetIndex((m_Dropdown.Index.Value - 1 + m_Dropdown.VMCollectionCount) % m_Dropdown.VMCollectionCount);
		}
		else
		{
			CallNotAllowedNotification();
		}
		return true;
	}

	public override bool HandleRight()
	{
		if (base.ViewModel.ModificationAllowed.Value)
		{
			m_Dropdown.SetIndex((m_Dropdown.Index.Value + 1) % m_Dropdown.VMCollectionCount);
		}
		else
		{
			CallNotAllowedNotification();
		}
		return true;
	}
}
