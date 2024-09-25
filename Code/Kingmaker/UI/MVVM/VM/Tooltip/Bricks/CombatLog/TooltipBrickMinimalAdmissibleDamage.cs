using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickMinimalAdmissibleDamage : ITooltipBrick
{
	private readonly int m_MinimalAdmissibleDamage;

	private readonly string m_ReasonValue;

	public TooltipBrickMinimalAdmissibleDamage(int minimalAdmissibleDamage, string reasonValue)
	{
		m_MinimalAdmissibleDamage = minimalAdmissibleDamage;
		m_ReasonValue = reasonValue;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickMinimalAdmissibleDamageVM(m_MinimalAdmissibleDamage, m_ReasonValue);
	}
}
