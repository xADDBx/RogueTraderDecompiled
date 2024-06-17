using System.Collections.Generic;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateShipInternalStructureUpgrade : TooltipTemplateShipUpgradeBase
{
	public TooltipTemplateShipInternalStructureUpgrade(string header, string description)
		: base(header, description)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		InternalStructure internalStructure = Game.Instance.Player.PlayerShip.Hull.InternalStructure;
		CanUpgrade = internalStructure.IsEnoughScrap;
		IsMaxLevel = internalStructure.IsMaxLevel;
		return base.GetHeader(type);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		PartStarshipHull hull = Game.Instance.Player.PlayerShip.Hull;
		CurrentUpgradeLevel = UIUtility.ArabicToRoman(Game.Instance.Player.PlayerShip.Hull.InternalStructure.UpgradeLevel);
		CurrentUpgradeCost = hull.InternalStructure.Blueprint.UpgradeCost[(hull.InternalStructure.Blueprint.UpgradeCost.Length <= hull.InternalStructure.UpgradeLevel + 1) ? hull.InternalStructure.UpgradeLevel : (hull.InternalStructure.UpgradeLevel + 1)].ToString();
		return base.GetBody(type);
	}
}
