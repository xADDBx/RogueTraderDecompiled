using Kingmaker.UnitLogic.Mechanics.Facts.Interfaces;

namespace Kingmaker.RuleSystem.Rules.Interfaces;

public struct RerollData
{
	public readonly int Amount;

	public readonly IMechanicEntityFact Source;

	public RerollData(int amount, IMechanicEntityFact source)
	{
		Amount = amount;
		Source = source;
	}
}
