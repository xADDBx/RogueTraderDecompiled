using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Loot;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GuidUtility;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Cargo;

public class CargoEntity : MechanicEntity<BlueprintCargo>, PartInventory.IOwner, IEntityPartOwner<PartInventory>, IEntityPartOwner, IHashable
{
	[JsonProperty]
	private int m_UnusableVolumePercent;

	[JsonProperty]
	private EntityRef<MechanicEntity> m_StarshipEntity;

	[JsonProperty]
	[CanBeNull]
	private EntityFactRef[] m_FactsAppliedToShip;

	private BlueprintCargoRoot.CargoTemplate m_OriginTemplate;

	public bool IsNew;

	public MechanicEntity StarshipEntity => m_StarshipEntity.Entity;

	public PartInventory Inventory => GetRequired<PartInventory>();

	public int UnusableVolumePercent => m_UnusableVolumePercent;

	public int FilledVolumePercent => Inventory.Items.Sum((ItemEntity x) => x.Blueprint.CargoVolumePercent * x.Count);

	public bool IsFull => FilledVolumePercent >= 100;

	public int ReputationPointsCost => base.Blueprint.OverridenReputationCost ?? m_OriginTemplate.ReputationPointsCost;

	public bool CanSell => IsFull;

	public CargoEntity([NotNull] BlueprintCargo blueprint, [NotNull] MechanicEntity starshipEntity)
		: base(Uuid.Instance.CreateString(), isInGame: true, blueprint)
	{
		m_StarshipEntity = starshipEntity;
		IsNew = true;
	}

	protected CargoEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		CacheTemplate();
		GetOrCreate<PartInventory>();
		CreateInventory();
	}

	private void CreateInventory()
	{
		CargoInventory component = base.Blueprint.GetComponent<CargoInventory>();
		if (component != null)
		{
			AddLoot(component.Loot);
			AddUnusableVolumePercent(component.UnusableVolumePercent);
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		CacheTemplate();
	}

	private void CacheTemplate()
	{
		m_OriginTemplate = Game.Instance.BlueprintRoot.SystemMechanics.CargoRoot.GetTemplate(base.Blueprint.OriginType);
	}

	public void AddLoot(IEnumerable<LootEntry> loot)
	{
		foreach (LootEntry item in loot)
		{
			if (item.Item == null)
			{
				continue;
			}
			PFLog.Default.Log($"Add loot {item.Item?.Name} {item.Count}");
			Inventory.Add(item.Item, item.Count, delegate(ItemEntity entity)
			{
				if (item.Identify)
				{
					entity.Identify();
				}
			});
		}
		foreach (ItemEntity item2 in Inventory.Items)
		{
			if (CargoHelper.IsTrashItem(item2))
			{
				PFLog.Default.Log("Convert loot " + item2.Name + " to unusable");
				m_UnusableVolumePercent += item2.Blueprint.CargoVolumePercent * item2.Count;
			}
		}
	}

	public void AddItem([NotNull] BlueprintItem item, int count)
	{
		if (CanAdd(item, out var canAddCount) || count <= canAddCount)
		{
			PFLog.Default.Log($"Add loot {item.Name} {count}");
			Inventory.Add(item, count, delegate(ItemEntity entity)
			{
				entity.Identify();
			});
			if (CargoHelper.IsTrashItem(item))
			{
				m_UnusableVolumePercent += item.CargoVolumePercent * count;
			}
		}
	}

	public bool CorrectOrigin(ItemsItemOrigin origin)
	{
		return base.Blueprint.OriginType == origin;
	}

	public bool CanAdd(ItemEntity itemEntity, out int canAddCount)
	{
		if (!CanAdd(itemEntity.Origin) || itemEntity.Collection == Inventory.Collection)
		{
			canAddCount = 0;
			return false;
		}
		if (itemEntity.Blueprint.CargoVolumePercent <= 0)
		{
			canAddCount = itemEntity.Count;
			return canAddCount > 0;
		}
		canAddCount = Mathf.CeilToInt((100f - (float)FilledVolumePercent) / (float)itemEntity.Blueprint.CargoVolumePercent);
		canAddCount = Math.Min(canAddCount, itemEntity.Count);
		canAddCount = Math.Min((CargoHelper.MaxFilledVolumePercentToAddItem - FilledVolumePercent) / itemEntity.Blueprint.CargoVolumePercent, canAddCount);
		return canAddCount > 0;
	}

	public bool CanAdd(BlueprintItem itemBlueprint, out int canAddCount)
	{
		if (!CanAdd(itemBlueprint.Origin))
		{
			canAddCount = 0;
			return false;
		}
		canAddCount = (CargoHelper.MaxFilledVolumePercentToAddItem - FilledVolumePercent) / itemBlueprint.CargoVolumePercent;
		return canAddCount > 0;
	}

	public bool CanAdd(ItemsItemOrigin origin)
	{
		if (base.Blueprint.Integral)
		{
			return false;
		}
		if (!CorrectOrigin(origin))
		{
			return false;
		}
		return !IsFull;
	}

	public void AddUnusableVolumePercent(int volume)
	{
		if (volume > 0)
		{
			m_UnusableVolumePercent += volume;
		}
	}

	public bool TryAdd(ItemEntity item, out int notAddedCount)
	{
		if (!CanAdd(item, out var canAddCount))
		{
			notAddedCount = item.Count;
			return false;
		}
		notAddedCount = item.Count - canAddCount;
		ItemsCollection from = item.Collection;
		int oldIndex = item.InventorySlotIndex;
		if (from == null)
		{
			Inventory.Add((canAddCount < item.Count) ? item.Split(canAddCount) : item);
		}
		else
		{
			item.Collection.Transfer(item, canAddCount, Inventory.Collection);
		}
		PFLog.Default.Log($"Add item {item} to cargo {base.UniqueId} {base.Blueprint.Name}");
		EventBus.RaiseEvent(delegate(ICargoStateChangedHandler h)
		{
			h.HandleAddItemToCargo(item, from, this, oldIndex);
		});
		return true;
	}

	public void AddItems(List<ItemEntity> items, out int addedCount)
	{
		addedCount = 0;
		foreach (ItemEntity item in items)
		{
			if (!TryAdd(item, out var notAddedCount) || notAddedCount > 0)
			{
				break;
			}
			addedCount++;
		}
	}

	public void OnAdd()
	{
		ReapplyFactsForOwnerShip();
	}

	public void OnRemove()
	{
		RemoveFactsFromOwnerShip();
	}

	public void RemoveFactsFromOwnerShip()
	{
		EntityFactRef[] array = m_FactsAppliedToShip.EmptyIfNull();
		foreach (EntityFactRef entityFactRef in array)
		{
			StarshipEntity.Facts.Remove(entityFactRef);
		}
		m_FactsAppliedToShip = null;
	}

	public void ReapplyFactsForOwnerShip()
	{
		m_FactsAppliedToShip = (from x in base.Blueprint.GetComponents<AddFactToCargoOwnerShip>().Select(delegate(AddFactToCargoOwnerShip c)
			{
				EntityFactRef? entityFactRef = m_FactsAppliedToShip?.FirstItem((EntityFactRef i) => i.Fact?.Blueprint == c.Fact);
				if (entityFactRef.HasValue && entityFactRef.GetValueOrDefault().Fact != null)
				{
					return entityFactRef.Value;
				}
				EntityFact entityFact = StarshipEntity.AddFact(c.Fact);
				if (entityFact == null)
				{
					PFLog.Default.Log("Can not create fact " + c.Fact?.Name + " by cargo " + base.Blueprint.Name);
					return (EntityFactRef)null;
				}
				if (entityFact.Sources.Count > 0 && !(entityFact is Feature))
				{
					PFLog.Default.Log("Can not set fact " + c.Fact?.Name + " source by cargo " + base.Blueprint.Name + " cause fact already has sources");
					return (EntityFactRef)null;
				}
				entityFact.AddSource(this);
				PFLog.Default.Log("Add fact " + c.Fact?.Name + " by cargo " + base.Blueprint.Name);
				return entityFact;
			})
			where x.Fact != null
			select x).ToArray();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_UnusableVolumePercent);
		EntityRef<MechanicEntity> obj = m_StarshipEntity;
		Hash128 val2 = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		EntityFactRef[] factsAppliedToShip = m_FactsAppliedToShip;
		if (factsAppliedToShip != null)
		{
			for (int i = 0; i < factsAppliedToShip.Length; i++)
			{
				EntityFactRef obj2 = factsAppliedToShip[i];
				Hash128 val3 = StructHasher<EntityFactRef>.GetHash128(ref obj2);
				result.Append(ref val3);
			}
		}
		return result;
	}
}
