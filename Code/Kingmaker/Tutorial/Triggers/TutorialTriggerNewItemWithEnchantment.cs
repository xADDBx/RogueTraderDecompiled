using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("Triggers on adding new item with specified enchantment\n`t|SourceItem` - new item")]
[TypeId("456ba597788dc904a889823b86673b2a")]
public class TutorialTriggerNewItemWithEnchantment : TutorialTrigger, IItemsCollectionHandler, ISubscriber, IIdentifyHandler, ISubscriber<IItemEntity>, IHashable
{
	[SerializeField]
	private BlueprintItemEnchantmentReference m_Enchantment;

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		TryToTrigger(collection.IsPlayerInventory, item);
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
	}

	public void OnItemIdentified(BaseUnitEntity character)
	{
		TryToTrigger(playerInventory: true, EventInvokerExtensions.GetEntity<ItemEntity>());
	}

	public void OnFailedToIdentify()
	{
	}

	private void TryToTrigger(bool playerInventory, ItemEntity item)
	{
		if (playerInventory && item.HasEnchantment(m_Enchantment))
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceItem = item;
			});
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
