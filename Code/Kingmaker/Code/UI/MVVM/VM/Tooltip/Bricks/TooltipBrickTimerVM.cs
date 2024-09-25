using System;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTimerVM : TooltipBaseBrickVM
{
	public readonly StringReactiveProperty Text = new StringReactiveProperty();

	public readonly bool ShowTimeIcon;

	public TooltipBrickTimerVM(Func<string> fs, bool showIcon)
	{
		TooltipBrickTimerVM tooltipBrickTimerVM = this;
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			tooltipBrickTimerVM.Text.Value = fs?.Invoke();
		}));
		ShowTimeIcon = showIcon;
	}
}
