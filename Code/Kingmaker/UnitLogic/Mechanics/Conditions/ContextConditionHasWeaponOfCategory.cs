using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("754ab0a3750bc2d418d17799df58a0b4")]
public class ContextConditionHasWeaponOfCategory : ContextCondition
{
	public WeaponCategory Category;

	public bool OnlyPrimaryHand;

	public bool CheckOnCaster;

	protected override string GetConditionCaption()
	{
		return $"Check if target has a weapon of {Category} category";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity mechanicEntity = (CheckOnCaster ? base.Context.MaybeCaster : base.Target.Entity);
		if (mechanicEntity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = mechanicEntity.GetBodyOptional()?.PrimaryHand.MaybeWeapon;
		if (OnlyPrimaryHand)
		{
			return itemEntityWeapon?.Blueprint?.Category == Category;
		}
		ItemEntityWeapon itemEntityWeapon2 = mechanicEntity.GetBodyOptional()?.SecondaryHand.MaybeWeapon;
		if (itemEntityWeapon?.Blueprint?.Category != Category)
		{
			return itemEntityWeapon2?.Blueprint?.Category == Category;
		}
		return true;
	}
}
