using Kingmaker.UI.Models.LevelUp;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Ship;

public class CharGenShipItemVM : SelectionGroupEntityVM
{
	public readonly ChargenUnit ChargenUnit;

	public readonly string Title;

	public readonly string Description;

	public readonly Sprite ShipBigPicture;

	public BlueprintStarship BlueprintStarship => ChargenUnit.Blueprint as BlueprintStarship;

	public CharGenShipItemVM(ChargenUnit chargenUnit)
		: base(allowSwitchOff: false)
	{
		ChargenUnit = chargenUnit;
		Title = BlueprintStarship.PlayerShipName;
		Description = BlueprintStarship.PlayerShipDescription;
		ShipBigPicture = BlueprintStarship.PlayerShipBigPicture;
	}

	protected override void DoSelectMe()
	{
	}
}
