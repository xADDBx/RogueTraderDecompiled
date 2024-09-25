using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[JsonObject]
public abstract class PartItemsCollection : EntityPart, IItemsCollection, IEnumerable<ItemEntity>, IEnumerable, IHashable
{
	private ItemsCollection m_Collection;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	private ItemsCollection CollectionConverter
	{
		get
		{
			if (!IsCollectionOwner)
			{
				return null;
			}
			return m_Collection;
		}
		set
		{
			m_Collection = value;
		}
	}

	private bool IsCollectionOwner
	{
		get
		{
			if (m_Collection?.Owner != base.Owner)
			{
				if (IsPlayerInventory)
				{
					UnitPartMainCharacter optional = base.ConcreteOwner.GetOptional<UnitPartMainCharacter>();
					if (optional != null)
					{
						return !optional.Temporary;
					}
					return false;
				}
				return false;
			}
			return true;
		}
	}

	public ItemsCollection Collection => m_Collection;

	public BaseUnitEntity OwnerUnit => base.Owner as BaseUnitEntity;

	public ReadonlyList<ItemEntity> Items => m_Collection.Items;

	public bool ForceStackable => m_Collection.ForceStackable;

	public bool IsVendorTable => m_Collection.IsVendorTable;

	public bool IsPlayerInventory => m_Collection.IsPlayerInventory;

	public bool IsSharedStash => m_Collection.IsSharedStash;

	public virtual bool HasLoot => Collection.HasLoot;

	protected override void OnAttach()
	{
		Setup();
	}

	protected override void OnSubscribe()
	{
		if (IsCollectionOwner)
		{
			m_Collection?.Subscribe();
		}
	}

	protected override void OnUnsubscribe()
	{
		if (IsCollectionOwner)
		{
			m_Collection.Unsubscribe();
		}
	}

	protected override void OnPreSave()
	{
		if (IsCollectionOwner)
		{
			m_Collection.PreSave();
		}
	}

	protected override void OnPrePostLoad()
	{
		if (m_Collection == null)
		{
			Setup();
		}
		else if (IsCollectionOwner)
		{
			m_Collection.PrePostLoad();
		}
	}

	protected override void OnPostLoad()
	{
		if (IsCollectionOwner)
		{
			m_Collection.PostLoad();
		}
	}

	protected override void OnDetach()
	{
		if (IsCollectionOwner)
		{
			m_Collection.Dispose();
		}
	}

	public void Setup()
	{
		ItemsCollection itemsCollection = SetupInternal(m_Collection);
		if (itemsCollection != m_Collection)
		{
			m_Collection = itemsCollection;
			OnCollectionChanged();
		}
	}

	protected abstract ItemsCollection SetupInternal([CanBeNull] ItemsCollection currentCollection);

	protected virtual void OnCollectionChanged()
	{
	}

	public bool Contains(BlueprintItem item, int count = 1)
	{
		return m_Collection.Contains(item, count);
	}

	public bool ContainsAny(IList<BlueprintItem> items)
	{
		return m_Collection.ContainsAny(items);
	}

	public ItemEntity Add(ItemEntity newItem, bool noAutoMerge = false)
	{
		return m_Collection.Add(newItem, noAutoMerge);
	}

	public void Add(BlueprintItem newBpItem, int count, Action<ItemEntity> callback = null, bool noAutoMerge = false)
	{
		m_Collection.Add(newBpItem, count, callback);
	}

	public ItemEntity Add(BlueprintItem newBpItem)
	{
		return m_Collection.Add(newBpItem);
	}

	public ItemEntity Remove(ItemEntity item, int? count = null)
	{
		return m_Collection.Remove(item, count);
	}

	public void Remove(BlueprintItem bpItem, int count = 1)
	{
		m_Collection.Remove(bpItem, count);
	}

	public void RemoveAll()
	{
		m_Collection.RemoveAll();
	}

	public ItemEntity Transfer(ItemEntity item, int count, ItemsCollection to)
	{
		return m_Collection.Transfer(item, count, to);
	}

	public ItemEntity TransferWithoutMerge(ItemEntity item, int count, ItemsCollection to)
	{
		return m_Collection.TransferWithoutMerge(item, count, to);
	}

	public ItemEntity Transfer(ItemEntity item, ItemsCollection to)
	{
		return m_Collection.Transfer(item, to);
	}

	public IEnumerator<ItemEntity> GetEnumerator()
	{
		return m_Collection.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_Collection.GetEnumerator();
	}

	public ItemEntity Transfer(ItemEntity item, int count, PartInventory to)
	{
		return m_Collection.Transfer(item, count, to.m_Collection);
	}

	public ItemEntity TransferWithoutMerge(ItemEntity item, int count, PartInventory to)
	{
		return m_Collection.TransferWithoutMerge(item, count, to.m_Collection);
	}

	public ItemEntity Transfer(ItemEntity item, PartInventory to)
	{
		return m_Collection.Transfer(item, to.m_Collection);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<ItemsCollection>.GetHash128(CollectionConverter);
		result.Append(ref val2);
		return result;
	}
}
