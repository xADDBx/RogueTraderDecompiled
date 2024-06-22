using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Items;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("39a40d5018244bd498cbbe39fb22c9e3")]
public class CheckHasTwoWeaponsOfClassificationGetter : PropertyGetter
{
	public WeaponClassification Classification;

	protected override int GetBaseValue()
	{
		if (base.CurrentEntity.GetBodyOptional()?.PrimaryHand?.MaybeWeapon?.Blueprint?.Classification != Classification || base.CurrentEntity.GetBodyOptional()?.SecondaryHand?.MaybeWeapon?.Blueprint?.Classification != Classification)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Checks if current entity has two weapons with selected classification";
	}
}
