using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target HP condition")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("8f4044deade58fe459ea7824a0c5900f")]
public class AbilityTargetHPCondition : BlueprintComponent, IAbilityTargetRestriction
{
	public int CurrentHPLessThan;

	public bool Inverted;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		MechanicEntity entity = target.Entity;
		if (entity != null)
		{
			PartHealth healthOptional = entity.GetHealthOptional();
			if (healthOptional != null)
			{
				if (healthOptional.HitPointsLeft < CurrentHPLessThan)
				{
					return !Inverted;
				}
				return Inverted;
			}
		}
		return false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHPCondition;
	}
}
