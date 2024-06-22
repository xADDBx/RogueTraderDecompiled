using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.Items.Slots;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("1773ce4423b542d2932f6e101806c26e")]
public class HasWeaponOfRangeType : UnitPropertyGetter
{
	public bool Melee;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!Melee)
		{
			return "Any weapon of " + FormulaTargetScope.Current + " is Ranged";
		}
		return "Any weapon of " + FormulaTargetScope.Current + " is Melee";
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon maybeWeapon = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
		int num;
		if (maybeWeapon == null || !maybeWeapon.Blueprint.IsMelee || !Melee)
		{
			ItemEntityWeapon maybeWeapon2 = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
			num = ((maybeWeapon2 != null && maybeWeapon2.Blueprint.IsRanged && !Melee) ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		ItemEntityWeapon maybeWeapon3 = base.CurrentEntity.Body.SecondaryHand.MaybeWeapon;
		int num2;
		if (maybeWeapon3 == null || !maybeWeapon3.Blueprint.IsMelee || !Melee)
		{
			ItemEntityWeapon maybeWeapon4 = base.CurrentEntity.Body.SecondaryHand.MaybeWeapon;
			num2 = ((maybeWeapon4 != null && maybeWeapon4.Blueprint.IsRanged && !Melee) ? 1 : 0);
		}
		else
		{
			num2 = 1;
		}
		bool flag = (byte)num2 != 0;
		bool flag2 = (Melee && base.CurrentEntity.Body.AdditionalLimbs.Any((WeaponSlot p) => p.MaybeWeapon?.Blueprint.IsMelee ?? false)) || (!Melee && base.CurrentEntity.Body.AdditionalLimbs.Any((WeaponSlot p) => p.MaybeWeapon?.Blueprint.IsRanged ?? false));
		if (((uint)num | (flag ? 1u : 0u) | (flag2 ? 1u : 0u)) == 0)
		{
			return 0;
		}
		return 1;
	}
}
