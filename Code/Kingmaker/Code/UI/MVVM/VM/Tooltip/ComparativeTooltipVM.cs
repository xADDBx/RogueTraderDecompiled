using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip;

public class ComparativeTooltipVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public List<TooltipVM> TooltipVms = new List<TooltipVM>();

	public ComparativeTooltipVM(IEnumerable<TooltipData> datas)
	{
		foreach (TooltipData data in datas)
		{
			TooltipVM tooltipVM = new TooltipVM(data, isComparative: true);
			AddDisposable(tooltipVM);
			TooltipVms.Add(tooltipVM);
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
