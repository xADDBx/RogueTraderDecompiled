using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateShipUpgradeBase : TooltipBaseTemplate
{
	protected bool CanUpgrade;

	protected bool IsMaxLevel;

	protected string Header;

	protected string Description;

	protected string CurrentUpgradeLevel;

	protected string CurrentUpgradeCost;

	public TooltipTemplateShipUpgradeBase(string header, string description)
	{
		Header = header;
		Description = description;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickTitle(Header));
		if (IsMaxLevel)
		{
			list.Add(new TooltipBrickTextBackground(UIStrings.Instance.ShipCustomization.AlreadyMaximum, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: false, 18, isGrayBackground: false, isGreenBackground: false, isRedBackground: true));
		}
		else if (CanUpgrade)
		{
			list.Add(new TooltipBrickTextBackground(UIStrings.Instance.ShipCustomization.CanUpgrade, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: false, 18, isGrayBackground: false, isGreenBackground: true));
		}
		else
		{
			list.Add(new TooltipBrickTextBackground(UIStrings.Instance.ShipCustomization.CantUpgrade, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: false, 18, isGrayBackground: false, isGreenBackground: false, isRedBackground: true));
		}
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickIconValueStat(UIStrings.Instance.ShipCustomization.CurrentUpgradeLevel.Text, CurrentUpgradeLevel, null, TooltipIconValueStatType.Normal, isWhite: false, needChangeSize: true, 18, 22),
			new TooltipBrickIconValueStat(UIStrings.Instance.ShipCustomization.CurrentShipUpgradeScrapValue, CurrentUpgradeCost, null, TooltipIconValueStatType.Normal, isWhite: false, needChangeSize: true, 18, 22),
			new TooltipBrickSeparator(TooltipBrickElementType.Medium),
			new TooltipBrickText(Description, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left)
		};
	}
}
