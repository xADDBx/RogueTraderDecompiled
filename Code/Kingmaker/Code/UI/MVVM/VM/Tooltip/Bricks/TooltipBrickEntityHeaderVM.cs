using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickEntityHeaderVM : TooltipBaseBrickVM
{
	public readonly string MainTitle;

	public readonly Sprite Image;

	public readonly string Title;

	public readonly string LeftLabel;

	public readonly string RightLabel;

	public readonly string RightLabelClassification;

	public readonly bool HasUpgrade;

	public TooltipBrickEntityHeaderVM()
	{
	}

	public TooltipBrickEntityHeaderVM(string mainTitle, Sprite image, bool hasUpgrade, string title, string leftLabel, string rightLabel, string rightLabelClassification)
	{
		MainTitle = mainTitle;
		Image = image;
		HasUpgrade = hasUpgrade;
		Title = title;
		LeftLabel = leftLabel;
		RightLabel = rightLabel;
		RightLabelClassification = rightLabelClassification;
	}
}
