using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.UnitLogic.Mechanics;

public static class ContextValueHelper
{
	public static int CalculateDiceValue(DiceType diceType, ContextValue diceCountValue, ContextValue bonusValue, MechanicsContext context)
	{
		int rollsCount = diceCountValue.Calculate(context);
		int num = bonusValue.Calculate(context);
		MechanicEntity mechanicEntity = context.MaybeCaster ?? context.MaybeOwner;
		int num2;
		if (mechanicEntity != null)
		{
			RuleRollDice rule = new RuleRollDice(mechanicEntity, new DiceFormula(rollsCount, diceType));
			num2 = context.TriggerRule(rule).Result;
		}
		else
		{
			PFLog.Default.Error("Caster and owner is missing");
			num2 = 0;
		}
		return num2 + num;
	}
}
