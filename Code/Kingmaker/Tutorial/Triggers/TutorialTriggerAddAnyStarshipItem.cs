using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("8ad8b6b9f105471b87fd47b94643d3cc")]
public class TutorialTriggerAddAnyStarshipItem : TutorialTrigger, IItemsCollectionHandler, ISubscriber, IHashable
{
	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection != null && collection.IsPlayerInventory && item.Blueprint is BlueprintStarshipItem)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
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
