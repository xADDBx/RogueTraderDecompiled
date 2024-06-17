namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickDamageRangeVM : TooltipBrickCombatLogBaseVM
{
	public readonly int CurrentValue;

	public readonly int MinValue;

	public readonly int MaxValue;

	public TooltipBrickDamageRangeVM(string name, int currentValue, int minValue, int maxValue, int nestedLevel, bool isResultValue, string resultValue, bool isProtectionIcon, bool isTargetHitIcon, bool isBorderChanceIcon, bool isGrayBackground, bool isBeigeBackground, bool isRedBackground)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground)
	{
		CurrentValue = currentValue;
		MinValue = minValue;
		MaxValue = maxValue;
	}
}
