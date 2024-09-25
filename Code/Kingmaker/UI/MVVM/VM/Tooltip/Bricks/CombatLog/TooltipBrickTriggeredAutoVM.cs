using System.Collections.Generic;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickTriggeredAutoVM : TooltipBaseBrickVM
{
	public readonly string TriggeredAutoText;

	public readonly List<ReasonBuffItemVM> ReasonBuffItems = new List<ReasonBuffItemVM>();

	public readonly bool IsSuccess;

	public TooltipBrickTriggeredAutoVM(string triggeredAutoText, IReadOnlyList<FeatureCountableFlag.BuffList.Element> reasonItems, bool isSuccess)
	{
		TriggeredAutoText = triggeredAutoText;
		IsSuccess = isSuccess;
		if (reasonItems == null)
		{
			return;
		}
		foreach (FeatureCountableFlag.BuffList.Element reasonItem in reasonItems)
		{
			ReasonBuffItemVM item = new ReasonBuffItemVM(reasonItem.BuffInformation);
			ReasonBuffItems.Add(item);
		}
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		ReasonBuffItems.ForEach(delegate(ReasonBuffItemVM item)
		{
			item.Dispose();
		});
		ReasonBuffItems.Clear();
	}
}
