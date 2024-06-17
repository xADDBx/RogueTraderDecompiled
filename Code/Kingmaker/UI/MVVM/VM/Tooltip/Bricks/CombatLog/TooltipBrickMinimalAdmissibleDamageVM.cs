using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickMinimalAdmissibleDamageVM : TooltipBaseBrickVM
{
	public readonly int MinimalAdmissibleDamage;

	public TooltipBrickMinimalAdmissibleDamageVM(int minimalAdmissibleDamage)
	{
		MinimalAdmissibleDamage = minimalAdmissibleDamage;
	}
}
