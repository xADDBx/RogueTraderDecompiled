namespace Kingmaker.Code.UI.MVVM.View.Tooltip.PC;

public class ComparativeTooltipPCView : ComparativeTooltipView
{
	private const int MaxTwoRowsTooltipHeight = 495;

	protected override void BindViewImplementation()
	{
		int height = ((base.ViewModel.TooltipVms.Count <= 2) ? base.ViewModel.MainTooltip.MaxHeight : 495);
		for (int i = 0; i < base.ViewModel.TooltipVms.Count - 1; i++)
		{
			base.ViewModel.TooltipVms[i].OverrideMaxHeight(height);
		}
		base.BindViewImplementation();
	}
}
