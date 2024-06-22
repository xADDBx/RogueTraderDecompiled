using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public class SettingsEntityDropdownVM : SettingsEntityWithValueVM, IVirtualListElementIdentifier
{
	public enum DropdownType
	{
		Default,
		DisplayMode
	}

	private readonly DropdownType m_DropdownType;

	public const int DefaultDropdownIndex = 0;

	public const int DisplayModeDropdownIndex = 1;

	private readonly IUISettingsEntityDropdown m_UISettingsEntity;

	public readonly ReadOnlyReactiveProperty<int> TempIndexValue;

	private OwlcatDropdownVM m_DropdownVM;

	public int VirtualListTypeId => (int)m_DropdownType;

	public IReadOnlyList<string> LocalizedValues => m_UISettingsEntity.LocalizedValues;

	public bool IsNextValue => GetTempValue() + 1 < m_UISettingsEntity.ValuesCount();

	public bool IsPrevValue => GetTempValue() - 1 >= 0;

	public SettingsEntityDropdownVM(IUISettingsEntityDropdown uiSettingsEntity, DropdownType dropdownType = DropdownType.Default, bool hideMarkImage = false)
		: base(uiSettingsEntity, hideMarkImage)
	{
		m_UISettingsEntity = uiSettingsEntity;
		m_DropdownType = dropdownType;
		AddDisposable(TempIndexValue = Observable.FromEvent(delegate(Action<int> h)
		{
			uiSettingsEntity.OnTempIndexValueChanged += h;
		}, delegate(Action<int> h)
		{
			uiSettingsEntity.OnTempIndexValueChanged -= h;
		}).ToReadOnlyReactiveProperty(uiSettingsEntity.GetIndexTempValue()));
	}

	public int GetTempValue()
	{
		return m_UISettingsEntity.GetIndexTempValue();
	}

	public void SetTempValue(int value)
	{
		SetValue(value);
	}

	public void SetValueAndConfirm(int value)
	{
		SetValue(value, force: true);
	}

	public void SetNextValue()
	{
		SetTempValue((GetTempValue() + 1) % m_UISettingsEntity.ValuesCount());
	}

	public void SetPrevValue()
	{
		SetTempValue((GetTempValue() - 1 + m_UISettingsEntity.ValuesCount()) % m_UISettingsEntity.ValuesCount());
	}

	private void SetValue(int value, bool force = false)
	{
		if (ModificationAllowed.Value)
		{
			if (force)
			{
				m_UISettingsEntity.SetIndexValueAndConfirm(value);
			}
			else
			{
				m_UISettingsEntity.SetIndexTempValue(value);
			}
		}
	}

	public OwlcatDropdownVM GetSorterDropDownVM()
	{
		List<DropdownItemVM> list = new List<DropdownItemVM>();
		if (LocalizedValues != null && LocalizedValues.Count > 0)
		{
			foreach (string localizedValue in LocalizedValues)
			{
				list.Add(new DropdownItemVM(localizedValue));
			}
		}
		m_DropdownVM?.Dispose();
		return m_DropdownVM = new OwlcatDropdownVM(list);
	}

	protected override void DisposeImplementation()
	{
		m_DropdownVM?.Dispose();
		base.DisposeImplementation();
	}
}
