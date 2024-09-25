using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;

public class SurfaceActionBarPartWeaponSetVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly AutoDisposingList<ActionBarSlotVM> MainHandSlots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly AutoDisposingList<ActionBarSlotVM> OffHandSlots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly AutoDisposingList<ActionBarSlotVM> ComboHandsSlots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly ReactiveProperty<ItemSlotVM> MainHandWeapon = new ReactiveProperty<ItemSlotVM>();

	public readonly ReactiveProperty<ItemSlotVM> OffHandWeapon = new ReactiveProperty<ItemSlotVM>();

	public readonly ReactiveCommand SlotsUpdated = new ReactiveCommand();

	public readonly ReactiveProperty<bool> IsCurrent = new ReactiveProperty<bool>();

	public int Index;

	public bool IsTwoHanded;

	public HandsEquipmentSet HandSet;

	private Action m_SwitchSetAction;

	public EntityRef<BaseUnitEntity> Unit;

	public List<ActionBarSlotVM> AllSlots
	{
		get
		{
			List<ActionBarSlotVM> list = new List<ActionBarSlotVM>();
			list.AddRange(MainHandSlots);
			list.AddRange(OffHandSlots);
			list.AddRange(ComboHandsSlots);
			return list;
		}
	}

	public void InitForUnit(int index, EntityRef<BaseUnitEntity> unit, HandsEquipmentSet handSet, Action switchWeapon)
	{
		Index = index;
		Unit = unit;
		HandSet = handSet;
		m_SwitchSetAction = switchWeapon;
		UpdateSlots();
	}

	public void UpdateIsCurrent(bool state)
	{
		IsCurrent.Value = state;
		UpdateSlots();
	}

	public void UpdateSlots()
	{
		ClearAll();
		IsTwoHanded = (HandSet.PrimaryHand?.MaybeItem as ItemEntityWeapon)?.HoldInTwoHands ?? false;
		ItemSlotVM disposable = (MainHandWeapon.Value = new ItemSlotVM(HandSet.PrimaryHand?.MaybeItem, 0, null, compareEnabled: false));
		AddDisposable(disposable);
		disposable = (OffHandWeapon.Value = new ItemSlotVM(HandSet.SecondaryHand?.MaybeItem, 1, null, compareEnabled: false));
		AddDisposable(disposable);
		if (IsCurrent.Value)
		{
			if (MainHandSlots.Any((ActionBarSlotVM s) => s.IsFake.Value))
			{
				MainHandSlots.Clear();
			}
			if (OffHandSlots.Any((ActionBarSlotVM s) => s.IsFake.Value))
			{
				OffHandSlots.Clear();
			}
			AddSlotsForHand(HandSet.PrimaryHand, MainHandSlots);
			AddSlotsForHand(HandSet.SecondaryHand, OffHandSlots);
		}
		else
		{
			if (MainHandSlots.Empty())
			{
				AddSlotsSceletons(HandSet.PrimaryHand, MainHandSlots);
			}
			if (OffHandSlots.Empty())
			{
				AddSlotsSceletons(HandSet.SecondaryHand, OffHandSlots);
			}
		}
		SlotsUpdated.Execute();
	}

	private void AddSlotsForHand(HandSlot hand, IList<ActionBarSlotVM> slots)
	{
		if (hand.MaybeItem != null)
		{
			for (int i = 0; i < hand.MaybeItem.Abilities.Count; i++)
			{
				TryAddAbilityToSlots(slots, hand.MaybeItem.Abilities[i], i);
			}
		}
	}

	private void AddSlotsSceletons(HandSlot hand, IList<ActionBarSlotVM> slots)
	{
		if (!(hand.MaybeItem?.Blueprint is BlueprintItemWeapon blueprintItemWeapon))
		{
			return;
		}
		foreach (WeaponAbility weaponAbility in blueprintItemWeapon.WeaponAbilities)
		{
			ActionBarSlotVM actionBarSlotVM = new ActionBarSlotVM(weaponAbility, hand.MaybeItem as ItemEntityWeapon);
			AddDisposable(actionBarSlotVM);
			slots.Add(actionBarSlotVM);
		}
	}

	private void TryAddAbilityToSlots(IList<ActionBarSlotVM> slots, Ability ability, int index)
	{
		if (!slots.Any((ActionBarSlotVM s) => (s.MechanicActionBarSlot as MechanicActionBarSlotAbility).Ability == ability.Data))
		{
			MechanicActionBarSlotAbility mechanicActionBarSlotAbility = new MechanicActionBarSlotAbility
			{
				Ability = ability.Data,
				Unit = Unit
			};
			if (mechanicActionBarSlotAbility.IsBad())
			{
				PFLog.Ability.Error(ability.Name + " lost Owner in its Fact");
				return;
			}
			ActionBarSlotVM actionBarSlotVM = new ActionBarSlotVM(mechanicActionBarSlotAbility, index);
			AddDisposable(actionBarSlotVM);
			slots.Add(actionBarSlotVM);
		}
	}

	protected override void DisposeImplementation()
	{
		ClearAll();
	}

	public void ClearAll()
	{
		MainHandSlots.Clear();
		OffHandSlots.Clear();
		ComboHandsSlots.Clear();
	}

	public void SwitchWeapon()
	{
		m_SwitchSetAction();
	}
}
