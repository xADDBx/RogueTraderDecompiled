using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickDamageRange : TooltipBrickCombatLogBase
{
	private readonly int m_CurrentValue;

	private readonly int m_MinValue;

	private readonly int m_MaxValue;

	public TooltipBrickDamageRange(string name, int currentValue, int minValue, int maxValue, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, bool isProtectionIcon = false, bool isTargetHitIcon = false, bool isBorderChanceIcon = false, bool isGrayBackground = false, bool isBeigeBackground = false, bool isRedBackground = false)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground)
	{
		m_CurrentValue = currentValue;
		m_MinValue = minValue;
		m_MaxValue = maxValue;
	}

	public override TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickDamageRangeVM(m_Name, m_CurrentValue, m_MinValue, m_MaxValue, m_NestedLevel, m_IsResultValue, m_ResultValue, m_IsProtectionIcon, m_IsTargetHitIcon, m_IsBorderChanceIcon, m_IsGrayBackground, m_IsBeigeBackground, m_IsRedBackground);
	}
}
