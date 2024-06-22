using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickMinimalAdmissibleDamageVM : TooltipBaseBrickVM
{
	public readonly int MinimalAdmissibleDamage;

	public readonly string ReasonValue;

	public TooltipBrickMinimalAdmissibleDamageVM(int minimalAdmissibleDamage, string reasonValue)
	{
		MinimalAdmissibleDamage = minimalAdmissibleDamage;
		ReasonValue = reasonValue;
	}
}
