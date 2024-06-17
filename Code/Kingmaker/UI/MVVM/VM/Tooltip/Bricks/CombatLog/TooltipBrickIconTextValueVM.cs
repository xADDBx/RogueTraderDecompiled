namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickIconTextValueVM : TooltipBrickCombatLogBaseVM
{
	public readonly string Value;

	public TooltipBrickIconTextValueVM(string name, string value, int nestedLevel, bool isResultValue, string resultValue, bool isProtectionIcon, bool isTargetHitIcon, bool isBorderChanceIcon, bool isGrayBackground, bool isBeigeBackground, bool isRedBackground)
		: base(name, nestedLevel, isResultValue, resultValue, isProtectionIcon, isTargetHitIcon, isBorderChanceIcon, isGrayBackground, isBeigeBackground, isRedBackground)
	{
		Value = value;
	}
}
