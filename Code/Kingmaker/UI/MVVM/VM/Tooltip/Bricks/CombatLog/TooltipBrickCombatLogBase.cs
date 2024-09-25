using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public abstract class TooltipBrickCombatLogBase : ITooltipBrick
{
	protected readonly string m_Name;

	protected readonly int m_NestedLevel;

	protected readonly bool m_IsResultValue;

	protected readonly string m_ResultValue;

	protected readonly bool m_IsProtectionIcon;

	protected readonly bool m_IsTargetHitIcon;

	protected readonly bool m_IsBorderChanceIcon;

	protected readonly bool m_IsGrayBackground;

	protected readonly bool m_IsBeigeBackground;

	protected readonly bool m_IsRedBackground;

	public TooltipBrickCombatLogBase(string name, int nestedLevel = 0, bool isResultValue = false, string resultValue = null, bool isProtectionIcon = false, bool isTargetHitIcon = false, bool isBorderChanceIcon = false, bool isGrayBackground = false, bool isBeigeBackground = false, bool isRedBackground = false)
	{
		m_Name = name;
		m_NestedLevel = nestedLevel;
		m_IsResultValue = isResultValue;
		m_ResultValue = resultValue;
		m_IsProtectionIcon = isProtectionIcon;
		m_IsTargetHitIcon = isTargetHitIcon;
		m_IsBorderChanceIcon = isBorderChanceIcon;
		m_IsGrayBackground = isGrayBackground;
		m_IsBeigeBackground = isBeigeBackground;
		m_IsRedBackground = isRedBackground;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return null;
	}
}
