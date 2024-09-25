using System;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickButtonVM : TooltipBaseBrickVM
{
	public readonly string Text;

	private readonly Action m_Callback;

	public TooltipBrickButtonVM(Action callback, string text)
	{
		m_Callback = callback;
		Text = text;
	}

	public void OnClick()
	{
		m_Callback?.Invoke();
	}
}
