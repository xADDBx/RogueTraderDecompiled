using System;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarArsenalSlot : MechanicActionBarSlotSpontaneusConvertedSpell, IHashable
{
	private readonly ActionBarSlotVM m_ParentSlot;

	private readonly WeaponSlot m_WeaponSlot;

	private readonly Action m_OnClose;

	public MechanicActionBarArsenalSlot(ActionBarSlotVM parentSlot, WeaponSlot weaponSlot, Action onClose)
	{
		m_ParentSlot = parentSlot;
		m_WeaponSlot = weaponSlot;
		m_OnClose = onClose;
	}

	public override void OnClick()
	{
		int activeWeaponIndex = m_WeaponSlot.AbilityVariants.FindIndex((Ability a) => a.Data == Spell);
		m_WeaponSlot.ActiveWeaponIndex = activeWeaponIndex;
		MechanicActionBarShipWeaponSlot mechanicSlot = new MechanicActionBarShipWeaponSlot(m_WeaponSlot, base.Unit)
		{
			Ability = m_WeaponSlot.ActiveAbility.Data
		};
		m_ParentSlot.SetMechanicSlot(mechanicSlot);
		m_OnClose?.Invoke();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
