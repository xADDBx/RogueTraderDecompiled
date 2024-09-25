using System.Linq;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("cfa5ce3b77ef4a67a153c9d3352c2527")]
public class TutorialTriggerPlayerHasManyWeapons : TutorialTrigger, IItemsCollectionHandler, ISubscriber, IHashable
{
	[SerializeField]
	private int m_Value = 4;

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if ((collection == null || collection.IsPlayerInventory) && item.Blueprint is BlueprintItemWeapon && GameHelper.GetPlayerCharacter().Inventory.OfType<ItemEntityWeapon>().Count() > m_Value)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceItem = item;
				context.SourceUnit = GameHelper.GetPlayerCharacter();
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
