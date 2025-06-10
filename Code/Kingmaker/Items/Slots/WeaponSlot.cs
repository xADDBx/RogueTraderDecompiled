using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

public class WeaponSlot : ItemSlot, IHashable
{
	private WeaponParticlesSnapMap m_NaturalFxSnapMap;

	private WeaponParticlesSnapMap m_FxSnapMap;

	public BlueprintAbility AttackOfOpportunityAbility => MaybeWeapon?.Blueprint.AttackOfOpportunityAbility;

	public BlueprintAbilityFXSettings AttackOfOpportunityAbilityFXSettings => MaybeWeapon?.Blueprint.AttackOfOpportunityAbilityFXSettings;

	public bool HasWeapon => MaybeWeapon != null;

	public virtual bool HasShield => false;

	public bool IsMelee
	{
		get
		{
			if (MaybeWeapon != null)
			{
				return MaybeWeapon.Blueprint.IsMelee;
			}
			return false;
		}
	}

	public bool IsRanged
	{
		get
		{
			if (MaybeWeapon != null)
			{
				return MaybeWeapon.Blueprint.IsRanged;
			}
			return false;
		}
	}

	public int AttackRange => Weapon.AttackRange;

	public int AttackOptimalRange => Weapon.AttackOptimalRange;

	[NotNull]
	public ItemEntityWeapon Weapon
	{
		get
		{
			if (MaybeWeapon == null)
			{
				throw new Exception("Has no weapon in slot");
			}
			return MaybeWeapon;
		}
	}

	[CanBeNull]
	public virtual ItemEntityWeapon MaybeWeapon
	{
		get
		{
			if (base.MaybeItem == null || !base.Active)
			{
				return null;
			}
			return base.MaybeItem as ItemEntityWeapon;
		}
	}

	[CanBeNull]
	public virtual ItemEntityShield MaybeShield => null;

	public WeaponParticlesSnapMap FxSnapMap
	{
		get
		{
			return m_FxSnapMap;
		}
		set
		{
			m_FxSnapMap = value;
		}
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		return item is ItemEntityWeapon;
	}

	[JsonConstructor]
	public WeaponSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public WeaponSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public virtual WeaponAnimationStyle GetWeaponStyle(bool isDollRoom = false)
	{
		return MaybeWeapon?.GetAnimationStyle(isDollRoom, base.Owner) ?? WeaponAnimationStyle.None;
	}

	public WeaponClassification GetWeaponClassification()
	{
		return MaybeWeapon?.Blueprint.Classification ?? WeaponClassification.None;
	}

	public void FindSnapMapForNaturalWeapon()
	{
		m_NaturalFxSnapMap = null;
		MechanicEntityView view = base.Owner.View;
		if (!view)
		{
			return;
		}
		PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
		WeaponParticlesSnapMap.WeaponSlot slotType = ((bodyOptional.PrimaryHand != this) ? ((bodyOptional.SecondaryHand == this) ? WeaponParticlesSnapMap.WeaponSlot.SecondaryHand : WeaponParticlesSnapMap.WeaponSlot.Unknown) : WeaponParticlesSnapMap.WeaponSlot.PrimaryHand);
		if (slotType == WeaponParticlesSnapMap.WeaponSlot.Unknown)
		{
			int num = bodyOptional.AdditionalLimbs.IndexOf(this);
			if (num < 0)
			{
				return;
			}
			slotType = (WeaponParticlesSnapMap.WeaponSlot)(num + 2);
		}
		m_NaturalFxSnapMap = view.GetComponentsInChildren<WeaponParticlesSnapMap>().FirstItem((WeaponParticlesSnapMap m) => m.Slot == slotType);
	}

	public override bool RemoveItem(bool autoMerge = true, bool force = false)
	{
		bool result = base.RemoveItem(autoMerge);
		m_FxSnapMap = null;
		m_NaturalFxSnapMap = null;
		return result;
	}

	public override bool CanRemoveItem()
	{
		return base.CanRemoveItem();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
