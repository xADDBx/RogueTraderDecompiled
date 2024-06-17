using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/ItemInContainerTrigger")]
[AllowMultipleComponents]
[TypeId("570b5a90510c3434d82aec942a3323a6")]
public class ItemInContainerTrigger : EntityFactComponentDelegate, IItemsCollectionHandler, ISubscriber, IHashable
{
	[InfoBox("Triggers for every item if null")]
	[SerializeField]
	private BlueprintItemReference m_ItemToCheck;

	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator MapObject;

	[InfoBox("Evaluators: ItemFromContextEvaluator, InteractedMapObject")]
	public ActionList OnAddActions;

	[InfoBox("Evaluators: ItemFromContextEvaluator, InteractedMapObject")]
	public ActionList OnRemoveActions;

	[CanBeNull]
	public BlueprintItem ItemToCheck => m_ItemToCheck?.Get();

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (OnAddActions.HasActions && (ItemToCheck == null || ItemToCheck == item.Blueprint))
		{
			MapObjectEntity value = MapObject.GetValue();
			if (value.GetOptional<InteractionLootPart>()?.Loot == collection)
			{
				RunActions(OnAddActions, value, item);
			}
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (OnRemoveActions.HasActions && (ItemToCheck == null || ItemToCheck == item.Blueprint))
		{
			MapObjectEntity value = MapObject.GetValue();
			if (value.GetOptional<InteractionLootPart>()?.Loot == collection)
			{
				RunActions(OnRemoveActions, value, item);
			}
		}
	}

	private static void RunActions(ActionList actions, MapObjectEntity lootObject, ItemEntity item)
	{
		using (ContextData<ItemEntity.ContextData>.Request().Setup(item))
		{
			using (ContextData<MechanicEntityData>.Request().Setup(lootObject))
			{
				actions.Run();
			}
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
