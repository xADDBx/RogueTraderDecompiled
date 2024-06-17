using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Cargo;

public class CargoState : IHashable
{
	[JsonProperty]
	private List<CargoEntity> m_CargoEntities = new List<CargoEntity>();

	public readonly CountableFlag LockTransferFromCargo = new CountableFlag();

	public IEnumerable<CargoEntity> CargoEntities => m_CargoEntities;

	public bool CanAdd(ItemEntity item)
	{
		return CargoHelper.CanTransferToCargo(item);
	}

	public IEnumerable<CargoEntity> Get(ItemsItemOrigin origin, Func<CargoEntity, bool> pred = null)
	{
		if (pred == null)
		{
			pred = (CargoEntity _) => true;
		}
		return m_CargoEntities.Where((CargoEntity x) => x.Blueprint.OriginType == origin && pred(x));
	}

	public IEnumerable<CargoEntity> Get(BlueprintCargo blueprint, Func<CargoEntity, bool> pred = null)
	{
		if (pred == null)
		{
			pred = (CargoEntity _) => true;
		}
		return m_CargoEntities.Where((CargoEntity x) => x.Blueprint == blueprint && pred(x));
	}

	private CargoEntity GetOrCreateCargo(ItemEntity item)
	{
		int canAddCount;
		IOrderedEnumerable<CargoEntity> source = from x in m_CargoEntities
			where x.CanAdd(item, out canAddCount)
			orderby x.FilledVolumePercent descending, x.UniqueId
			select x;
		if (!source.Any())
		{
			return Create(item.Origin);
		}
		return source.First();
	}

	public void Create(BlueprintCargo blueprint, int count)
	{
		while (count > 0)
		{
			Create(blueprint);
			count--;
		}
	}

	public CargoEntity Create(BlueprintCargo blueprint)
	{
		if (blueprint == null)
		{
			return null;
		}
		CargoEntity entity = Entity.Initialize(new CargoEntity(blueprint, Game.Instance.Player.PlayerShip));
		entity.OnAdd();
		m_CargoEntities.Add(entity);
		EventBus.RaiseEvent(delegate(ICargoStateChangedHandler h)
		{
			h.HandleCreateNewCargo(entity);
		});
		return entity;
	}

	public CargoEntity Create(ItemsItemOrigin origin)
	{
		BlueprintCargoRoot.CargoTemplate template = Game.Instance.BlueprintRoot.SystemMechanics.CargoRoot.GetTemplate(origin);
		return Create(template.Template);
	}

	public void AddToCargo(IEnumerable<ItemEntity> items)
	{
		switch (items.Count())
		{
		case 0:
			return;
		case 1:
			AddToCargo(items.First());
			return;
		}
		List<ItemEntity> list = (from x in items
			where CargoHelper.CanTransferToCargo(x) && !CargoHelper.IsItemInCargo(x)
			orderby x.Blueprint.Origin, x.Blueprint.CargoVolumePercent, x.UniqueId
			select x).ToList();
		foreach (ItemsItemOrigin key in CargoHelper.Origins)
		{
			foreach (CargoEntity item in (from x in m_CargoEntities
				where x.CanAdd(key)
				orderby x.FilledVolumePercent descending, x.UniqueId
				select x).ToList())
			{
				item.AddItems(list, out var addedCount);
				list.RemoveRange(0, addedCount);
				if (list.Count == 0 || list.First().Blueprint.Origin != key)
				{
					break;
				}
			}
			while (list.Count > 0 && list.First().Origin == key)
			{
				Create(key).AddItems(list, out var addedCount2);
				list.RemoveRange(0, addedCount2);
			}
			if (list.Count == 0)
			{
				break;
			}
		}
		foreach (ItemEntity item2 in items)
		{
			item2.SetToCargoAutomatically(toCargo: true);
		}
	}

	public void AddToCargo(ItemEntity item)
	{
		if (CargoHelper.CanTransferToCargo(item) && !CargoHelper.IsItemInCargo(item))
		{
			int notAddedCount;
			while (item.Count > 0 && GetOrCreateCargo(item).TryAdd(item, out notAddedCount) && notAddedCount != 0)
			{
			}
		}
	}

	public void SellCargoes(List<CargoEntity> entities, FactionType factionType)
	{
		foreach (CargoEntity entity in entities)
		{
			SellCargo(entity, factionType, fromMassSell: true);
		}
		EventBus.RaiseEvent(delegate(IVendorMassSellCargoHandler h)
		{
			h.HandleMassSellChange();
		});
	}

	private void SellCargo(CargoEntity entity, FactionType factionType, bool fromMassSell = false)
	{
		if (entity.IsFull)
		{
			ReputationHelper.GainFactionReputation(factionType, entity.ReputationPointsCost);
			Remove(entity, fromMassSell);
		}
	}

	public void Remove(BlueprintCargo blueprint)
	{
		List<CargoEntity> list = (from x in m_CargoEntities
			where x.Blueprint == blueprint
			orderby x.FilledVolumePercent descending, x.UniqueId
			select x).ToList();
		if (list.Count != 0)
		{
			CargoEntity cargoEntity = list.FirstItem();
			if (cargoEntity != null)
			{
				Remove(cargoEntity);
			}
		}
	}

	public void Remove(CargoEntity entity, bool fromMassSell = false)
	{
		entity.OnRemove();
		m_CargoEntities.Remove(entity);
		EventBus.RaiseEvent(delegate(ICargoStateChangedHandler h)
		{
			h.HandleRemoveCargo(entity, fromMassSell);
		});
		foreach (CargoEntity cargoEntity in m_CargoEntities)
		{
			cargoEntity.ReapplyFactsForOwnerShip();
		}
	}

	public void PostLoad()
	{
		foreach (CargoEntity cargoEntity in m_CargoEntities)
		{
			cargoEntity.PostLoad();
		}
	}

	public void PreSave()
	{
		foreach (CargoEntity cargoEntity in m_CargoEntities)
		{
			cargoEntity.PreSave();
		}
	}

	public void Subscribe()
	{
		foreach (CargoEntity cargoEntity in m_CargoEntities)
		{
			cargoEntity.Subscribe();
		}
	}

	public void Unsubscribe()
	{
		foreach (CargoEntity cargoEntity in m_CargoEntities)
		{
			cargoEntity.Unsubscribe();
		}
	}

	public void Dispose()
	{
		foreach (CargoEntity cargoEntity in m_CargoEntities)
		{
			cargoEntity.Dispose();
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<CargoEntity> cargoEntities = m_CargoEntities;
		if (cargoEntities != null)
		{
			for (int i = 0; i < cargoEntities.Count; i++)
			{
				Hash128 val = ClassHasher<CargoEntity>.GetHash128(cargoEntities[i]);
				result.Append(ref val);
			}
		}
		return result;
	}
}
