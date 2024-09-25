using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("0408ed03c2844bf89bb4a6569af75bf9")]
public class RemoveAllEquipment : UnitFactComponentDelegate, IHashable
{
	public class HandEquipmentSetItems : IHashable
	{
		[JsonProperty]
		public BlueprintItem PrimaryHand;

		[JsonProperty]
		public BlueprintItem SecondaryHand;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(PrimaryHand);
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(SecondaryHand);
			result.Append(ref val2);
			return result;
		}
	}

	public class Data : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public List<HandEquipmentSetItems> HandSets = new List<HandEquipmentSetItems>();

		[JsonProperty]
		[CanBeNull]
		public HandEquipmentSetItems PolymorphHandsEquipmentSet;

		[JsonProperty]
		public List<BlueprintItemEquipmentUsable> QuickSlots = new List<BlueprintItemEquipmentUsable>();

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemArmor Armor;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentShirt Shirt;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentBelt Belt;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentHead Head;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentGlasses Glasses;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentFeet Feet;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentGloves Gloves;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentNeck Neck;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentRing Ring1;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentRing Ring2;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentWrist Wrist;

		[JsonProperty]
		[CanBeNull]
		public BlueprintItemEquipmentShoulders Shoulders;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			List<HandEquipmentSetItems> handSets = HandSets;
			if (handSets != null)
			{
				for (int i = 0; i < handSets.Count; i++)
				{
					Hash128 val2 = ClassHasher<HandEquipmentSetItems>.GetHash128(handSets[i]);
					result.Append(ref val2);
				}
			}
			Hash128 val3 = ClassHasher<HandEquipmentSetItems>.GetHash128(PolymorphHandsEquipmentSet);
			result.Append(ref val3);
			List<BlueprintItemEquipmentUsable> quickSlots = QuickSlots;
			if (quickSlots != null)
			{
				for (int j = 0; j < quickSlots.Count; j++)
				{
					Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(quickSlots[j]);
					result.Append(ref val4);
				}
			}
			Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Armor);
			result.Append(ref val5);
			Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Shirt);
			result.Append(ref val6);
			Hash128 val7 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Belt);
			result.Append(ref val7);
			Hash128 val8 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Head);
			result.Append(ref val8);
			Hash128 val9 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Glasses);
			result.Append(ref val9);
			Hash128 val10 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Feet);
			result.Append(ref val10);
			Hash128 val11 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Gloves);
			result.Append(ref val11);
			Hash128 val12 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Neck);
			result.Append(ref val12);
			Hash128 val13 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Ring1);
			result.Append(ref val13);
			Hash128 val14 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Ring2);
			result.Append(ref val14);
			Hash128 val15 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Wrist);
			result.Append(ref val15);
			Hash128 val16 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Shoulders);
			result.Append(ref val16);
			return result;
		}
	}

	protected override void OnFactAttached()
	{
		base.OnFactAttached();
		using (ContextData<ItemsCollection.SuppressEvents>.Request())
		{
			Data data = base.Fact.RequestSavableData<Data>(this);
			PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
			if (bodyOptional == null)
			{
				return;
			}
			data.Armor = bodyOptional.Armor.MaybeItem?.Blueprint as BlueprintItemArmor;
			UnequipItem(bodyOptional.Armor);
			data.Shirt = bodyOptional.Shirt.MaybeItem?.Blueprint as BlueprintItemEquipmentShirt;
			UnequipItem(bodyOptional.Shirt);
			data.Belt = bodyOptional.Belt.MaybeItem?.Blueprint as BlueprintItemEquipmentBelt;
			UnequipItem(bodyOptional.Belt);
			data.Head = bodyOptional.Head.MaybeItem?.Blueprint as BlueprintItemEquipmentHead;
			UnequipItem(bodyOptional.Head);
			data.Glasses = bodyOptional.Glasses.MaybeItem?.Blueprint as BlueprintItemEquipmentGlasses;
			UnequipItem(bodyOptional.Glasses);
			data.Feet = bodyOptional.Feet.MaybeItem?.Blueprint as BlueprintItemEquipmentFeet;
			UnequipItem(bodyOptional.Feet);
			data.Gloves = bodyOptional.Gloves.MaybeItem?.Blueprint as BlueprintItemEquipmentGloves;
			UnequipItem(bodyOptional.Gloves);
			data.Neck = bodyOptional.Neck.MaybeItem?.Blueprint as BlueprintItemEquipmentNeck;
			UnequipItem(bodyOptional.Neck);
			data.Ring1 = bodyOptional.Ring1.MaybeItem?.Blueprint as BlueprintItemEquipmentRing;
			UnequipItem(bodyOptional.Ring1);
			data.Ring2 = bodyOptional.Ring2.MaybeItem?.Blueprint as BlueprintItemEquipmentRing;
			UnequipItem(bodyOptional.Ring2);
			data.Wrist = bodyOptional.Wrist.MaybeItem?.Blueprint as BlueprintItemEquipmentWrist;
			UnequipItem(bodyOptional.Wrist);
			data.Shoulders = bodyOptional.Shoulders.MaybeItem?.Blueprint as BlueprintItemEquipmentShoulders;
			UnequipItem(bodyOptional.Shoulders);
			if (bodyOptional.QuickSlots != null)
			{
				UsableSlot[] quickSlots = bodyOptional.QuickSlots;
				foreach (UsableSlot usableSlot in quickSlots)
				{
					data.QuickSlots.Add(usableSlot.MaybeItem?.Blueprint as BlueprintItemEquipmentUsable);
					UnequipItem(usableSlot);
				}
			}
			foreach (HandsEquipmentSet handsEquipmentSet in bodyOptional.HandsEquipmentSets)
			{
				HandEquipmentSetItems item = new HandEquipmentSetItems
				{
					PrimaryHand = handsEquipmentSet.PrimaryHand.MaybeItem?.Blueprint,
					SecondaryHand = handsEquipmentSet.SecondaryHand.MaybeItem?.Blueprint
				};
				data.HandSets.Add(item);
				UnequipItem(handsEquipmentSet.PrimaryHand);
				UnequipItem(handsEquipmentSet.SecondaryHand);
			}
			HandsEquipmentSet polymorphHandsEquipmentSet = bodyOptional.PolymorphHandsEquipmentSet;
			if (polymorphHandsEquipmentSet != null)
			{
				data.PolymorphHandsEquipmentSet = new HandEquipmentSetItems
				{
					PrimaryHand = polymorphHandsEquipmentSet.PrimaryHand.MaybeItem?.Blueprint,
					SecondaryHand = polymorphHandsEquipmentSet.SecondaryHand.MaybeItem?.Blueprint
				};
				UnequipItem(polymorphHandsEquipmentSet.PrimaryHand);
				UnequipItem(polymorphHandsEquipmentSet.SecondaryHand);
			}
		}
	}

	protected override void OnFactDetached()
	{
		base.OnFactDetached();
		Data data = base.Fact.RequestSavableData<Data>(this);
		PartUnitBody bodyOptional = base.Owner.GetBodyOptional();
		if (bodyOptional != null)
		{
			EquipItem(data.Armor, bodyOptional.Armor);
			EquipItem(data.Shirt, bodyOptional.Shirt);
			EquipItem(data.Belt, bodyOptional.Belt);
			EquipItem(data.Head, bodyOptional.Head);
			EquipItem(data.Glasses, bodyOptional.Glasses);
			EquipItem(data.Feet, bodyOptional.Feet);
			EquipItem(data.Gloves, bodyOptional.Gloves);
			EquipItem(data.Neck, bodyOptional.Neck);
			EquipItem(data.Ring1, bodyOptional.Ring1);
			EquipItem(data.Ring2, bodyOptional.Ring2);
			EquipItem(data.Wrist, bodyOptional.Wrist);
			EquipItem(data.Shoulders, bodyOptional.Shoulders);
			for (int i = 0; i < data.QuickSlots.Count; i++)
			{
				EquipItem(data.QuickSlots[i], bodyOptional.QuickSlots[i]);
			}
			for (int j = 0; j < data.HandSets.Count; j++)
			{
				HandsEquipmentSet handsEquipmentSet = bodyOptional.HandsEquipmentSets[j];
				EquipItem(data.HandSets[j].PrimaryHand, handsEquipmentSet.PrimaryHand);
				EquipItem(data.HandSets[j].SecondaryHand, handsEquipmentSet.SecondaryHand);
			}
			HandEquipmentSetItems polymorphHandsEquipmentSet = data.PolymorphHandsEquipmentSet;
			EquipItem(polymorphHandsEquipmentSet?.PrimaryHand, bodyOptional.PolymorphHandsEquipmentSet?.PrimaryHand);
			EquipItem(polymorphHandsEquipmentSet?.SecondaryHand, bodyOptional.PolymorphHandsEquipmentSet?.SecondaryHand);
		}
	}

	private void UnequipItem(ItemSlot slot)
	{
		ItemEntity maybeItem = slot.MaybeItem;
		slot.MaybeItem?.OnWillUnequip();
		slot.MaybeItem?.Dispose();
		maybeItem?.Collection?.Extract(maybeItem);
	}

	private void EquipItem(BlueprintItem itemBp, ItemSlot slot)
	{
		if (slot != null)
		{
			slot.RemoveItem(autoMerge: true, force: true);
			if (itemBp != null)
			{
				ItemEntity item = itemBp.CreateEntity();
				slot.InsertItem(item, force: true);
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
