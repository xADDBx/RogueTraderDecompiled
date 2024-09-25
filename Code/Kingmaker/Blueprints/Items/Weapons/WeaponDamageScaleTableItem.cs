using Kingmaker.Enums;
using Kingmaker.RuleSystem;

namespace Kingmaker.Blueprints.Items.Weapons;

public struct WeaponDamageScaleTableItem
{
	public readonly Size Size;

	public readonly DiceFormula Dice;

	public WeaponDamageScaleTableItem(Size size, DiceFormula dice)
	{
		Size = size;
		Dice = dice;
	}
}
