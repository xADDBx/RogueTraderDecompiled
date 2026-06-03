using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarSlotAugmentsOverchargeAbility : MechanicActionBarSlotAbility, IHashable
{
	public sealed override AbilityData Ability { get; set; }

	public MechanicActionBarSlotAugmentsOverchargeAbility(Ability ability, BaseUnitEntity owner)
	{
		Ability = ability.Data;
		base.Unit = owner;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
