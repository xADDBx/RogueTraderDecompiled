using System.Collections.Generic;

namespace Kingmaker.RuleSystem.Rules.Interfaces;

public interface IRuleRollDice
{
	List<int> RollHistory { get; }

	bool ReplacedOne { get; }

	int Result { get; }

	bool ReplaceOneWithMax { get; set; }

	List<RerollData> Rerolls { get; }
}
