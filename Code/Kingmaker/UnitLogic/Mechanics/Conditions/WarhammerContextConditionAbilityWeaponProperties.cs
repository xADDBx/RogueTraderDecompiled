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
		ItemEntityWeapon itemEntityWeapon = ((!checkOnOwner) ? base.Target.Entity?.GetFirstWeapon() : base.Context.MaybeOwner?.GetFirstWeapon());
		bool flag = false;
		if (anyHand || bothHands)
		{
			ItemEntityWeapon itemEntityWeapon2 = ((!checkOnOwner) ? base.Target.Entity?.GetSecondWeapon() : base.Context.MaybeOwner?.GetSecondWeapon());
			if (itemEntityWeapon2 == null)
			{
				flag = false;
			}
			else if ((!isMelee || itemEntityWeapon2.Blueprint.IsMelee) && (!isCanBurst || itemEntityWeapon2.GetWeaponStats().ResultRateOfFire > 1) && (!checkCategory || Categories.HasItem(itemEntityWeapon2.Blueprint.Category)))
			{
				flag = true;
			}
		}
		if (itemEntityWeapon == null && !bothHands)
		{
			return flag;
		}
		bool flag2 = (!isMelee || itemEntityWeapon.Blueprint.IsMelee) && (!isCanBurst || itemEntityWeapon.GetWeaponStats().ResultRateOfFire > 1) && (!checkCategory || Categories.HasItem(itemEntityWeapon.Blueprint.Category));
		if (bothHands)
		{
			return flag2 && flag;
		}
		return flag2;
	}
}
