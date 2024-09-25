using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickChance : TooltipBrickCombatLogBase
{
	private readonly int m_SufficientValue;

	private readonly int? m_CurrentValue;

	public TooltipBrickChance(string name, int sufficientValue, int? currentValue, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, bool isProtectionIcon = false, bool isTargetHitIcon = false, bool isBorderChanceIcon = false, bool isGrayBackground = false, bool isBeigeBackground = false, bool isRedBackground = false)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground)
	{
		m_SufficientValue = sufficientValue;
		m_CurrentValue = currentValue;
	}

	public override TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickChanceVM(m_Name, m_SufficientValue, m_CurrentValue, m_NestedLevel, m_IsResultValue, m_ResultValue, m_IsProtectionIcon, m_IsTargetHitIcon, m_IsBorderChanceIcon, m_IsGrayBackground, m_IsBeigeBackground, m_IsRedBackground);
	}
}
