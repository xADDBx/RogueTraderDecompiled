using System.Collections.Generic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker;

public class TooltipBrickShipInspectScheme : ITooltipBrick
{
	private readonly string m_Title;

	private readonly string m_ArmorTitle;

	private readonly string m_ShieldsTitle;

	private readonly string m_PortTitle;

	private readonly string m_ForeTitle;

	private readonly string m_StarboardTitle;

	private readonly string m_AftTitle;

	private readonly Dictionary<ArmourAndShieldValueType, InspectSchemeValueVM> m_BrickShipInspectSchemes = new Dictionary<ArmourAndShieldValueType, InspectSchemeValueVM>();

	public TooltipBrickShipInspectScheme(string title, string armorTitle, string shieldsTitle, string portTitle, string foreTitle, string starboardTitle, string aftTitle)
	{
		m_Title = title;
		m_ArmorTitle = armorTitle;
		m_ShieldsTitle = shieldsTitle;
		m_PortTitle = portTitle;
		m_ForeTitle = foreTitle;
		m_StarboardTitle = starboardTitle;
		m_AftTitle = aftTitle;
	}

	public TooltipBaseBrickVM GetVM()
	{
		TooltipBrickShipInspectSchemeVM tooltipBrickShipInspectSchemeVM = new TooltipBrickShipInspectSchemeVM(m_Title, m_ArmorTitle, m_ShieldsTitle, m_PortTitle, m_ForeTitle, m_StarboardTitle, m_AftTitle);
		tooltipBrickShipInspectSchemeVM.AddShipInspectValues(m_BrickShipInspectSchemes);
		return tooltipBrickShipInspectSchemeVM;
	}

	public void SetArmourValue(ArmourAndShieldValueType type, string value, TooltipBaseTemplate tooltip)
	{
		InspectSchemeValueVM value2 = new InspectSchemeValueVM(type, value, tooltip);
		m_BrickShipInspectSchemes[type] = value2;
	}
}
