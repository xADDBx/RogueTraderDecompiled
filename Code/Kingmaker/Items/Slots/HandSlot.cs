using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Enums;
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

	public override bool HasShield => MaybeShield != null;

	private bool CanUnActive
	{
		get
		{
			if (HasShield)
			{
				return !base.Owner.HasMechanicFeature(MechanicsFeatureType.OverrideShieldWeaponSetsPlacement);
			}
			return false;
		}
	}

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
	public override ItemEntityShield MaybeShield
	{
		get
		{
			if (base.MaybeItem == null || !base.Active)
			{
				return null;
			}
			return base.MaybeItem as ItemEntityShield;
		}
	}

	public override ItemEntityWeapon MaybeWeapon
	{
		get
		{
			ItemEntityWeapon itemEntityWeapon = base.MaybeWeapon;
			if (itemEntityWeapon == null)
			{
				ItemEntityShield maybeShield = MaybeShield;
				if (maybeShield == null)
				{
					return null;
				}
				itemEntityWeapon = maybeShield.WeaponComponent;
			}
			return itemEntityWeapon;
		}
	}

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
		base.Active = !base.Disabled && (CanUnActive || bodyOptional == null || HandsEquipmentSet == bodyOptional.CurrentHandsEquipmentSet);
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
		PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
		if (!base.Owner.HasMechanicFeature(MechanicsFeatureType.OverrideShieldWeaponSetsPlacement) && (!IsPrimaryHand || base.MaybeItem is ItemEntityWeapon { HoldInTwoHands: not false }) && bodyOptional != null)
		{
			IList<HandsEquipmentSet> handsEquipmentSets = bodyOptional.HandsEquipmentSets;
			for (int i = 0; i < handsEquipmentSets.Count; i++)
			{
				if (handsEquipmentSets[i] != HandsEquipmentSet && handsEquipmentSets[i].SecondaryHand.HasShield)
				{
					handsEquipmentSets[i].SecondaryHand.RemoveItem();
				}
			}
		}
		if (!IsPrimaryHand)
		{
			HandSlot primaryHand = HandsEquipmentSet.PrimaryHand;
			if (primaryHand.MaybeItem is ItemEntityWeapon { HoldInTwoHands: not false })
			{
				primaryHand.RemoveItem();
			}
		}
		if (base.MaybeItem is ItemEntityWeapon { HoldInTwoHands: not false } itemEntityWeapon3)
		{
			if (IsPrimaryHand)
			{
				PairSlot.RemoveItem();
			}
			else
			{
				RemoveItem();
				PairSlot.InsertItem(itemEntityWeapon3);
			}
		}
		if (base.MaybeItem is ItemEntityShield && bodyOptional != null)
		{
			IList<HandsEquipmentSet> handsEquipmentSets2 = bodyOptional.HandsEquipmentSets;
			if (!base.Owner.HasMechanicFeature(MechanicsFeatureType.OverrideShieldWeaponSetsPlacement))
			{
				for (int j = 0; j < handsEquipmentSets2.Count; j++)
				{
					if (handsEquipmentSets2[j] != HandsEquipmentSet)
					{
						handsEquipmentSets2[j].ClearForShieldInOtherSet(HandsEquipmentSet);
					}
				}
			}
			else
			{
				for (int k = 0; k < handsEquipmentSets2.Count; k++)
				{
					HandsEquipmentSet handsEquipmentSet = handsEquipmentSets2[k];
					if (handsEquipmentSet != HandsEquipmentSet && base.MaybeItem == handsEquipmentSet.SecondaryHand.MaybeItem)
					{
						handsEquipmentSet.OverrideSecondaryHand(null);
						handsEquipmentSet.SecondaryHand.IsDirty = true;
					}
				}
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

	protected override void OnItemRemoved()
	{
		PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
		if (base.MaybeItem is ItemEntityShield && !base.Owner.HasMechanicFeature(MechanicsFeatureType.OverrideShieldWeaponSetsPlacement) && bodyOptional != null)
		{
			IList<HandsEquipmentSet> handsEquipmentSets = bodyOptional.HandsEquipmentSets;
			for (int i = 0; i < handsEquipmentSets.Count; i++)
			{
				if (handsEquipmentSets[i] != HandsEquipmentSet)
				{
					handsEquipmentSets[i].OverrideSecondaryHand(null);
				}
			}
		}
		IsDirty = true;
	}

	public override WeaponAnimationStyle GetWeaponStyle(bool isDollRoom = false)
	{
		if (!HasShield)
		{
			return base.GetWeaponStyle(isDollRoom);
		}
		return WeaponAnimationStyle.Shield;
	}

	public override void PostLoad()
	{
		base.PostLoad();
		PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
		if (!(base.MaybeItem is ItemEntityShield) || base.Owner.HasMechanicFeature(MechanicsFeatureType.OverrideShieldWeaponSetsPlacement) || bodyOptional == null)
		{
			return;
		}
		IList<HandsEquipmentSet> handsEquipmentSets = bodyOptional.HandsEquipmentSets;
		for (int i = 0; i < handsEquipmentSets.Count; i++)
		{
			if (handsEquipmentSets[i] != HandsEquipmentSet)
			{
				handsEquipmentSets[i].OverrideSecondaryHand(HandsEquipmentSet.SecondaryHand);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
