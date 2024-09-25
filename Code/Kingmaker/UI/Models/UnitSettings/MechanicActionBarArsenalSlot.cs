using System;
using Kingmaker.GameCommands;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarArsenalSlot : MechanicActionBarSlotSpontaneusConvertedSpell, IHashable
{
	private readonly WeaponSlot m_WeaponSlot;

	private readonly Action m_OnClose;

	public MechanicActionBarArsenalSlot(WeaponSlot weaponSlot, Action onClose)
	{
		m_WeaponSlot = weaponSlot;
		m_OnClose = onClose;
	}

	public override void OnClick()
	{
		int activeAbilityId = m_WeaponSlot.AbilityVariants.FindIndex((Ability a) => a.Data == Spell);
		Game.Instance.GameCommandQueue.AddCommand(new SetActiveWeaponIndexGameCommand(m_WeaponSlot, activeAbilityId));
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
