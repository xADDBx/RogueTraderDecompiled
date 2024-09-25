using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class RaceTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		string name = Game.Instance.Player.MainCharacterEntity.Progression.Race.Name;
		if (!capitalized)
		{
			return name.ToLowerInvariant();
		}
		return name;
	}
}
