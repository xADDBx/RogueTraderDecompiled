using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class CustomCompanionCostTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return $"{Game.Instance.Player.GetCustomCompanionCost()}";
	}
}
