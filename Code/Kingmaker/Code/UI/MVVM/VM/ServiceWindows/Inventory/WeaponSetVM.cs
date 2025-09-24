using System;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class WeaponSetVM : SelectionGroupEntityVM
{
	public readonly ReactiveProperty<bool> IsEnabled = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<bool> HasMainHand = new ReactiveProperty<bool>(initialValue: true);

	public readonly Action OnSetChange;

	public int Index { get; set; }

	public EquipSlotVM Primary { get; set; }

	public EquipSlotVM Secondary { get; set; }

	public WeaponSetVM(int index, Action onSetChange, bool hasMainHand = false)
		: base(allowSwitchOff: false)
	{
		Index = index;
		OnSetChange = onSetChange;
		HasMainHand.Value = hasMainHand;
	}

	public WeaponSetVM(int index, EquipSlotVM primary, EquipSlotVM secondary, Action onSetChange)
		: this(index, onSetChange)
	{
		Index = index;
		Primary = primary;
		Secondary = secondary;
	}

	protected override void DoSelectMe()
	{
		OnSetChange?.Invoke();
	}

	public void SetAvailable(bool state)
	{
		SetAvailableState(state);
	}

	public void SetEnabled(bool enabled)
	{
		IsEnabled.Value = enabled;
		SetAvailableState(enabled);
	}

	public void SetMainHand(bool value)
	{
		HasMainHand.Value = value;
	}
}
