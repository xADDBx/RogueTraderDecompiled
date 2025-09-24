using System.Collections.Generic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker;

public class TooltipBrickShipInspectSchemeVM : TooltipBaseBrickVM
{
	public readonly string Title;

	public readonly string ArmorTitle;

	public readonly string ShieldsTitle;

	public readonly string PortTitle;

	public readonly string ForeTitle;

	public readonly string StarboardTitle;

	public readonly string AftTitle;

	public Dictionary<ArmourAndShieldValueType, InspectSchemeValueVM> BrickShipInspectSchemes = new Dictionary<ArmourAndShieldValueType, InspectSchemeValueVM>();

	public TooltipBrickShipInspectSchemeVM(string title, string armorTitle, string shieldsTitle, string portTitle, string foreTitle, string starboardTitle, string aftTitle)
	{
		Title = title;
		ArmorTitle = armorTitle;
		ShieldsTitle = shieldsTitle;
		PortTitle = portTitle;
		ForeTitle = foreTitle;
		StarboardTitle = starboardTitle;
		AftTitle = aftTitle;
	}

	public void AddShipInspectValues(Dictionary<ArmourAndShieldValueType, InspectSchemeValueVM> brickShipInspectSchemes)
	{
		BrickShipInspectSchemes = brickShipInspectSchemes;
	}
}
