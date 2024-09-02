using System;
using System.Collections.Generic;
using Kingmaker.TextTools.Base;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.TextTools;

public class RtMaleFemaleTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		string result = "";
		try
		{
			int num = (int)(Game.Instance.Player.MainCharacterOriginalEntity?.GetDescriptionOptional()?.Gender).GetValueOrDefault();
			if (parameters.Count > 2 && Game.Instance.Player.PlayerIsKing)
			{
				num += 2;
			}
			if (parameters.Count > num)
			{
				result = parameters[num];
			}
		}
		catch (Exception)
		{
		}
		return result;
	}
}
