using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("6667ad881b0ea6c4fab5ef5b77490081")]
public class WarhammerAbilityManageResources : BlueprintComponent, IAbilityCasterRestriction, IAbilityOnCastLogic
{
	public bool CostsMaximumMovePoints;

	[HideIf("CostsMaximumMovePoints")]
	public int mustHaveMovePoints;

	[HideIf("CostsMaximumMovePoints")]
	public int shouldSpendMovePoints;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		PartUnitCombatState combatStateOptional = caster.GetCombatStateOptional();
		if (combatStateOptional == null)
		{
			return true;
		}
		int num = (CostsMaximumMovePoints ? Rulebook.Trigger(new RuleCalculateMovementPoints(caster)).Result : mustHaveMovePoints);
		return combatStateOptional.ActionPointsBlue >= (float)num;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.NoResources;
	}

	public void OnCast(AbilityExecutionContext context)
	{
		PartUnitCombatState combatStateOptional = context.Caster.GetCombatStateOptional();
		if (combatStateOptional != null)
		{
			int num = (CostsMaximumMovePoints ? Rulebook.Trigger(new RuleCalculateMovementPoints(context.Caster)).Result : shouldSpendMovePoints);
			float? blue = num;
			combatStateOptional.SpendActionPoints(null, blue);
		}
	}
}
