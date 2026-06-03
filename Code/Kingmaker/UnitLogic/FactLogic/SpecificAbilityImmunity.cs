using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("c34951c1229c4c64c8288f4d24476433")]
public class SpecificAbilityImmunity : AbilityImmunityComponent, IHashable
{
	[SerializeField]
	private BlueprintAbilityReference[] Abilities;

	public override bool HasImmunityTo(BlueprintAbility ability)
	{
		if (!m_InvertCondition)
		{
			return Abilities.Any((BlueprintAbilityReference a) => a?.Get() == ability);
		}
		return Abilities.All((BlueprintAbilityReference a) => a?.Get() != ability);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
