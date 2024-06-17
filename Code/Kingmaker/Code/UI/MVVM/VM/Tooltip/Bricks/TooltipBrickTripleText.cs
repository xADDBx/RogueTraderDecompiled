using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTripleText : TooltipBrickDoubleText
{
	private readonly string m_MiddleLine;

	private readonly Sprite m_LeftIcon;

	private readonly Sprite m_MiddleIcon;

	private readonly Sprite m_RightIcon;

	private readonly TextFieldParams m_LeftParams;

	private readonly TextFieldParams m_MiddleParams;

	private readonly TextFieldParams m_RightParams;

	public TooltipBrickTripleText(string leftLine, string middleLine, string rightLine, Sprite leftIcon = null, Sprite middleIcon = null, Sprite rightIcon = null, TextFieldParams leftLineParams = null, TextFieldParams middleLineParams = null, TextFieldParams rightLineParams = null)
		: base(leftLine, rightLine)
	{
		m_MiddleLine = middleLine;
		m_LeftIcon = leftIcon;
		m_MiddleIcon = middleIcon;
		m_RightIcon = rightIcon;
		m_LeftParams = leftLineParams;
		m_MiddleParams = middleLineParams;
		m_RightParams = rightLineParams;
	}

	public override TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTripleTextVM(m_LeftLine, m_MiddleLine, m_RightLine, m_LeftIcon, m_MiddleIcon, m_RightIcon, m_LeftParams, m_MiddleParams, m_RightParams);
	}
}
