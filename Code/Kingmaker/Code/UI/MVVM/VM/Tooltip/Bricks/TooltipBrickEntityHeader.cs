using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
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

	private readonly string m_RightLabelClassification;

	private readonly bool m_HasUpgrade;

	private readonly bool m_IsAugment;

	private readonly TooltipTemplateItem m_AugmentTooltip;

	public TooltipBrickEntityHeader(string mainTitle, Sprite image, bool hasUpgrade, string title = null, string leftLabel = null, string rightLabel = null, string rightLabelClassification = null, bool isAugment = false, TooltipTemplateItem augmentTooltip = null)
	{
		m_MainTitle = mainTitle;
		m_Image = image;
		m_Title = title;
		m_LeftLabel = leftLabel;
		m_RightLabel = rightLabel;
		m_RightLabelClassification = rightLabelClassification;
		m_HasUpgrade = hasUpgrade;
		m_IsAugment = isAugment;
		m_AugmentTooltip = augmentTooltip;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickEntityHeaderVM(m_MainTitle, m_Image, m_HasUpgrade, m_Title, m_LeftLabel, m_RightLabel, m_RightLabelClassification, m_IsAugment, m_AugmentTooltip);
	}
}
