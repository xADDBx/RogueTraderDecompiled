using Kingmaker.Code.UI.MVVM.View.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Utility.DisposableExtension;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;

public class SettingsEntityDropdownPCView : SettingsEntityWithValueView<SettingsEntityDropdownVM>
{
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
		SubscribeNotAllowedSelectable(m_Dropdown);
	}

	private void SetupDropdown()
	{
		m_Dropdown.Bind(base.ViewModel.GetSorterDropDownVM());
	}

	private void SetValueFromUI(int value)
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
		SetNotAllowedModificationHint(m_Dropdown);
	}

	public override bool HandleLeft()
	{
		if (base.ViewModel.ModificationAllowed.Value)
		{
			m_Dropdown.SetIndex(m_Dropdown.Index.Value - 1);
		}
		return true;
	}

	public override bool HandleRight()
	{
		if (base.ViewModel.ModificationAllowed.Value)
		{
			m_Dropdown.SetIndex(m_Dropdown.Index.Value + 1);
		}
		return true;
	}
}
