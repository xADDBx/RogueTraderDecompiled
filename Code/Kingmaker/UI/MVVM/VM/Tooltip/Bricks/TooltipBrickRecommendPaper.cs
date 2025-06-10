using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickRecommendPaper : ITooltipBrick
{
	private string m_FeatureName;

	private bool m_Value;

	public TooltipBrickRecommendPaper(string featureName, bool value)
	{
		m_FeatureName = featureName;
		m_Value = value;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickRecommendPaperVM(m_FeatureName, m_Value);
	}
}
