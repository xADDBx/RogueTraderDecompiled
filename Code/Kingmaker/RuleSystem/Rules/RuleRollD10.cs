using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Interfaces;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollD10 : RuleRollDice, IRuleRollD10, IRuleRollDice
{
	public RuleRollD10(IMechanicEntity initiator)
		: base(initiator, new DiceFormula(1, DiceType.D10))
	{
	}

	public RuleRollD10(BaseUnitEntity initiator, int resultOverride)
		: base(initiator, new DiceFormula(1, DiceType.D10))
	{
		ResultOverride = resultOverride;
	}

	public static RuleRollD10 FromInt(BaseUnitEntity initiator, int roll)
	{
		RuleRollD10 ruleRollD = new RuleRollD10(initiator, roll);
		Rulebook.Trigger(ruleRollD);
		return ruleRollD;
	}
}
