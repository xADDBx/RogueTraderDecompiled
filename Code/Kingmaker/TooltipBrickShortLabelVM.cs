using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker;

public class TooltipBrickShortLabelVM : TooltipBaseBrickVM
{
	public ReactiveProperty<string> StatShortName = new ReactiveProperty<string>();

	public TooltipBrickShortLabelVM(string statShortName)
	{
		StatShortName.Value = statShortName;
	}
}
