using System.Collections.Generic;
using Kingmaker.TextTools.Base;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.TextTools;

public class PetNameTemplate : TextTemplate
{
	public override string Generate(bool capitalized, List<string> parameters)
	{
		return ((Game.Instance.DialogController.ActingUnit ?? Game.Instance.Player.MainCharacterEntity).GetOptional<UnitPartPetOwner>()?.PetUnit)?.Name ?? string.Empty;
	}
}
