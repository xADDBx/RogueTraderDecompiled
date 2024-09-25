using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("82e471e43c7b41ea81cf654116fb7f82")]
public class ContextConditionHasReloadableWeapon : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Check is target has a weapon it can reload";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = entity.GetBodyOptional()?.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon itemEntityWeapon2 = entity.GetBodyOptional()?.SecondaryHand.MaybeWeapon;
		if (itemEntityWeapon == null || itemEntityWeapon.Blueprint.WarhammerMaxAmmo < 0 || itemEntityWeapon.CurrentAmmo >= itemEntityWeapon.Blueprint.WarhammerMaxAmmo)
		{
			if (itemEntityWeapon2 != null && itemEntityWeapon2.Blueprint.WarhammerMaxAmmo >= 0)
			{
				return itemEntityWeapon2.CurrentAmmo < itemEntityWeapon2.Blueprint.WarhammerMaxAmmo;
			}
			return false;
		}
		return true;
	}
}
