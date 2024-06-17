using System;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickButton : ITooltipBrick
{
	private readonly Action m_Callback;

	private readonly string m_Text;

	public TooltipBrickButton(Action callback, string text)
	{
		m_Callback = callback;
		m_Text = text;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickButtonVM(m_Callback, m_Text);
	}
}
