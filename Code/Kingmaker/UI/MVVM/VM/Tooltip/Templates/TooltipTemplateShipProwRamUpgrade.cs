using System.Collections.Generic;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateShipProwRamUpgrade : TooltipTemplateShipUpgradeBase
{
	public TooltipTemplateShipProwRamUpgrade(string header, string description)
		: base(header, description)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		ProwRam prowRam = Game.Instance.Player.PlayerShip.Hull.ProwRam;
		CanUpgrade = prowRam.IsEnoughScrap;
		IsMaxLevel = prowRam.IsMaxLevel;
		return base.GetHeader(type);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		PartStarshipHull hull = Game.Instance.Player.PlayerShip.Hull;
		CurrentUpgradeLevel = UIUtility.ArabicToRoman(Game.Instance.Player.PlayerShip.Hull.ProwRam.UpgradeLevel);
		CurrentUpgradeCost = hull.ProwRam.Blueprint.UpgradeCost[(hull.ProwRam.Blueprint.UpgradeCost.Length <= hull.ProwRam.UpgradeLevel + 1) ? hull.ProwRam.UpgradeLevel : (hull.ProwRam.UpgradeLevel + 1)].ToString();
		return base.GetBody(type);
	}
}
