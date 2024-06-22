using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;

public class AbilitiesGroupVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISpaceCombatActionsHolder
{
	public readonly string GroupLabel;

	public readonly ReactiveCollection<ActionBarSlotVM> Slots = new ReactiveCollection<ActionBarSlotVM>();

	public readonly AutoDisposingList<ActionBarSlotVM> EmptySlots = new AutoDisposingList<ActionBarSlotVM>();

	public AbilitiesGroupVM(string groupLabel)
	{
		GroupLabel = groupLabel;
	}

	protected override void DisposeImplementation()
	{
		Clear();
		EmptySlots.Clear();
	}

	public void SetAbilities(IEnumerable<Ability> abilities, BaseUnitEntity owner, WeaponSlotType weaponSlotType = WeaponSlotType.None)
	{
		Clear();
		if (abilities != null)
		{
			for (int i = 0; i < abilities.Count(); i++)
			{
				Slots.Add(new ActionBarSlotVM(new MechanicActionBarSlotAbility
				{
					Ability = abilities.ToList()[i].Data,
					Unit = owner
				}, i, isInCharScreen: false, null, weaponSlotType));
			}
		}
	}

	public void SetWeaponSlotAbilities(List<WeaponSlot> weaponSlots, BaseUnitEntity owner, WeaponSlotType weaponSlotType = WeaponSlotType.None)
	{
		Clear();
		if (weaponSlots != null)
		{
			for (int i = 0; i < weaponSlots.Count; i++)
			{
				MechanicActionBarShipWeaponSlot abs = new MechanicActionBarShipWeaponSlot(weaponSlots[i], owner);
				Slots.Add(new ActionBarSlotVM(abs, i, isInCharScreen: false, null, weaponSlotType));
			}
		}
	}

	public void UpdateEmptySlots(int count)
	{
		if (count >= 0 && EmptySlots.Count != count)
		{
			int num = count - EmptySlots.Count;
			int num2 = ((num > 0) ? num : 0);
			int end = ((num < 0) ? Math.Abs(num) : 0);
			for (int i = 0; i < num2; i++)
			{
				EmptySlots.Add(new ActionBarSlotVM(new MechanicActionBarSlotEmpty(), i));
			}
			EmptySlots.RemoveRangeAndDispose(0, end);
		}
	}

	private void Clear()
	{
		Slots.ForEach(delegate(ActionBarSlotVM slotVm)
		{
			slotVm.Dispose();
		});
		Slots.Clear();
	}

	public bool HasActions()
	{
		return Enumerable.Any(Slots, (ActionBarSlotVM slotVm) => slotVm.IsPossibleActive.Value);
	}

	public void HighlightOn()
	{
	}

	public void HighlightOff()
	{
	}
}
