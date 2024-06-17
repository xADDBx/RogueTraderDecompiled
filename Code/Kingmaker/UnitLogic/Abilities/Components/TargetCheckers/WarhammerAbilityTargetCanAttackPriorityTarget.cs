using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target has fact")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("a0c18085e5c74579bca2ea0f0fb34e8c")]
public class WarhammerAbilityTargetCanAttackPriorityTarget : BlueprintComponent, IAbilityTargetRestriction
{
	[SerializeField]
	private BlueprintBuffReference m_TargetBuff;

	public WarhammerContextActionAttackPriorityTarget.PriorityTargetAttackSelectType AttackSelectType;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		MechanicEntity entity = target.Entity;
		if (entity == null)
		{
			return false;
		}
		BaseUnitEntity baseUnitEntity = ability.Caster.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
		if (baseUnitEntity == null)
		{
			return false;
		}
		if (WarhammerContextActionAttackPriorityTarget.SelectAttackAbility(entity, baseUnitEntity, AttackSelectType) == null)
		{
			return false;
		}
		if (LosCalculations.GetWarhammerLos(ability.Caster, casterPosition, baseUnitEntity) == LosCalculations.CoverType.Invisible)
		{
			return false;
		}
		return true;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		BaseUnitEntity baseUnitEntity = ability.Caster.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
		if (baseUnitEntity == null)
		{
			return BlueprintRoot.Instance.LocalizedTexts.Reasons.HasNoPriorityTarget;
		}
		MechanicEntity entity = target.Entity;
		if (entity == null || WarhammerContextActionAttackPriorityTarget.SelectAttackAbility(entity, baseUnitEntity, AttackSelectType) == null)
		{
			return BlueprintRoot.Instance.LocalizedTexts.Reasons.HasNoAttacksForPriorityTarget;
		}
		if (LosCalculations.GetWarhammerLos(ability.Caster, casterPosition, baseUnitEntity) == LosCalculations.CoverType.Invisible)
		{
			return BlueprintRoot.Instance.LocalizedTexts.Reasons.HasNoLosToPriorityTarget;
		}
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.HasNoAttacksForPriorityTarget;
	}
}
