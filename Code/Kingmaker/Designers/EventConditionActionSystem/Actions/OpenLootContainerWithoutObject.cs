using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c2703d1959217704e826fc2e38a8852a")]
public class OpenLootContainerWithoutObject : GameAction
{
	public class LootList : ILootable
	{
		private ItemsCollection m_Items { get; set; }

		public string Name => ContextData<StarSystemContextData>.Current?.StarSystemObject?.Blueprint.Name;

		public string Description => null;

		public BaseUnitEntity OwnerEntity => null;

		public ItemsCollection Items => m_Items;

		public List<BlueprintCargoReference> Cargo => null;

		public Func<ItemEntity, bool> CanInsertItem => (ItemEntity _) => false;

		public LootList(List<LootEntry> loot)
		{
			m_Items = new ItemsCollection(ContextData<StarSystemContextData>.Current?.StarSystemObject);
			foreach (LootEntry item in loot)
			{
				m_Items.Add(item.Item, item.Count);
			}
		}
	}

	[SerializeField]
	private List<LootEntry> m_ExplorationLoot = new List<LootEntry>();

	public override string GetCaption()
	{
		return "Открывает лут контейнер с заданным списком лута";
	}

	public override void RunAction()
	{
		LootList lootList = new LootList(m_ExplorationLoot);
		LootList[] iLootables = new LootList[1] { lootList };
		EventBus.RaiseEvent(Game.Instance.Player.MainCharacter.Entity.ToIBaseUnitEntity(), delegate(ILootInteractionHandler l)
		{
			ILootable[] objects = iLootables;
			l.HandleSpaceLootInteraction(objects, LootContainerType.StarSystemObject, null);
		});
	}
}
