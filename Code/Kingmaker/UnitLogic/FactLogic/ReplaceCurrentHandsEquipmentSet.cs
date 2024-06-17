using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.Attributes;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("d9a11879e7bd4893adfce6e4b8261428")]
public class ReplaceCurrentHandsEquipmentSet : UnitFactComponentDelegate, IHashable
{
	public class Data : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public BlueprintItem ReplacedWeaponPrimaryHand;

		[JsonProperty]
		public BlueprintItem ReplacedWeaponSecondaryHand;

		[JsonProperty]
		public int HandsEquipmentSetIndex;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(ReplacedWeaponPrimaryHand);
			result.Append(ref val2);
			Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(ReplacedWeaponSecondaryHand);
			result.Append(ref val3);
			result.Append(ref HandsEquipmentSetIndex);
			return result;
		}
	}

	[SerializeField]
	private BlueprintItemWeaponReference m_WeaponPrimaryHand;

	[SerializeField]
	[HideIf("IsTwoHanded")]
	private BlueprintItemWeaponReference m_WeaponSecondaryHand;

	public BlueprintItemWeapon WeaponPrimaryHand => m_WeaponPrimaryHand?.Get();

	private bool IsTwoHanded => WeaponPrimaryHand?.IsTwoHanded ?? false;

	public BlueprintItemWeapon WeaponSecondaryHand => m_WeaponSecondaryHand?.Get();

	protected override void OnActivate()
	{
		BaseUnitEntity owner = base.Fact.Owner;
		HandsEquipmentSet currentHandsEquipmentSet = owner.Body.CurrentHandsEquipmentSet;
		HandSlot primaryHand = currentHandsEquipmentSet.PrimaryHand;
		HandSlot secondaryHand = currentHandsEquipmentSet.SecondaryHand;
		Data data = RequestSavableData<Data>();
		data.ReplacedWeaponPrimaryHand = primaryHand.MaybeItem?.Blueprint;
		data.ReplacedWeaponSecondaryHand = secondaryHand.MaybeItem?.Blueprint;
		data.HandsEquipmentSetIndex = owner.Body.CurrentHandEquipmentSetIndex;
		ItemEntity maybeItem = primaryHand.MaybeItem;
		ItemEntity maybeItem2 = secondaryHand.MaybeItem;
		InsertWeapon(WeaponPrimaryHand, primaryHand);
		InsertWeapon(WeaponSecondaryHand, secondaryHand);
		maybeItem?.Collection?.Extract(maybeItem);
		maybeItem2?.Collection?.Extract(maybeItem2);
	}

	protected override void OnDeactivate()
	{
		BaseUnitEntity owner = base.Fact.Owner;
		Data data = RequestSavableData<Data>();
		HandsEquipmentSet handsEquipmentSet = owner.Body.HandsEquipmentSets[data.HandsEquipmentSetIndex];
		HandSlot primaryHand = handsEquipmentSet.PrimaryHand;
		HandSlot secondaryHand = handsEquipmentSet.SecondaryHand;
		ItemEntity maybeItem = primaryHand.MaybeItem;
		ItemEntity maybeItem2 = secondaryHand.MaybeItem;
		InsertWeapon(data.ReplacedWeaponPrimaryHand, primaryHand);
		InsertWeapon(data.ReplacedWeaponSecondaryHand, secondaryHand);
		maybeItem?.Collection?.Extract(maybeItem);
		maybeItem2?.Collection?.Extract(maybeItem2);
	}

	private void InsertWeapon(BlueprintItem weapon, HandSlot slot)
	{
		if (weapon == null)
		{
			slot.MaybeItem?.OnWillUnequip();
			slot.MaybeItem?.Dispose();
		}
		else
		{
			ItemEntity item = weapon.CreateEntity();
			slot.InsertItem(item, force: true);
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
