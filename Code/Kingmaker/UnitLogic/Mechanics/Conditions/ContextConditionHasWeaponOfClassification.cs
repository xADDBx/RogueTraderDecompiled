using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("9718865f16054c38aa50cc4ef1a74388")]
public class ContextConditionHasWeaponOfClassification : ContextCondition
{
	public WeaponClassification Classification;

	public bool OnlyPrimaryHand;

	public bool CheckOnCaster;

	protected override string GetConditionCaption()
	{
		return "Check is target has a weapon of selected classification";
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
			return itemEntityWeapon?.Blueprint?.Classification == Classification;
		}
		ItemEntityWeapon itemEntityWeapon2 = mechanicEntity.GetBodyOptional()?.SecondaryHand.MaybeWeapon;
		if (itemEntityWeapon?.Blueprint?.Classification != Classification)
		{
			return itemEntityWeapon2?.Blueprint?.Classification == Classification;
		}
		return true;
	}
}
