using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[AllowMultipleComponents]
[TypeId("0baee9b0e78f4cb388882ff6e59934a5")]
public class TutorialTriggerPlayerGainItem : TutorialTrigger, IItemsCollectionHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintItemReference m_ItemReference;

	private BlueprintItem Item => m_ItemReference.Get();

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection != null && collection.IsPlayerInventory && Item == item.Blueprint)
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
