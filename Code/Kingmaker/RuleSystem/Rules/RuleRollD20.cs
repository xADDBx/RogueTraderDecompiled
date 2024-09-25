using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollD20 : RuleRollDice
{
	public RuleRollD20(IMechanicEntity initiator)
		: base(initiator, new DiceFormula(1, DiceType.D20))
	{
	}

	public RuleRollD20(IMechanicEntity initiator, int resultOverride)
		: base(initiator, new DiceFormula(1, DiceType.D20))
	{
		ResultOverride = resultOverride;
	}

	public static RuleRollD20 FromInt(IMechanicEntity initiator, int roll)
	{
		RuleRollD20 ruleRollD = new RuleRollD20(initiator, roll);
		Rulebook.Trigger(ruleRollD);
		return ruleRollD;
	}
}
