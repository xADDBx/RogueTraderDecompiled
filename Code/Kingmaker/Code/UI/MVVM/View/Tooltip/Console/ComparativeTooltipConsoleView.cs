using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console;

public class ComparativeTooltipConsoleView : ComparativeTooltipView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		List<Vector2> pivots = base.ViewModel.TooltipVms.FirstOrDefault()?.PriorityPivots;
		AddDisposable(DelayedInvoker.InvokeInTime(delegate
		{
			Show(pivots);
		}, 0.2f));
	}
}
