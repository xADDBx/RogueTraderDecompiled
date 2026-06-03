using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/ItemTrigger")]
[AllowMultipleComponents]
[TypeId("2692cd97dff261b40b530d7b25e425cd")]
public class PartyInventoryTrigger : EntityFactComponentDelegate, IItemsCollectionHandler, ISubscriber, ICargoStateChangedHandler, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Item")]
	private BlueprintItemReference m_Item;

	public ActionList OnAddActions;

	public ActionList OnRemoveActions;

	public BlueprintItem Item => m_Item?.Get();

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (item.Blueprint != Item)
		{
			return;
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.Inventory.Collection == collection)
			{
				OnAddActions.Run();
				break;
			}
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (item.Blueprint != Item)
		{
			return;
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet.Inventory.Collection == collection)
			{
				OnRemoveActions.Run();
				break;
			}
		}
	}

	public void HandleCreateNewCargo(CargoEntity entity)
	{
	}

	public void HandleRemoveCargo(CargoEntity entity, bool fromMassSell)
	{
	}

	public void HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex)
	{
		if (item.Blueprint == Item)
		{
			OnAddActions.Run();
		}
	}

	public void HandleRemoveItemFromCargo(ItemEntity item, CargoEntity from)
	{
		if (item.Blueprint == Item)
		{
			OnRemoveActions.Run();
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
