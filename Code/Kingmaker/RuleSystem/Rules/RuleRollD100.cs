using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Interfaces;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollD100 : RuleRollDice, IRuleRollD100, IRuleRollDice
{
	public RuleRollD100(IMechanicEntity initiator)
		: base(initiator, new DiceFormula(1, DiceType.D100))
	{
	}

	public RuleRollD100(IMechanicEntity initiator, int resultOverride)
		: base(initiator, new DiceFormula(1, DiceType.D100))
	{
		ResultOverride = resultOverride;
	}

	public static RuleRollD100 FromInt(IMechanicEntity initiator, int roll)
	{
		RuleRollD100 ruleRollD = new RuleRollD100(initiator, roll);
		Rulebook.Trigger(ruleRollD);
		return ruleRollD;
	}
}
