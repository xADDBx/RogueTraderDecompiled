using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Serializable]
[ComponentName("Events/GainAugmentTrigger")]
[AllowMultipleComponents]
[TypeId("65688823901caa448b712f3a5112016f")]
public class PartyGainAugmentTrigger : EntityFactComponentDelegate, IItemsCollectionHandler, ISubscriber, IHashable
{
	[Tooltip("Trigger only if the augment is Tier 2")]
	public bool OnlyTier2;

	[Tooltip("Trigger only if the augment has overdrive (galvanization)")]
	public bool OnlyWithOverdrive;

	public ActionList OnAugmentGained;

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection == null || !collection.IsPlayerInventory || !(item.Blueprint is BlueprintItemAugment blueprintItemAugment))
		{
			return;
		}
		if (OnlyTier2)
		{
			EquipmentRestrictionAugmentTier component = blueprintItemAugment.GetComponent<EquipmentRestrictionAugmentTier>();
			if (component == null || component.AugmentTier < AugmentTier.Tier2)
			{
				return;
			}
		}
		if (!OnlyWithOverdrive || blueprintItemAugment.OverdriveAbility != null)
		{
			OnAugmentGained.Run();
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
