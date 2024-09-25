using System;
using System.Linq;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class PointOfInterestLoot : BasePointOfInterest, IItemsCollectionHandler, ISubscriber, IHashable
{
	private LootFromPointOfInterestHolder m_LootHolder;

	private bool m_IsFirstTimeOpenLoot = true;

	public new BlueprintPointOfInterestLoot Blueprint => (BlueprintPointOfInterestLoot)base.Blueprint;

	public PointOfInterestLoot(BlueprintPointOfInterestLoot blueprint)
		: base(blueprint)
	{
	}

	public PointOfInterestLoot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void Interact(StarSystemObjectEntity entity)
	{
		if (base.Status == ExplorationStatus.Explored)
		{
			return;
		}
		m_LootHolder = entity.LootHolder.FirstOrDefault((LootFromPointOfInterestHolder lootHolder) => lootHolder.Point.Blueprint == Blueprint);
		LootFromPointOfInterestHolder[] iLootable = new LootFromPointOfInterestHolder[1] { m_LootHolder };
		if (m_LootHolder == null)
		{
			PFLog.Default.Warning("Cannot find lootHolder for point of interest");
			return;
		}
		if (m_IsFirstTimeOpenLoot)
		{
			EventBus.Subscribe(this);
			m_IsFirstTimeOpenLoot = false;
		}
		EventBus.RaiseEvent(Game.Instance.Player.MainCharacter.ToIBaseUnitEntity(), delegate(ILootInteractionHandler l)
		{
			ILootable[] objects = iLootable;
			l.HandleSpaceLootInteraction(objects, LootContainerType.StarSystemObject, null);
		});
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		CheckStatus();
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		CheckStatus();
	}

	private void CheckStatus()
	{
		if (m_LootHolder != null && !m_LootHolder.Items.Any())
		{
			Game.Instance.GameCommandQueue.PointOfInterestSetInteracted(Blueprint);
			EventBus.Unsubscribe(this);
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
