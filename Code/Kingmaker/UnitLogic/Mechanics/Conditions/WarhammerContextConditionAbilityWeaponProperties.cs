using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("a81d559efc13c8647b43b186679bdf7c")]
public class WarhammerContextConditionAbilityWeaponProperties : ContextCondition
{
	public bool isMelee;

	public bool isCanBurst;

	public bool checkCategory;

	[SerializeField]
	[ShowIf("checkCategory")]
	public WeaponCategory[] Categories;

	public bool checkOnOwner;

	public bool anyHand;

	public bool bothHands;

	protected override string GetConditionCaption()
	{
		return "Check if equipped weapon has required property";
	}

	protected override bool CheckCondition()
	{
		ItemEntityWeapon itemEntityWeapon = ((!checkOnOwner) ? base.Target.Entity?.GetPrimaryHandWeapon() : base.Context.MaybeOwner?.GetPrimaryHandWeapon());
		bool flag = false;
		if (anyHand || bothHands)
		{
			ItemEntityWeapon weapon = ((!checkOnOwner) ? base.Target.Entity?.GetSecondaryHandWeapon() : base.Context.MaybeOwner?.GetSecondaryHandWeapon());
			flag = HasCorrectWeapon(weapon);
		}
		if (itemEntityWeapon == null && !bothHands)
		{
			return flag;
		}
		bool flag2 = HasCorrectWeapon(itemEntityWeapon);
		if (bothHands)
		{
			return flag2 && flag;
		}
		return flag2;
	}

	private bool HasCorrectWeapon(ItemEntityWeapon weapon)
	{
		if (weapon != null && (!isMelee || weapon.Blueprint.IsMelee) && (!isCanBurst || weapon.GetWeaponStats().ResultRateOfFire > 1))
		{
			if (checkCategory)
			{
				return Categories.HasItem(weapon.Blueprint.Category);
			}
			return true;
		}
		return false;
	}
}
