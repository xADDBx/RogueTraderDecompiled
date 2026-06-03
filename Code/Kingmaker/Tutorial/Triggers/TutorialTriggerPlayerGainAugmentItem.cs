using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("37780d3bf4b47394ab2932b795c25d63")]
public class TutorialTriggerPlayerGainAugmentItem : TutorialTrigger, IItemsCollectionHandler, ISubscriber, IHashable
{
	[Tooltip("Trigger only if the augment is Tier 2")]
	public bool OnlyTier2;

	[Tooltip("Trigger only if the augment has overdrive (galvanization)")]
	public bool OnlyWithOverdrive;

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
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = GameHelper.GetPlayerCharacter();
				context.SourceItem = item;
			});
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
