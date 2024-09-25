using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickMomentumPortraitVM : TooltipBaseBrickVM
{
	public readonly Sprite Sprite;

	public readonly bool Enable;

	public TooltipBrickMomentumPortraitVM(Sprite sprite, bool enable)
	{
		Sprite = sprite;
		Enable = enable;
	}
}
