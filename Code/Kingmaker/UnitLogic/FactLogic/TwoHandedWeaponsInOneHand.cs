using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Animation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b1bd63be736e4001b4fd57a2c26cda42")]
public class TwoHandedWeaponsInOneHand : UnitFactComponentDelegate, IHashable
{
	public enum SlotRestrictionType
	{
		None,
		Primary,
		Secondary
	}

	[SerializeField]
	private SlotRestrictionType m_SlotRestriction;

	[SerializeField]
	private WeaponAnimationStyle[] m_SourceAnimationStyles;

	[SerializeField]
	private WeaponAnimationStyle m_TargetAnimationStyle;

	public WeaponAnimationStyle TargetAnimationStyle => m_TargetAnimationStyle;

	public bool AffectsHandSlot(HandSlot slot)
	{
		bool isPrimaryHand = slot.IsPrimaryHand;
		return m_SlotRestriction switch
		{
			SlotRestrictionType.Primary => isPrimaryHand, 
			SlotRestrictionType.Secondary => !isPrimaryHand, 
			_ => true, 
		};
	}

	public bool AppliesToWeapon(ItemEntityWeapon weapon)
	{
		return m_SourceAnimationStyles.Contains(weapon.Blueprint.VisualParameters.AnimStyle);
	}

	protected override void OnActivateOrPostLoad()
	{
		base.OnActivateOrPostLoad();
		base.Owner.GetOrCreate<UnitPartTwoHandedInOneHand>().Register(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		UnitPartTwoHandedInOneHand optional = base.Owner.GetOptional<UnitPartTwoHandedInOneHand>();
		if (optional == null)
		{
			PFLog.EntityFact.Error("Logic error: no {0} part found on unregister. Fact {}, Component {1}", typeof(UnitPartTwoHandedInOneHand), base.Fact, this);
		}
		else
		{
			optional.Unregister(base.Fact, this);
		}
	}

	public bool CanHoldInSingleHand(ItemEntityWeapon weapon, HandSlot slot)
	{
		if (!AffectsHandSlot(slot))
		{
			return false;
		}
		if (!AppliesToWeapon(weapon))
		{
			return false;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
