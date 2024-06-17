using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items.Slots;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("537c8f834c094964db16dd2ba24fdb69")]
public class LockEquipmentSlot : UnitFactComponentDelegate, IHashable
{
	private enum SlotType
	{
		Armor,
		MainHand,
		OffHand,
		Cloak,
		Bracers,
		Boots,
		Gloves,
		Ring1,
		Ring2,
		Necklace,
		Belt,
		Headgear,
		Weapon1,
		Weapon2,
		Weapon3,
		Weapon4,
		Weapon5,
		Weapon6,
		Weapon7,
		Weapon8,
		Glasses,
		Shirt
	}

	[SerializeField]
	private SlotType m_SlotType;

	[SerializeField]
	private bool m_Deactivate;

	protected override void OnActivate()
	{
		LockSlot();
	}

	protected override void OnDeactivate()
	{
		UnlockSlot();
	}

	protected override void OnPostLoad()
	{
		LockSlot();
	}

	private void LockSlot()
	{
		ItemSlot slot = GetSlot();
		if (slot != null)
		{
			slot.Lock.Retain();
			if (m_Deactivate)
			{
				slot.RetainDeactivateFlag();
			}
		}
	}

	private void UnlockSlot()
	{
		ItemSlot slot = GetSlot();
		if (slot != null)
		{
			slot.Lock.Release();
			if (m_Deactivate)
			{
				slot.ReleaseDeactivateFlag();
			}
		}
	}

	private ItemSlot GetSlot()
	{
		switch (m_SlotType)
		{
		case SlotType.Armor:
			return base.Owner.Body.Armor;
		case SlotType.Shirt:
			return base.Owner.Body.Shirt;
		case SlotType.MainHand:
			return base.Owner.Body.PrimaryHand;
		case SlotType.OffHand:
			return base.Owner.Body.SecondaryHand;
		case SlotType.Cloak:
			return base.Owner.Body.Shoulders;
		case SlotType.Bracers:
			return base.Owner.Body.Wrist;
		case SlotType.Boots:
			return base.Owner.Body.Feet;
		case SlotType.Gloves:
			return base.Owner.Body.Gloves;
		case SlotType.Ring1:
			return base.Owner.Body.Ring1;
		case SlotType.Ring2:
			return base.Owner.Body.Ring2;
		case SlotType.Necklace:
			return base.Owner.Body.Neck;
		case SlotType.Belt:
			return base.Owner.Body.Belt;
		case SlotType.Headgear:
			return base.Owner.Body.Head;
		case SlotType.Glasses:
			return base.Owner.Body.Glasses;
		case SlotType.Weapon1:
			return base.Owner.Body.HandsEquipmentSets[0].PrimaryHand;
		case SlotType.Weapon2:
			return base.Owner.Body.HandsEquipmentSets[0].SecondaryHand;
		case SlotType.Weapon3:
			return base.Owner.Body.HandsEquipmentSets[1].PrimaryHand;
		case SlotType.Weapon4:
			return base.Owner.Body.HandsEquipmentSets[1].SecondaryHand;
		case SlotType.Weapon5:
			return base.Owner.Body.HandsEquipmentSets[2].PrimaryHand;
		case SlotType.Weapon6:
			return base.Owner.Body.HandsEquipmentSets[2].SecondaryHand;
		case SlotType.Weapon7:
			return base.Owner.Body.HandsEquipmentSets[3].PrimaryHand;
		case SlotType.Weapon8:
			return base.Owner.Body.HandsEquipmentSets[3].SecondaryHand;
		default:
			PFLog.Default.Error($"Can't extract slot of type {m_SlotType}");
			return null;
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
