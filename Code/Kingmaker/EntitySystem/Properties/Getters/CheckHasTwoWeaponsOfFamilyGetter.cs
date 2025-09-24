using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Items;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d2770d0dca214cd8a6053bb2ad891a9b")]
public class CheckHasTwoWeaponsOfFamilyGetter : PropertyGetter
{
	public WeaponFamily Family;

	protected override int GetBaseValue()
	{
		if (base.CurrentEntity.GetBodyOptional()?.PrimaryHand?.MaybeWeapon?.Blueprint?.Family != Family || base.CurrentEntity.GetBodyOptional()?.SecondaryHand?.MaybeWeapon?.Blueprint?.Family != Family)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Checks if current entity has two weapons with selected family";
	}
}
