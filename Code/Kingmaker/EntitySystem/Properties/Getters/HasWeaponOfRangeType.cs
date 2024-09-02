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

	public bool NotNatural;

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
		if (maybeWeapon == null || !maybeWeapon.Blueprint.IsMelee || !Melee)
		{
			goto IL_0066;
		}
		if (NotNatural)
		{
			ItemEntityWeapon maybeWeapon2 = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
			if (maybeWeapon2 != null && maybeWeapon2.Blueprint.IsNatural)
			{
				goto IL_0066;
			}
		}
		int num = 1;
		goto IL_00d0;
		IL_01a0:
		int num2;
		bool flag = (byte)num2 != 0;
		bool flag2 = (Melee && base.CurrentEntity.Body.AdditionalLimbs.Any(delegate(WeaponSlot p)
		{
			ItemEntityWeapon maybeWeapon11 = p.MaybeWeapon;
			if (maybeWeapon11 != null && maybeWeapon11.Blueprint.IsMelee)
			{
				if (NotNatural)
				{
					ItemEntityWeapon maybeWeapon12 = p.MaybeWeapon;
					if (maybeWeapon12 == null)
					{
						return true;
					}
					return !maybeWeapon12.Blueprint.IsNatural;
				}
				return true;
			}
			return false;
		})) || (!Melee && base.CurrentEntity.Body.AdditionalLimbs.Any(delegate(WeaponSlot p)
		{
			ItemEntityWeapon maybeWeapon9 = p.MaybeWeapon;
			if (maybeWeapon9 != null && maybeWeapon9.Blueprint.IsRanged)
			{
				if (NotNatural)
				{
					ItemEntityWeapon maybeWeapon10 = p.MaybeWeapon;
					if (maybeWeapon10 == null)
					{
						return true;
					}
					return !maybeWeapon10.Blueprint.IsNatural;
				}
				return true;
			}
			return false;
		}));
		if (((uint)num | (flag ? 1u : 0u) | (flag2 ? 1u : 0u)) == 0)
		{
			return 0;
		}
		return 1;
		IL_00d0:
		ItemEntityWeapon maybeWeapon3 = base.CurrentEntity.Body.SecondaryHand.MaybeWeapon;
		if (maybeWeapon3 == null || !maybeWeapon3.Blueprint.IsMelee || !Melee)
		{
			goto IL_0136;
		}
		if (NotNatural)
		{
			ItemEntityWeapon maybeWeapon4 = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
			if (maybeWeapon4 != null && maybeWeapon4.Blueprint.IsNatural)
			{
				goto IL_0136;
			}
		}
		num2 = 1;
		goto IL_01a0;
		IL_0066:
		ItemEntityWeapon maybeWeapon5 = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
		if (maybeWeapon5 != null && maybeWeapon5.Blueprint.IsRanged && !Melee)
		{
			if (NotNatural)
			{
				ItemEntityWeapon maybeWeapon6 = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
				num = ((maybeWeapon6 == null || !maybeWeapon6.Blueprint.IsNatural) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
		}
		else
		{
			num = 0;
		}
		goto IL_00d0;
		IL_0136:
		ItemEntityWeapon maybeWeapon7 = base.CurrentEntity.Body.SecondaryHand.MaybeWeapon;
		if (maybeWeapon7 != null && maybeWeapon7.Blueprint.IsRanged && !Melee)
		{
			if (NotNatural)
			{
				ItemEntityWeapon maybeWeapon8 = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
				num2 = ((maybeWeapon8 == null || !maybeWeapon8.Blueprint.IsNatural) ? 1 : 0);
			}
			else
			{
				num2 = 1;
			}
		}
		else
		{
			num2 = 0;
		}
		goto IL_01a0;
	}
}
