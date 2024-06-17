using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickMomentumPortrait : ITooltipBrick
{
	private readonly Sprite m_Sprite;

	private readonly bool m_Enable;

	public TooltipBrickMomentumPortrait(Sprite sprite, bool enable)
	{
		m_Sprite = sprite;
		m_Enable = enable;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickMomentumPortraitVM(m_Sprite, m_Enable);
	}
}
