using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;

namespace Kingmaker.Blueprints.Items.Weapons;

public static class WeaponDamageScaleTable
{
	private class TableData
	{
		public readonly List<WeaponDamageScaleTableRow> Rows;

		public TableData(params WeaponDamageScaleTableRow[] rows)
		{
			Rows = rows.ToList();
		}
	}

	private static readonly TableData Data = new TableData(Row(Item(Size.Fine, D1()), Item(Size.Diminutive, D1()), Item(Size.Tiny, D1()), Item(Size.Small, D1()), Item(Size.Medium, D1()), Item(Size.Large, D(1, DiceType.D2)), Item(Size.Huge, D(1, DiceType.D3)), Item(Size.Gargantuan, D(1, DiceType.D4)), Item(Size.Colossal, D(1, DiceType.D6))), Row(Item(Size.Fine, D1()), Item(Size.Diminutive, D1()), Item(Size.Tiny, D1()), Item(Size.Small, D1()), Item(Size.Medium, D(1, DiceType.D2)), Item(Size.Large, D(1, DiceType.D3)), Item(Size.Huge, D(1, DiceType.D4)), Item(Size.Gargantuan, D(1, DiceType.D6)), Item(Size.Colossal, D(1, DiceType.D8))), Row(Item(Size.Fine, D1()), Item(Size.Diminutive, D1()), Item(Size.Tiny, D1()), Item(Size.Small, D(1, DiceType.D2)), Item(Size.Medium, D(1, DiceType.D3)), Item(Size.Large, D(1, DiceType.D4)), Item(Size.Huge, D(1, DiceType.D6)), Item(Size.Gargantuan, D(1, DiceType.D8)), Item(Size.Colossal, D(2, DiceType.D6))), Row(Item(Size.Fine, D1()), Item(Size.Diminutive, D1()), Item(Size.Tiny, D(1, DiceType.D2)), Item(Size.Small, D(1, DiceType.D3)), Item(Size.Medium, D(1, DiceType.D4)), Item(Size.Large, D(1, DiceType.D6)), Item(Size.Huge, D(1, DiceType.D8)), Item(Size.Gargantuan, D(2, DiceType.D6)), Item(Size.Colossal, D(3, DiceType.D6))), Row(Item(Size.Fine, D1()), Item(Size.Diminutive, D(1, DiceType.D2)), Item(Size.Tiny, D(1, DiceType.D3)), Item(Size.Small, D(1, DiceType.D4)), Item(Size.Medium, D(1, DiceType.D6)), Item(Size.Large, D(1, DiceType.D8)), Item(Size.Huge, D(2, DiceType.D6)), Item(Size.Gargantuan, D(3, DiceType.D6)), Item(Size.Colossal, D(4, DiceType.D6))), Row(Item(Size.Fine, D(1, DiceType.D2)), Item(Size.Diminutive, D(1, DiceType.D3)), Item(Size.Tiny, D(1, DiceType.D4)), Item(Size.Small, D(1, DiceType.D6)), Item(Size.Medium, D(1, DiceType.D8)), Item(Size.Large, D(2, DiceType.D6)), Item(Size.Huge, D(3, DiceType.D6)), Item(Size.Gargantuan, D(4, DiceType.D6)), Item(Size.Colossal, D(6, DiceType.D6))), Row(Item(Size.Fine, D(1, DiceType.D2)), Item(Size.Diminutive, D(1, DiceType.D3)), Item(Size.Tiny, D(1, DiceType.D4)), Item(Size.Small, D(1, DiceType.D6)), Item(Size.Medium, D(2, DiceType.D4)), Item(Size.Large, D(2, DiceType.D6)), Item(Size.Huge, D(3, DiceType.D6)), Item(Size.Gargantuan, D(4, DiceType.D6)), Item(Size.Colossal, D(6, DiceType.D6))), Row(Item(Size.Fine, D(1, DiceType.D3)), Item(Size.Diminutive, D(1, DiceType.D4)), Item(Size.Tiny, D(1, DiceType.D6)), Item(Size.Small, D(1, DiceType.D8)), Item(Size.Medium, D(1, DiceType.D10)), Item(Size.Large, D(2, DiceType.D8)), Item(Size.Huge, D(3, DiceType.D8)), Item(Size.Gargantuan, D(4, DiceType.D8)), Item(Size.Colossal, D(6, DiceType.D8))), Row(Item(Size.Fine, D(1, DiceType.D4)), Item(Size.Diminutive, D(1, DiceType.D6)), Item(Size.Tiny, D(1, DiceType.D8)), Item(Size.Small, D(1, DiceType.D10)), Item(Size.Medium, D(1, DiceType.D12)), Item(Size.Large, D(3, DiceType.D6)), Item(Size.Huge, D(4, DiceType.D6)), Item(Size.Gargantuan, D(6, DiceType.D6)), Item(Size.Colossal, D(8, DiceType.D6))), Row(Item(Size.Fine, D(1, DiceType.D4)), Item(Size.Diminutive, D(1, DiceType.D6)), Item(Size.Tiny, D(1, DiceType.D8)), Item(Size.Small, D(1, DiceType.D10)), Item(Size.Medium, D(2, DiceType.D6)), Item(Size.Large, D(3, DiceType.D6)), Item(Size.Huge, D(4, DiceType.D6)), Item(Size.Gargantuan, D(6, DiceType.D6)), Item(Size.Colossal, D(8, DiceType.D6))), Row(Item(Size.Fine, D(1, DiceType.D4)), Item(Size.Diminutive, D(1, DiceType.D6)), Item(Size.Tiny, D(1, DiceType.D10)), Item(Size.Small, D(1, DiceType.D10)), Item(Size.Medium, D(2, DiceType.D8)), Item(Size.Large, D(3, DiceType.D8)), Item(Size.Huge, D(4, DiceType.D8)), Item(Size.Gargantuan, D(6, DiceType.D8)), Item(Size.Colossal, D(8, DiceType.D8))), Row(Item(Size.Fine, D(1, DiceType.D6)), Item(Size.Diminutive, D(1, DiceType.D8)), Item(Size.Tiny, D(1, DiceType.D10)), Item(Size.Small, D(2, DiceType.D6)), Item(Size.Medium, D(3, DiceType.D6)), Item(Size.Large, D(4, DiceType.D6)), Item(Size.Huge, D(6, DiceType.D6)), Item(Size.Gargantuan, D(8, DiceType.D6)), Item(Size.Colossal, D(12, DiceType.D6))), Row(Item(Size.Fine, D(1, DiceType.D8)), Item(Size.Diminutive, D(1, DiceType.D10)), Item(Size.Tiny, D(2, DiceType.D6)), Item(Size.Small, D(2, DiceType.D8)), Item(Size.Medium, D(2, DiceType.D10)), Item(Size.Large, D(4, DiceType.D8)), Item(Size.Huge, D(6, DiceType.D8)), Item(Size.Gargantuan, D(8, DiceType.D8)), Item(Size.Colossal, D(12, DiceType.D8))), Row(Item(Size.Fine, D(1, DiceType.D8)), Item(Size.Diminutive, D(1, DiceType.D10)), Item(Size.Tiny, D(2, DiceType.D6)), Item(Size.Small, D(3, DiceType.D6)), Item(Size.Medium, D(4, DiceType.D6)), Item(Size.Large, D(6, DiceType.D6)), Item(Size.Huge, D(8, DiceType.D6)), Item(Size.Gargantuan, D(12, DiceType.D6)), Item(Size.Colossal, D(16, DiceType.D6))), Row(Item(Size.Fine, D(1, DiceType.D10)), Item(Size.Diminutive, D(1, DiceType.D10)), Item(Size.Tiny, D(3, DiceType.D6)), Item(Size.Small, D(2, DiceType.D12)), Item(Size.Medium, D(3, DiceType.D10)), Item(Size.Large, D(4, DiceType.D10)), Item(Size.Huge, D(6, DiceType.D10)), Item(Size.Gargantuan, D(8, DiceType.D10)), Item(Size.Colossal, D(12, DiceType.D10))));

	private static List<WeaponDamageScaleTableItem> GetAllScales(DiceFormula dice, Size size)
	{
		for (int i = 0; i < Data.Rows.Count; i++)
		{
			for (int j = 0; j < Data.Rows[i].Items.Count; j++)
			{
				WeaponDamageScaleTableItem weaponDamageScaleTableItem = Data.Rows[i].Items[j];
				if (weaponDamageScaleTableItem.Size == size && weaponDamageScaleTableItem.Dice == dice)
				{
					return Data.Rows[i].Items;
				}
			}
		}
		return new List<WeaponDamageScaleTableItem>();
	}

	public static DiceFormula Scale(DiceFormula baseDice, Size size, Size baseDiceSize = Size.Medium, BlueprintItemWeapon weapon = null)
	{
		if (size == baseDiceSize)
		{
			return baseDice;
		}
		List<WeaponDamageScaleTableItem> allScales = GetAllScales(baseDice, baseDiceSize);
		for (int i = 0; i < allScales.Count; i++)
		{
			if (allScales[i].Size == size)
			{
				return allScales[i].Dice;
			}
		}
		PFLog.Default.Error(weapon, "Can't scale damage: {0} {1}->{2} ({3})", baseDice, baseDiceSize, size, weapon);
		return baseDice;
	}

	private static WeaponDamageScaleTableRow Row(params WeaponDamageScaleTableItem[] items)
	{
		if (!Enum.GetValues(typeof(Size)).Cast<Size>().All((Size v) => items.Count((WeaponDamageScaleTableItem i) => i.Size == v) == 1))
		{
			PFLog.Default.Error("Invalid row in WeaponDamageScaleTable!");
		}
		return new WeaponDamageScaleTableRow(items);
	}

	private static WeaponDamageScaleTableItem Item(Size size, DiceFormula dice)
	{
		return new WeaponDamageScaleTableItem(size, dice);
	}

	private static DiceFormula D1()
	{
		return DiceFormula.One;
	}

	private static DiceFormula D(int rolls, DiceType dice)
	{
		return new DiceFormula(rolls, dice);
	}
}
