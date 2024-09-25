using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class NameTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return (Game.Instance.DialogController.ActingUnit ?? Game.Instance.Player.MainCharacterEntity).CharacterName;
	}
}
