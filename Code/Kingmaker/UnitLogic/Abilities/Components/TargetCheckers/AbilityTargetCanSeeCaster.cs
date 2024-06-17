using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target can see caster")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("8507226f5bdd6a24bae48dcbcbfe0958")]
public class AbilityTargetCanSeeCaster : BlueprintComponent, IAbilityTargetRestriction
{
	public bool Not;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		PartCombatGroup partCombatGroup = target.Entity?.GetCombatGroupOptional();
		if (partCombatGroup != null)
		{
			bool flag = partCombatGroup.Memory.ContainsVisible(ability.Caster);
			if (!Not)
			{
				return flag;
			}
			return !flag;
		}
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Not ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetCanNotSeeCaster : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetCanSeeCaster;
	}
}
