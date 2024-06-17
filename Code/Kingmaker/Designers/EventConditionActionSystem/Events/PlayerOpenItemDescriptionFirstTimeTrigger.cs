using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[TypeId("4eed9274a7d420c40a17f7982062b98b")]
public class PlayerOpenItemDescriptionFirstTimeTrigger : EntityFactComponentDelegate, IPlayerOpenItemDescriptionFirstTimeHandler, ISubscriber<IItemEntity>, ISubscriber, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Item")]
	private BlueprintItemReference m_Item;

	public ActionList Action;

	public BlueprintItem Item => m_Item?.Get();

	public void HandlePlayerOpenItemDescriptionFirstTime()
	{
		if (EventInvokerExtensions.GetEntity<ItemEntity>().Blueprint == Item)
		{
			Action.Run();
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
