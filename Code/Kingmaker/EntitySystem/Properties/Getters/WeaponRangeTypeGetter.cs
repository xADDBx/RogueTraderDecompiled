using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("145fae42674897643a03d1d165941210")]
public class WeaponRangeTypeGetter : UnitPropertyGetter
{
	private enum WeaponRangeType
	{
		Melee,
		Ranged
	}

	[SerializeField]
	private bool SecondWeapon;

	[SerializeField]
	private WeaponRangeType m_RangeType;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		string text = ((m_RangeType == WeaponRangeType.Melee) ? "Melee" : "Ranged");
		if (!SecondWeapon)
		{
			return "First Weapon of " + FormulaTargetScope.Current + " Range is " + text;
		}
		return "Second Weapon of " + FormulaTargetScope.Current + " Range is " + text;
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon itemEntityWeapon = (SecondWeapon ? base.CurrentEntity.Body.SecondaryHand.MaybeWeapon : base.CurrentEntity.Body.PrimaryHand.MaybeWeapon);
		if (itemEntityWeapon == null)
		{
			return 0;
		}
		bool isMelee = itemEntityWeapon.Blueprint.IsMelee;
		if ((!isMelee || m_RangeType != 0) && (isMelee || m_RangeType != WeaponRangeType.Ranged))
		{
			return 0;
		}
		return 1;
	}
}
