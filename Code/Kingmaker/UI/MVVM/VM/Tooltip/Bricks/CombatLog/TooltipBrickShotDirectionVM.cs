using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickShotDirectionVM : TooltipBaseBrickVM
{
	public readonly int DeviationMin;

	public readonly int DeviationMax;

	public readonly int DeviationValue;

	public TooltipBrickShotDirectionVM(int deviationMin, int deviationMax, int deviationValue)
	{
		DeviationMin = deviationMin;
		DeviationMax = deviationMax;
		DeviationValue = deviationValue;
	}
}
