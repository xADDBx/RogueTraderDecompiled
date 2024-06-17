using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Mechadendrites;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

public class HandSlot : WeaponSlot, IHashable
{
	private class SuppressNotifyEquipmentScope : ContextFlag<SuppressNotifyEquipmentScope>
	{
	}

	public bool IsDirty { get; set; }

	public bool HasShield => MaybeShield != null;

	[NotNull]
	public ItemEntityShield Shield
	{
		get
		{
			if (MaybeShield == null)
			{
				throw new Exception("Has no shield in slot");
			}
			return MaybeShield;
		}
	}

	[CanBeNull]
	public ItemEntityShield MaybeShield => base.MaybeItem as ItemEntityShield;

	public HandsEquipmentSet HandsEquipmentSet => base.Owner.GetBodyOptional()?.GetHandsEquipmentSet(this) ?? throw new Exception($"Can't find HandsEquipmentSet for HandSlot ({base.Owner})");

	public HandSlot PairSlot
	{
		get
		{
			HandsEquipmentSet handsEquipmentSet = HandsEquipmentSet;
			if (handsEquipmentSet.PrimaryHand != this)
			{
				return handsEquipmentSet.PrimaryHand;
			}
			return handsEquipmentSet.SecondaryHand;
		}
	}

	public bool IsPrimaryHand => HandsEquipmentSet.PrimaryHand == this;

	public override void UpdateActive()
	{
		PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
		base.Active = !base.Disabled && (bodyOptional == null || HandsEquipmentSet == bodyOptional.CurrentHandsEquipmentSet);
	}

	public static IDisposable SuppressNotifyEquipment()
	{
		return ContextData<SuppressNotifyEquipmentScope>.Request();
	}

	protected override void OnActiveChanged()
	{
		base.OnActiveChanged();
		if (!ContextData<SuppressNotifyEquipmentScope>.Current && base.Owner?.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment?.HandleEquipmentSlotUpdated(this, null);
		}
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		if (!base.IsItemSupported(item) && (!(item is ItemEntityShield) || IsPrimaryHand) && !(item.Blueprint is BlueprintItemEquipmentHandSimple))
		{
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = item as ItemEntityWeapon;
		if (itemEntityWeapon != null && base.Owner.HasMechadendrites())
		{
			if (IsPrimaryHand && itemEntityWeapon.Blueprint.IsRanged)
			{
				return false;
			}
			if (!IsPrimaryHand && itemEntityWeapon.Blueprint.IsMelee)
			{
				return false;
			}
		}
		if (itemEntityWeapon != null && base.Owner.Facts.Contains((BlueprintUnitFact)BlueprintWarhammerRoot.Instance.CommonSpaceMarineFact))
		{
			if (IsPrimaryHand && itemEntityWeapon.Blueprint.IsMelee)
			{
				return false;
			}
			if (!IsPrimaryHand && itemEntityWeapon.Blueprint.IsRanged)
			{
				return false;
			}
		}
		if (itemEntityWeapon == null || !itemEntityWeapon.HoldInTwoHands)
		{
			ItemEntityWeapon maybeWeapon = PairSlot.MaybeWeapon;
			if (maybeWeapon == null || !maybeWeapon.HoldInTwoHands)
			{
				goto IL_0105;
			}
		}
		if (PairSlot.HasItem && !PairSlot.CanRemoveItem())
		{
			return false;
		}
		goto IL_0105;
		IL_0105:
		return true;
	}

	public HandSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public HandSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnItemInserted()
	{
		if (!IsPrimaryHand)
		{
			HandSlot primaryHand = HandsEquipmentSet.PrimaryHand;
			if (primaryHand.MaybeItem is ItemEntityWeapon { HoldInTwoHands: not false })
			{
				primaryHand.RemoveItem();
			}
		}
		if (base.MaybeItem is ItemEntityWeapon { HoldInTwoHands: not false } itemEntityWeapon2)
		{
			if (IsPrimaryHand)
			{
				PairSlot.RemoveItem();
			}
			else
			{
				RemoveItem();
				PairSlot.InsertItem(itemEntityWeapon2);
			}
		}
		IsDirty = true;
	}

	public override bool RemoveItem(bool autoMerge = true, bool force = false)
	{
		if (base.RemoveItem(autoMerge, force))
		{
			return IsDirty = true;
		}
		return false;
	}

	public override WeaponAnimationStyle GetWeaponStyle(bool isDollRoom = false)
	{
		if (!HasShield)
		{
			return base.GetWeaponStyle(isDollRoom);
		}
		return WeaponAnimationStyle.Shield;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
