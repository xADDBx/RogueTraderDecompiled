using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Cargo;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
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
		List<CargoEntity> cargoes = new List<CargoEntity>();
		Action<CargoEntity> action = delegate(CargoEntity cargoEntity)
		{
			cargoes.Add(cargoEntity);
			EventBus.RaiseEvent(delegate(ICargoStateChangedHandler h)
			{
				h.HandleCreateNewCargo(cargoEntity);
			});
		};
		CargoEntity cargoEntity2 = null;
		LootEntry[] items = Loot.Items;
		foreach (LootEntry lootEntry in items)
		{
			int num = lootEntry.Count;
			while (num > 0)
			{
				if (cargoEntity2 == null || cargoEntity2.IsFull)
				{
					using (ContextData<ItemsCollection.SuppressEvents>.Request())
					{
						cargoEntity2 = Game.Instance.Player.CargoState.Create(m_Origin);
					}
				}
				if (!cargoEntity2.CanAdd(lootEntry.Item, out var canAddCount) || canAddCount <= 0)
				{
					break;
				}
				int num2 = Math.Min(num, canAddCount);
				num -= num2;
				cargoEntity2.AddItem(lootEntry.Item, num2);
				if (cargoEntity2.IsFull)
				{
					action(cargoEntity2);
				}
			}
		}
		if (cargoEntity2 != null && !cargoEntity2.IsFull)
		{
			action(cargoEntity2);
		}
		EventBus.RaiseEvent(delegate(IAddCargoActionHandler h)
		{
			h.HandleAddCargoAction(cargoes);
		});
	}
}
