using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickAttribute : ITooltipBrick
{
	private readonly string m_Name;

	private readonly string m_Acronym;

	private readonly bool m_IsRecommended;

	private readonly StripeType m_Type;

	private readonly TooltipBaseTemplate m_Tooltip;

	public TooltipBrickAttribute(string name, string acronym, TooltipBaseTemplate tooltip, StripeType type, bool isRecommended)
	{
		m_Name = name;
		m_Acronym = acronym;
		m_Tooltip = tooltip;
		m_Type = type;
		m_IsRecommended = isRecommended;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickAttributeVM(m_Name, m_Acronym, m_Tooltip, m_Type, m_IsRecommended);
	}
}
