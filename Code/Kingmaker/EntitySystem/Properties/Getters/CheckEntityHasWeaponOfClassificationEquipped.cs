using System;
using System.Linq;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("63a9f48cb67c49dfb3de92d216d8adbb")]
public class CheckEntityHasWeaponOfClassificationEquipped : PropertyGetter, PropertyContextAccessor.IOptionalFact, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WeaponClassification Classification;

	public bool IsOneHanded;

	public bool IsTwoHanded;

	protected override int GetBaseValue()
	{
		PartUnitBody bodyOptional = base.CurrentEntity.GetBodyOptional();
		if (bodyOptional == null)
		{
			return 0;
		}
		if (!bodyOptional.Hands.Any((HandSlot p) => CheckWeapon(p.MaybeWeapon?.Blueprint)))
		{
			return 0;
		}
		return 1;
	}

	public bool CheckWeapon(BlueprintItemWeapon weapon)
	{
		if (weapon == null)
		{
			return false;
		}
		if (weapon.Classification != Classification)
		{
			return false;
		}
		if (IsOneHanded && weapon.IsTwoHanded)
		{
			return false;
		}
		if (IsTwoHanded && !weapon.IsTwoHanded)
		{
			return false;
		}
		return true;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current entity has a proper weapon equipped";
	}
}
