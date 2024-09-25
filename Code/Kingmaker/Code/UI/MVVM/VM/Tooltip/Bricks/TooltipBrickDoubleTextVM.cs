using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickDoubleTextVM : TooltipBaseBrickVM
{
	public readonly string LeftLine;

	public readonly string RightLine;

	public readonly TextAnchor LeftAlignment;

	public readonly TextAnchor RightAlignment;

	public TooltipBrickDoubleTextVM(string leftLine, string rightLine, TextAnchor leftAlignment = TextAnchor.MiddleCenter, TextAnchor rightAlignment = TextAnchor.MiddleCenter)
	{
		LeftLine = leftLine;
		RightLine = rightLine;
		LeftAlignment = leftAlignment;
		RightAlignment = rightAlignment;
	}
}
