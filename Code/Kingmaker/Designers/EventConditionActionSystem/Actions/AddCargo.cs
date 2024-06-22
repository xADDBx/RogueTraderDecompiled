using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("343049de4e36454c85b36f38485730f3")]
public class AddCargo : GameAction
{
	public ItemsItemOrigin m_Origin;

	public BlueprintLootReference m_Loot;

	public BlueprintLoot Loot => m_Loot?.Get();

	public override string GetCaption()
	{
		return $"Create and add cargo from {Loot}";
	}

	protected override void RunAction()
	{
		CargoEntity cargoEntity = null;
		List<CargoEntity> cargoes = new List<CargoEntity>();
		LootEntry[] items = Loot.Items;
		foreach (LootEntry lootEntry in items)
		{
			int num = lootEntry.Count;
			while (num > 0)
			{
				if (cargoEntity == null || cargoEntity.IsFull)
				{
					cargoEntity = Game.Instance.Player.CargoState.Create(m_Origin);
				}
				if (!cargoEntity.CanAdd(lootEntry.Item, out var canAddCount) || canAddCount <= 0)
				{
					break;
				}
				int num2 = Math.Min(num, canAddCount);
				num -= num2;
				cargoEntity.AddItem(lootEntry.Item, num2);
				if (cargoEntity.IsFull)
				{
					cargoes.Add(cargoEntity);
				}
			}
		}
		if (cargoEntity != null && !cargoEntity.IsFull)
		{
			cargoes.Add(cargoEntity);
		}
		EventBus.RaiseEvent(delegate(IAddCargoActionHandler h)
		{
			h.HandleAddCargoAction(cargoes);
		});
	}
}
