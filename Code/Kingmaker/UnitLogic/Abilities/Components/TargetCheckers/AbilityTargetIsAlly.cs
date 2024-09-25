using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target has fact")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("04f031e209c83d74e94fd97295f30aea")]
public class AbilityTargetIsAlly : BlueprintComponent, IAbilityTargetRestriction
{
	public bool Not;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (!Not)
		{
			if (target.Entity != null)
			{
				return target.Entity.IsAlly(ability.Caster);
			}
			return false;
		}
		if (target.Entity != null)
		{
			return !target.Entity.IsAlly(ability.Caster);
		}
		return true;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Not ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsEnemy : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsAlly;
	}
}
