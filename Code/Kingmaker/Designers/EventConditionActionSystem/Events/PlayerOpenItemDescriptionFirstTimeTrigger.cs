using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[TypeId("4eed9274a7d420c40a17f7982062b98b")]
public class PlayerOpenItemDescriptionFirstTimeTrigger : EntityFactComponentDelegate, IPlayerOpenItemDescriptionHandler, ISubscriber<IItemEntity>, ISubscriber, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public List<BlueprintItem> OpenedItems = new List<BlueprintItem>();

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			List<BlueprintItem> openedItems = OpenedItems;
			if (openedItems != null)
			{
				for (int i = 0; i < openedItems.Count; i++)
				{
					Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(openedItems[i]);
					result.Append(ref val2);
				}
			}
			return result;
		}
	}

	public List<ItemToActions> Items;

	public void HandlePlayerOpenItemDescription()
	{
		if (Items == null)
		{
			return;
		}
		SavableData savableData = RequestSavableData<SavableData>();
		BlueprintItem blueprintItem = EventInvokerExtensions.GetEntity<ItemEntity>()?.Blueprint;
		if (!savableData.OpenedItems.Contains(blueprintItem))
		{
			ItemToActions itemToActions = Items.Find((ItemToActions itoa) => itoa.Item == blueprintItem);
			if (itemToActions != null)
			{
				itemToActions.Actions.Run();
				savableData.OpenedItems.Add(itemToActions.Item);
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
