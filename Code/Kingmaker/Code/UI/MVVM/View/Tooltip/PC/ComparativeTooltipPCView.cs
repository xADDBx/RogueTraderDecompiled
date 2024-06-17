using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.PC;

public class ComparativeTooltipPCView : ComparativeTooltipView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(DelayedInvoker.InvokeInTime(delegate
		{
			Show(base.ViewModel.TooltipVms.FirstOrDefault()?.PriorityPivots);
		}, 0.2f));
	}
}
