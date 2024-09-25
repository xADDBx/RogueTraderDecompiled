using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[TypeId("05f0b74176f345c58e301d05bf287b09")]
public class AbilityTargetPropheticIntervention : BlueprintComponent, IAbilityTargetRestriction
{
	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return ability.Caster.Parts.GetOptional<UnitPartPropheticIntervention>()?.HasEntry(target.Entity as UnitEntity) ?? false;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsNotPropheticIntervention;
	}
}
