using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

internal class SavingThrowData : ContextData<SavingThrowData>
{
	public RulePerformSavingThrow Rule { get; private set; }

	public SavingThrowData Setup(RulePerformSavingThrow rule)
	{
		Rule = rule;
		return this;
	}

	protected override void Reset()
	{
		Rule = null;
	}
}
