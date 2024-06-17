using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickMinimalAdmissibleDamage : ITooltipBrick
{
	private readonly int m_MinimalAdmissibleDamage;

	public TooltipBrickMinimalAdmissibleDamage(int minimalAdmissibleDamage)
	{
		m_MinimalAdmissibleDamage = minimalAdmissibleDamage;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickMinimalAdmissibleDamageVM(m_MinimalAdmissibleDamage);
	}
}
