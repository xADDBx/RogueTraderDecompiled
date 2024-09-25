using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickEntityHeaderVM : TooltipBaseBrickVM
{
	public string MainTitle;

	public Sprite Image;

	public string Title;

	public string LeftLabel;

	public string RightLabel;

	public readonly bool HasUpgrade;

	public TooltipBrickEntityHeaderVM()
	{
	}

	public TooltipBrickEntityHeaderVM(string mainTitle, Sprite image, bool hasUpgrade, string title, string leftLabel, string rightLabel)
	{
		MainTitle = mainTitle;
		Image = image;
		HasUpgrade = hasUpgrade;
		Title = title;
		LeftLabel = leftLabel;
		RightLabel = rightLabel;
	}
}
