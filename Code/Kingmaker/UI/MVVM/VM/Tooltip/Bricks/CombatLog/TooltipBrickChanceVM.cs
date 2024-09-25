namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickChanceVM : TooltipBrickCombatLogBaseVM
{
	public readonly int SufficientValue;

	public readonly int? CurrentValue;

	public TooltipBrickChanceVM(string name, int sufficientValue, int? currentValue, int nestedLevel, bool isResultValue, string resultValue, bool isProtectionIcon, bool isTargetHitIcon, bool isBorderChanceIcon, bool isGrayBackground, bool isBeigeBackground, bool isRedBackground)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground)
	{
		SufficientValue = sufficientValue;
		CurrentValue = currentValue;
	}
}
