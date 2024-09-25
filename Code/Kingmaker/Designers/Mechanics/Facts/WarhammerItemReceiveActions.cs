using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[TypeId("482dab298e77afa45b0e9ac9546860c9")]
public class WarhammerItemReceiveActions : UnitFactComponentDelegate, IItemsCollectionHandler, ISubscriber, IHashable
{
	[Tooltip("When item with this blueprint is added to any collection, action list below will be executed")]
	[SerializeField]
	private BlueprintItemReference m_ItemToCheck;

	[Tooltip("There are many item collections in game. This check box makes actions to execute when items are placed to player inventory collection only")]
	public bool PlayerInventoryOnly;

	[Tooltip("Action list to execute when item above is added to any collection")]
	public ActionList ActionsOnAdd;

	public BlueprintItem ItemToCheck => m_ItemToCheck?.Get();

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (item.Blueprint == ItemToCheck && (!PlayerInventoryOnly || collection.IsPlayerInventory))
		{
			for (int i = 0; i < count; i++)
			{
				base.Fact.RunActionInContext(ActionsOnAdd);
			}
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
