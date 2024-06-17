using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("8abf85d8c6ea04343a2e4fe6bb27a3bb")]
public class ContextActionHealTarget : ContextAction
{
	public ContextDiceValue Value;

	public override string GetCaption()
	{
		return $"Heal {Value} of hit point damage";
	}

	public override void RunAction()
	{
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error(this, "Invalid target for effect '{0}'", GetType().Name);
		}
		else if (base.Context.MaybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
		}
		else
		{
			RuleHealDamage rule = new RuleHealDamage(base.Context.MaybeCaster, base.Target.Entity, new DiceFormula(Value.DiceCountValue.Calculate(base.Context), Value.DiceType), Value.BonusValue.Calculate(base.Context), base.Context.SourceAbilityContext?.Ability);
			base.Context.TriggerRule(rule);
		}
	}

	public HealPredictionData GetHealPrediction([NotNull] AbilityExecutionContext context, [CanBeNull] MechanicEntity target)
	{
		if (context.MaybeCaster == null || target == null)
		{
			return new HealPredictionData();
		}
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			RuleCalculateHeal ruleCalculateHeal = Rulebook.Trigger(new RuleCalculateHeal(context.Caster, target, new DiceFormula(Value.DiceCountValue.Calculate(context), Value.DiceType), Value.BonusValue.Calculate(context)));
			return new HealPredictionData
			{
				Value = ruleCalculateHeal.Value,
				Bonus = ruleCalculateHeal.Bonus
			};
		}
	}
}
