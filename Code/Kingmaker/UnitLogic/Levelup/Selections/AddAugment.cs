using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("bda4111076cd43ee8eb22bab3bf3db7d")]
public class AddAugment : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintItemAugmentReference _augmentItem;

	public BlueprintItemAugment AugmentItem => _augmentItem.Get();

	protected override void OnActivate()
	{
		BaseUnitEntity owner = base.Fact.Owner;
		BlueprintAugmentSlot augmentSlot = AugmentItem.AugmentSlot;
		AugmentSlot augmentSlot2 = owner.Body.Augments.Slots[augmentSlot];
		if (augmentSlot2.MaybeItem != null)
		{
			PFLog.EntityFact.Error($"Augment slot {augmentSlot} is already occupied by " + $"{augmentSlot2.MaybeItem.Blueprint} for unit {owner}! " + $"Cannot insert augment {AugmentItem}");
			return;
		}
		ItemEntity item = AugmentItem.CreateEntity();
		augmentSlot2.InsertItem(item, force: true);
	}

	protected override void OnDeactivate()
	{
		BaseUnitEntity owner = base.Fact.Owner;
		BlueprintAugmentSlot augmentSlot = AugmentItem.AugmentSlot;
		AugmentSlot augmentSlot2 = owner.Body.Augments.Slots[augmentSlot];
		augmentSlot2.MaybeItem?.OnWillUnequip();
		augmentSlot2.MaybeItem?.Dispose();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
