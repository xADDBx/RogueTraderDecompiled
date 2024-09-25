using System;
using Owlcat.Runtime.UI.ConsoleTools.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool.TMPLinkNavigation;

public class TMPLinkNavigationEntityAdvanced<TLinkData> : TMPLinkNavigationEntity
{
	private readonly TLinkData m_LinkData;

	private readonly Action<string, TLinkData> m_OnLinkClicked;

	private readonly Action<string, TLinkData> m_OnLinkFocused;

	private readonly Func<string, TLinkData, TooltipBaseTemplate> m_GetTooltipTemplate;

	public TMPLinkNavigationEntityAdvanced(TextMeshProUGUI text, OwlcatMultiButton firstFocus, OwlcatMultiButton secondFocus, GlossaryPoint glossaryPoint, TLinkData linkData, Action<string, TLinkData> onLinkClicked, Action<string, TLinkData> onLinkFocused, Func<string, TLinkData, TooltipBaseTemplate> getTooltipTemplate)
		: base(text, firstFocus, secondFocus, glossaryPoint)
	{
		m_LinkData = linkData;
		m_OnLinkClicked = onLinkClicked;
		m_OnLinkFocused = onLinkFocused;
		m_GetTooltipTemplate = getTooltipTemplate;
	}

	protected override void OnLinkClicked()
	{
		m_OnLinkClicked?.Invoke(m_LinkId, m_LinkData);
	}

	protected override void OnLinkFocused()
	{
		m_OnLinkFocused?.Invoke(m_LinkId, m_LinkData);
	}

	public override TooltipBaseTemplate TooltipTemplate()
	{
		return m_GetTooltipTemplate?.Invoke(m_LinkId, m_LinkData);
	}
}
