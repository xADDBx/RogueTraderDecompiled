using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickRecommendPaperVM : TooltipBaseBrickVM
{
	public readonly string FeatureName;

	public readonly bool Value;

	public TooltipBrickRecommendPaperVM(string featureName, bool value)
	{
		FeatureName = featureName;
		Value = value;
	}
}
