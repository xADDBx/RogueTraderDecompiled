using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items;

[JsonObject(MemberSerialization.OptIn)]
public class ItemsCollection : IItemsCollection, IEnumerable<ItemEntity>, IEnumerable, IHashable
{
	public class DoNotRemoveFromSlot : ContextFlag<DoNotRemoveFromSlot>
	{
	}

	public class SuppressEvents : ContextFlag<SuppressEvents>
	{
	}

	[JsonProperty]
	private List<ItemEntity> m_Items = new List<ItemEntity>();

	[JsonProperty]
	private EntityRef m_OwnerRef;

	public IEntity Owner => m_OwnerRef.Entity;

	public Entity ConcreteOwner => (Entity)Owner;

	public BaseUnitEntity OwnerUnit => m_OwnerRef.Entity as BaseUnitEntity;

	public bool IsPostLoadExecuted { get; private set; }

	public bool IsPostLoadFixesExecuted { get; private set; }

	public bool IsDisposingNow { get; private set; }

	public float Weight { get; private set; }

	public bool ForceStackable { get; set; }

	public bool IsVendorTable { get; set; }

	public ReadonlyList<ItemEntity> Items => m_Items;

	public bool IsPlayerInventory => this == Game.Instance.Player.Inventory;

	public bool IsSharedStash => this == Game.Instance.Player.SharedStash;

	public bool HasLoot => m_Items.HasItem((ItemEntity i) => i.IsLootable && i.IsAvailable() && (i.HoldingSlot?.CanRemoveItem() ?? true));

	[JsonConstructor]
	private ItemsCollection()
	{
	}

	public ItemsCollection(Entity owner)
	{
		m_OwnerRef = owner?.Ref ?? default(EntityRef);
		IsPostLoadExecuted = true;
		IsPostLoadFixesExecuted = true;
	}

	public IEnumerator<ItemEntity> GetEnumerator()
	{
		return m_Items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Contains(BlueprintItem item, int count = 1)
	{
		int num = 0;
		for (int i = 0; i < m_Items.Count; i++)
		{
			ItemEntity itemEntity = m_Items[i];
			if (itemEntity.Blueprint == item)
			{
				num += itemEntity.Count;
				if (num >= count)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ContainsAny(IList<BlueprintItem> items)
	{
		for (int i = 0; i < m_Items.Count; i++)
		{
			ItemEntity item = m_Items[i];
			if (items.HasItem((BlueprintItem ii) => ii == item.Blueprint))
			{
				return true;
			}
		}
		return false;
	}

	public void Insert(ItemEntity item)
	{
		if (item.Collection != null)
		{
			PFLog.Default.Error($"Item {item} already in collection now");
			item.Collection.Remove(item);
		}
		if (m_Items.Contains(item))
		{
			PFLog.Default.Error($"Collection already contains item {item} (owner: {OwnerUnit})");
		}
		else
		{
			m_Items.Add(item);
		}
		item.Collection = this;
		item.UpdateSlotIndex();
		DeltaWeight(item.TotalWeight);
		if (!item.IsStackable && item.Count > 1)
		{
			int num = item.Count - 1;
			item.DecrementCount(num, force: true);
			for (int i = 0; i < num; i++)
			{
				Add(item.Blueprint)?.UpdateSlotIndex();
			}
		}
	}

	public void Extract(ItemEntity item)
	{
		m_Items.Remove(item);
		item.Collection = null;
		if (!ContextData<DoNotRemoveFromSlot>.Current)
		{
			item.HoldingSlot?.RemoveItem();
		}
		item.UpdateSlotIndex();
		DeltaWeight(0f - item.TotalWeight);
	}

	public ItemEntity Add(ItemEntity newItem, bool noAutoMerge = false)
	{
		newItem.SetOriginAreaIfNull(Game.Instance.CurrentlyLoadedArea);
		if (IsPlayerInventory)
		{
			MoneyReplacement component = newItem.Blueprint.GetComponent<MoneyReplacement>();
			if (component != null)
			{
				long amount = component.Cost * newItem.Count;
				Game.Instance.Player.GainMoney(amount);
			}
			ScrapReplacement component2 = newItem.Blueprint.GetComponent<ScrapReplacement>();
			if (component2 != null)
			{
				int scrap = component2.Cost * newItem.Count;
				Game.Instance.Player.Scrap.Receive(scrap);
			}
			BuildPointsReplacement component3 = newItem.Blueprint.GetComponent<BuildPointsReplacement>();
			if (component != null || component3 != null || component2 != null)
			{
				OnItemAdded(newItem, newItem.Count);
				newItem.Dispose();
				return null;
			}
			newItem.Time = Game.Instance.Player.GameTime;
		}
		int count = newItem.Count;
		if ((newItem.IsStackable || ForceStackable) && !noAutoMerge)
		{
			foreach (ItemEntity item in m_Items)
			{
				if (item.TryMerge(newItem))
				{
					OnItemAdded(item, count);
					return item;
				}
			}
		}
		Insert(newItem);
		OnItemAdded(newItem, count);
		return newItem;
	}

	public void Add(BlueprintItem newBpItem, int count, [CanBeNull] Action<ItemEntity> callback = null, bool noAutoMerge = false)
	{
		if (newBpItem.IsActuallyStackable || ForceStackable)
		{
			ItemEntity itemEntity = newBpItem.CreateEntity();
			itemEntity.IncrementCount(count - 1, ForceStackable);
			itemEntity = Add(itemEntity, noAutoMerge);
			try
			{
				callback?.Invoke(itemEntity);
				return;
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				return;
			}
		}
		while (count-- > 0)
		{
			ItemEntity obj = Add(newBpItem.CreateEntity(), noAutoMerge);
			try
			{
				callback?.Invoke(obj);
			}
			catch (Exception ex2)
			{
				PFLog.Default.Exception(ex2);
			}
		}
	}

	public ItemEntity Add(BlueprintItem newBpItem)
	{
		return Add(newBpItem.CreateEntity());
	}

	public ItemEntity Remove(ItemEntity item, int? count = null)
	{
		int valueOrDefault = count.GetValueOrDefault();
		if (!count.HasValue)
		{
			valueOrDefault = item.Count;
			count = valueOrDefault;
		}
		ItemEntity itemEntity = ((!item.IsStackable || item.Count == count) ? item : item.Split(count.Value));
		if (itemEntity.Collection != null)
		{
			Extract(itemEntity);
		}
		OnItemsRemoved(item, count.Value);
		return itemEntity;
	}

	public void Remove(BlueprintItem bpItem, int count = 1)
	{
		if (IsPlayerInventory)
		{
			MoneyReplacement component = bpItem.GetComponent<MoneyReplacement>();
			if ((bool)component)
			{
				long amount = component.Cost * count;
				Game.Instance.Player.SpendMoney(amount);
				ItemEntity itemEntity = bpItem.CreateEntity();
				OnItemsRemoved(itemEntity, count);
				itemEntity.Dispose();
				return;
			}
		}
		int num = count;
		for (int num2 = m_Items.Count - 1; num2 >= 0; num2--)
		{
			ItemEntity itemEntity2 = m_Items[num2];
			if (itemEntity2.Blueprint == bpItem)
			{
				ItemSlot holdingSlot = itemEntity2.HoldingSlot;
				if (holdingSlot != null && !holdingSlot.RemoveItem())
				{
					PFLog.Default.Error("Can't remove {0} of {1}: item equipped and non-removable", num, bpItem);
				}
				else
				{
					int num3 = Math.Min(count, itemEntity2.Count);
					Remove(itemEntity2, num3);
					count -= num3;
					if (count < 1)
					{
						break;
					}
				}
			}
		}
		if (count > 0)
		{
			PFLog.Default.Error("Can't remove {0} of {1} (removed only {2})", num, bpItem, num - count);
		}
	}

	public void RemoveAll()
	{
		for (int num = m_Items.Count - 1; num >= 0; num--)
		{
			ItemEntity item = m_Items[num];
			Remove(item);
		}
	}

	public int RemoveAll([NotNull] BlueprintItem blueprint)
	{
		int num = 0;
		for (int num2 = m_Items.Count - 1; num2 >= 0; num2--)
		{
			ItemEntity itemEntity = m_Items[num2];
			if (itemEntity?.Blueprint == blueprint)
			{
				num += itemEntity.Count;
				Remove(itemEntity);
			}
		}
		return num;
	}

	public ItemEntity Transfer(ItemEntity item, int count, ItemsCollection to)
	{
		return Transfer(item, count, to, noAutoMerge: false);
	}

	public ItemEntity TransferWithoutMerge(ItemEntity item, int count, ItemsCollection to)
	{
		return Transfer(item, count, to, noAutoMerge: true);
	}

	public ItemEntity Transfer(ItemEntity item, ItemsCollection to)
	{
		return Transfer(item, item.Count, to);
	}

	private ItemEntity Transfer(ItemEntity item, int count, ItemsCollection to, bool noAutoMerge)
	{
		if (!ContextData<DoNotRemoveFromSlot>.Current)
		{
			ItemSlot holdingSlot = item.HoldingSlot;
			if (holdingSlot != null && !holdingSlot.CanRemoveItem())
			{
				return null;
			}
		}
		if (to.IsPlayerInventory)
		{
			item.SetToCargoAutomatically(toCargo: false);
		}
		ItemEntity itemEntity = Remove(item, count);
		to.Add(itemEntity, noAutoMerge);
		return itemEntity;
	}

	protected virtual void OnItemAdded(ItemEntity item, int count)
	{
		if (!ContextData<SuppressEvents>.Current)
		{
			EventBus.RaiseEvent(delegate(IItemsCollectionHandler l)
			{
				l.HandleItemsAdded(this, item, count);
			});
		}
		if (IsPlayerInventory)
		{
			item.TryIdentify();
		}
	}

	protected virtual void OnItemsRemoved(ItemEntity item, int count)
	{
		if (!ContextData<SuppressEvents>.Current)
		{
			EventBus.RaiseEvent(delegate(IItemsCollectionHandler l)
			{
				l.HandleItemsRemoved(this, item, count);
			});
		}
	}

	public void ValidateInventorySlotIndices()
	{
		List<ItemEntity> list = new List<ItemEntity>();
		list.AddRange(Enumerable.Repeat<ItemEntity>(null, Items.Count));
		int num = 0;
		foreach (ItemEntity item in Items)
		{
			if (item.InventorySlotIndex >= 0)
			{
				int inventorySlotIndex = item.InventorySlotIndex;
				if (inventorySlotIndex > num)
				{
					num = inventorySlotIndex;
				}
				if (list.Count <= inventorySlotIndex)
				{
					list.AddRange(Enumerable.Repeat<ItemEntity>(null, num + 1));
				}
				if (list[inventorySlotIndex] != null)
				{
					PFLog.Default.Error("More than one items with index {0} in items collection", inventorySlotIndex);
				}
				list[inventorySlotIndex] = item;
			}
		}
	}

	public void PreSave()
	{
		foreach (ItemEntity item in m_Items)
		{
			try
			{
				item.PreSave();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public void PrePostLoad()
	{
		foreach (ItemEntity item in m_Items)
		{
			item.Collection = this;
			item.PrePostLoad();
		}
	}

	public void PostLoad()
	{
		if (IsPostLoadExecuted)
		{
			return;
		}
		ValidateInventoryItems();
		bool flag = IsPlayerInventory || this == Game.Instance.Player.SharedStash;
		if (flag)
		{
			m_Items = m_Items.Distinct().ToList();
		}
		List<ItemEntity> list = null;
		foreach (ItemEntity item in m_Items)
		{
			try
			{
				if (flag)
				{
					item.UpdateSlotIndex();
				}
				if (item.Count < 1 || item.Collection != this)
				{
					if (list == null)
					{
						list = new List<ItemEntity>();
					}
					list.Add(item);
				}
				if (!item.IsPostLoadExecuted)
				{
					item.PostLoad();
				}
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		if (list != null)
		{
			foreach (ItemEntity item2 in list)
			{
				PFLog.Default.Error($"Remove invalid item: {item2}");
				if (item2.Collection == this)
				{
					Remove(item2);
				}
				else
				{
					m_Items.Remove(item2);
				}
				if (item2.HoldingSlot != null)
				{
					using (ContextData<ItemSlot.IgnoreLock>.Request())
					{
						PFLog.Default.Error($"Remove invalid item from slot: {item2}");
						item2.HoldingSlot.RemoveItem();
					}
				}
			}
		}
		if (flag)
		{
			foreach (ItemEntity item3 in m_Items)
			{
				if (item3.InventorySlotIndex < 0)
				{
					continue;
				}
				foreach (ItemEntity item4 in m_Items)
				{
					if (item3 != item4 && item3.InventorySlotIndex == item4.InventorySlotIndex)
					{
						PFLog.Default.Error($"Restore item's inventory slot index: {item3}");
						item3.UpdateSlotIndex(force: true);
						break;
					}
				}
			}
		}
		UpdateWeight();
		IsPostLoadExecuted = true;
	}

	public void ApplyPostLoadFixes()
	{
		if (IsPostLoadFixesExecuted)
		{
			return;
		}
		foreach (ItemEntity item in m_Items)
		{
			try
			{
				item.ApplyPostLoadFixes();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		IsPostLoadFixesExecuted = true;
	}

	private void ValidateInventoryItems()
	{
		using PooledHashSet<ItemEntity> pooledHashSet = PooledHashSet<ItemEntity>.Get(m_Items);
		using PooledHashSet<ItemEntity> pooledHashSet2 = PooledHashSet<ItemEntity>.Get();
		StringBuilder stringBuilder = null;
		for (int num = pooledHashSet.Count - 1; num >= 0; num--)
		{
			ItemEntity itemEntity = m_Items[num];
			if (itemEntity.HoldingSlot != null && itemEntity.HoldingSlot.Owner == null)
			{
				m_Items.RemoveAt(num);
				pooledHashSet2.Add(itemEntity);
				stringBuilder = stringBuilder ?? new StringBuilder();
				stringBuilder.AppendLine($"{itemEntity} was removed. Reason: item.HoldingSlot != null && x.HoldingSlot.Owner == null");
			}
			if (!itemEntity.OnPostLoadValidation())
			{
				m_Items.RemoveAt(num);
				pooledHashSet2.Add(itemEntity);
				stringBuilder = stringBuilder ?? new StringBuilder();
				stringBuilder.AppendLine($"{itemEntity} was removed. Reason: invalid Blueprint type or unexist blueprint");
			}
		}
		if (pooledHashSet2.Count > 0)
		{
			stringBuilder.Insert(0, "ItemEntity.PostLoad: Collection(owner_id: " + m_OwnerRef.Id + ") contains invalid items. Removed items: \n");
			PFLog.Default.Error(stringBuilder.ToString());
		}
	}

	public void Dispose()
	{
		IsDisposingNow = true;
		try
		{
			foreach (ItemEntity item in m_Items)
			{
				try
				{
					item.Dispose();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
		finally
		{
			IsDisposingNow = false;
		}
	}

	public bool DropItem(ItemEntity item)
	{
		if (item.Collection != this)
		{
			return false;
		}
		IAbstractUnitEntity player = Game.Instance.Player.MainCharacter.Entity;
		DroppedLoot droppedLoot = UnityEngine.Object.FindObjectsOfType<DroppedLoot>().FirstOrDefault((DroppedLoot o) => o.IsDroppedByPlayer && player.DistanceTo(o.ViewTransform.position) < 5.Feet().Meters);
		if (!droppedLoot)
		{
			droppedLoot = Game.Instance.EntitySpawner.SpawnEntityWithView(BlueprintRoot.Instance.Prefabs.DroppedLootBag, player.Position, player.ViewTransform.rotation, Game.Instance.State.LoadedAreaState.MainState);
			droppedLoot.Loot = new ItemsCollection(droppedLoot.Data);
			droppedLoot.DroppedBy = (BaseUnitEntity)player;
		}
		Transfer(item, droppedLoot.Loot);
		return true;
	}

	public void Subscribe()
	{
		foreach (ItemEntity item in m_Items)
		{
			item.Subscribe();
		}
	}

	public void Unsubscribe()
	{
		foreach (ItemEntity item in m_Items)
		{
			item.Unsubscribe();
		}
	}

	private void UpdateWeight()
	{
		if (!IsPlayerInventory)
		{
			return;
		}
		foreach (ItemEntity item in m_Items)
		{
			Weight += item.TotalWeight;
		}
	}

	public void DeltaWeight(float deltaWeight)
	{
		if (IsPlayerInventory)
		{
			Weight += deltaWeight;
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<ItemEntity> items = m_Items;
		if (items != null)
		{
			for (int i = 0; i < items.Count; i++)
			{
				Hash128 val = ClassHasher<ItemEntity>.GetHash128(items[i]);
				result.Append(ref val);
			}
		}
		EntityRef obj = m_OwnerRef;
		Hash128 val2 = EntityRefHasher.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
