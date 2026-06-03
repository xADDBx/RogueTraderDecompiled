using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("39397a5a3851be2449b106a516288e2d")]
public class AbilityGroupImmunity : AbilityImmunityComponent, IHashable
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	public override bool HasImmunityTo(BlueprintAbility blueprint)
	{
		ReferenceArrayProxy<BlueprintAbilityGroup> abilityGroups = blueprint.AbilityGroups;
		if (!m_InvertCondition)
		{
			return abilityGroups.Any((BlueprintAbilityGroup p) => Groups.Contains(p));
		}
		return abilityGroups.All((BlueprintAbilityGroup p) => !Groups.Contains(p));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
