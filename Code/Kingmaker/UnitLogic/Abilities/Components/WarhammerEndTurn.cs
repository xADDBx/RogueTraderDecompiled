using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("224c1eb24ed98e942ae15b9e10e62724")]
public class WarhammerEndTurn : BlueprintComponent, IAbilityOnCastLogic
{
	public bool clearMPInsteadOfEndingTurn;

	public ConditionsChecker Condition;

	[SerializeField]
	private BlueprintBuffReference m_BuffToCaster;

	public BlueprintBuff BuffToCaster => m_BuffToCaster?.Get();

	public void OnCast(AbilityExecutionContext context)
	{
		using (context.GetDataScope(context.Caster.ToITargetWrapper()))
		{
			if (Condition != null && !Condition.Check() && Condition.HasConditions)
			{
				return;
			}
		}
		if (context.Ability.IsAttackOfOpportunity)
		{
			return;
		}
		context.Caster.Buffs.Add(BuffToCaster, context);
		if (clearMPInsteadOfEndingTurn)
		{
			PartUnitCombatState combatStateOptional = context.Caster.GetCombatStateOptional();
			if (combatStateOptional != null && !(context.MaybeCaster?.Features.DoNotResetMovementPointsOnAttacks))
			{
				if (combatStateOptional.SaveMPAfterUsingNextAbility)
				{
					combatStateOptional.SaveMPAfterUsingNextAbility = false;
				}
				else
				{
					combatStateOptional.SpendActionPointsAll(yellow: false, blue: true);
				}
			}
		}
		else if (Game.Instance.TurnController.CurrentUnit == context.Caster)
		{
			Game.Instance.TurnController.RequestEndTurn();
		}
	}
}
