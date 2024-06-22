using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip;

public class ComparativeTooltipVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<TooltipVM> TooltipVms = new List<TooltipVM>();

	public TooltipVM MainTooltip => TooltipVms.LastOrDefault();

	public TooltipVM FirstCompareTooltip => TooltipVms.FirstOrDefault();

	public RectTransform MainOwnerTransform => MainTooltip?.OwnerTransform;

	public RectTransform ComparativeOwnerTransform => FirstCompareTooltip?.OwnerTransform;

	public ComparativeTooltipVM(IEnumerable<TooltipData> tooltipsData, bool showScrollbar)
	{
		foreach (TooltipData tooltipsDatum in tooltipsData)
		{
			TooltipVM tooltipVM = new TooltipVM(tooltipsDatum, isComparative: true, shouldNotHideLittleTooltip: false, showScrollbar);
			AddDisposable(tooltipVM);
			TooltipVms.Add(tooltipVM);
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
