using System;
using System.Collections.Generic;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class RtNameTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		string result = "";
		try
		{
			result = Game.Instance.Player.MainCharacterOriginalEntity.CharacterName;
		}
		catch (Exception)
		{
		}
		return result;
	}
}
