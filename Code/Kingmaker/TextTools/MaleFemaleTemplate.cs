using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.TextTools;

public class MaleFemaleTemplate : TextTemplate
{
	public override int MinParameters => 2;

	public override int MaxParameters => 4;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		int num = (int)(((MechanicEntity)((GameLogContext.InScope ? GameLogContext.SourceEntity.Value : null) ?? Game.Instance.DialogController.ActingUnit ?? Game.Instance.Player.MainCharacterEntity))?.GetDescriptionOptional()?.Gender).GetValueOrDefault();
		if (parameters.Count > 2 && Game.Instance.Player.PlayerIsKing)
		{
			num += 2;
		}
		if (parameters.Count > num)
		{
			return parameters[num];
		}
		return "";
	}
}
