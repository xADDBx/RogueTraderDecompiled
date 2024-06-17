using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("3ebaea478cd96e848ae4f464a37ee246")]
public class AbilityCasterHasWeaponWithRangeType : BlueprintComponent, IAbilityCasterRestriction
{
	public WeaponRangeType RangeType;

	public bool MustHaveBurst;

	public bool OnlyMainHand;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		PartUnitBody bodyOptional = caster.GetBodyOptional();
		if (bodyOptional == null)
		{
			return false;
		}
		if (OnlyMainHand)
		{
			if (bodyOptional.PrimaryHand.HasWeapon)
			{
				if (RangeType.IsSuitableAttackType(bodyOptional.PrimaryHand.Weapon.Blueprint.AttackType))
				{
					if (MustHaveBurst && bodyOptional.PrimaryHand.Weapon.GetWeaponStats().ResultRateOfFire <= 1)
					{
						return bodyOptional.PrimaryHand.Weapon.Abilities.Any(delegate(Ability p)
						{
							WarhammerAbilityAttackDelivery component4 = p.Blueprint.GetComponent<WarhammerAbilityAttackDelivery>();
							return component4 != null && component4.Special == WarhammerAbilityAttackDelivery.SpecialType.Burst;
						});
					}
					return true;
				}
				return false;
			}
			return false;
		}
		bool flag = (bodyOptional.PrimaryHand.HasWeapon && ((RangeType.IsSuitableAttackType(bodyOptional.PrimaryHand.Weapon.Blueprint.AttackType) && (!MustHaveBurst || bodyOptional.PrimaryHand.Weapon.GetWeaponStats().ResultRateOfFire > 1)) || bodyOptional.PrimaryHand.Weapon.Abilities.Any(delegate(Ability p)
		{
			WarhammerAbilityAttackDelivery component3 = p.Blueprint.GetComponent<WarhammerAbilityAttackDelivery>();
			return component3 != null && component3.Special == WarhammerAbilityAttackDelivery.SpecialType.Burst;
		}))) || (bodyOptional.SecondaryHand.HasWeapon && ((RangeType.IsSuitableAttackType(bodyOptional.SecondaryHand.Weapon.Blueprint.AttackType) && (!MustHaveBurst || bodyOptional.SecondaryHand.Weapon.GetWeaponStats().ResultRateOfFire > 1)) || bodyOptional.SecondaryHand.Weapon.Abilities.Any(delegate(Ability p)
		{
			WarhammerAbilityAttackDelivery component2 = p.Blueprint.GetComponent<WarhammerAbilityAttackDelivery>();
			return component2 != null && component2.Special == WarhammerAbilityAttackDelivery.SpecialType.Burst;
		})));
		foreach (WeaponSlot additionalLimb in bodyOptional.AdditionalLimbs)
		{
			flag = flag || (additionalLimb.HasWeapon && RangeType.IsSuitableWeapon(additionalLimb.Weapon) && (!MustHaveBurst || additionalLimb.Weapon.GetWeaponStats().ResultRateOfFire > 1)) || additionalLimb.Weapon.Abilities.Any(delegate(Ability p)
			{
				WarhammerAbilityAttackDelivery component = p.Blueprint.GetComponent<WarhammerAbilityAttackDelivery>();
				return component != null && component.Special == WarhammerAbilityAttackDelivery.SpecialType.Burst;
			});
		}
		return flag;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		string text = LocalizedTexts.Instance.WeaponRangeTypes.GetText(RangeType);
		return LocalizedTexts.Instance.Reasons.SpecificWeaponRequired.ToString(delegate
		{
			GameLogContext.Text = text;
		});
	}
}
