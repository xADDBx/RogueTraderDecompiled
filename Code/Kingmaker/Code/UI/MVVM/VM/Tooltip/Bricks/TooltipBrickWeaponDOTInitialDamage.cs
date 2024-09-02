using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickWeaponDOTInitialDamage : ITooltipBrick
{
	public readonly BlueprintBuff Buff;

	public readonly int InitialDamage;

	public TooltipBrickWeaponDOTInitialDamage(BlueprintBuff buff, int initialDamage)
	{
		Buff = buff;
		InitialDamage = initialDamage;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickWeaponDOTInitialDamageVM(Buff, InitialDamage);
	}
}
