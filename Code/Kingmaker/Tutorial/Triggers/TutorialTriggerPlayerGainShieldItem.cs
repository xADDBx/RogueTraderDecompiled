using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("1c68320c186846449d7765d8630c20e2")]
public class TutorialTriggerPlayerGainShieldItem : TutorialTrigger, IItemsCollectionHandler, ISubscriber, IHashable
{
	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection != null && collection.IsPlayerInventory && item is ItemEntityShield)
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
