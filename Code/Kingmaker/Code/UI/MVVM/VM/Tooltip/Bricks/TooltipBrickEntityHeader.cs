using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickEntityHeader : ITooltipBrick
{
	private readonly string m_MainTitle;

	private readonly Sprite m_Image;

	private readonly string m_Title;

	private readonly string m_LeftLabel;

	private readonly string m_RightLabel;

	private readonly bool m_HasUpgrade;

	public TooltipBrickEntityHeader(string mainTitle, Sprite image, bool hasUpgrade, string title = null, string leftLabel = null, string rightLabel = null)
	{
		m_MainTitle = mainTitle;
		m_Image = image;
		m_Title = title;
		m_LeftLabel = leftLabel;
		m_RightLabel = rightLabel;
		m_HasUpgrade = hasUpgrade;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickEntityHeaderVM(m_MainTitle, m_Image, m_HasUpgrade, m_Title, m_LeftLabel, m_RightLabel);
	}
}
