using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class ItemsCollectionRef : IMemoryPackable<ItemsCollectionRef>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ItemsCollectionRefFormatter : MemoryPackFormatter<ItemsCollectionRef>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ItemsCollectionRef value)
		{
			ItemsCollectionRef.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ItemsCollectionRef value)
		{
			ItemsCollectionRef.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef m_OwnerRef;

	[JsonProperty]
	[MemoryPackInclude]
	private LootFromPointOfInterestHolderRef m_LootHolderRef;

	[MemoryPackIgnore]
	public ItemsCollection ItemsCollection
	{
		get
		{
			if (m_OwnerRef.Entity == null)
			{
				return null;
			}
			IEntity entity = m_OwnerRef.Entity;
			if (!(entity is StarSystemObjectEntity))
			{
				if (entity is Player { Inventory: var inventory })
				{
					return inventory;
				}
			}
			else if (m_LootHolderRef?.LootHolder?.Items != null)
			{
				return m_LootHolderRef?.LootHolder?.Items;
			}
			if (entity is PartInventory.IOwner owner)
			{
				return owner.Inventory.Collection;
			}
			return m_OwnerRef.Entity.ToEntity().GetOptional<InteractionLootPart>()?.Items;
		}
	}

	[MemoryPackConstructor]
	private ItemsCollectionRef()
	{
	}

	[JsonConstructor]
	public ItemsCollectionRef([CanBeNull] ItemsCollection collection, [CanBeNull] LootFromPointOfInterestHolderRef lootHolderRef = null)
	{
		m_OwnerRef = new EntityRef(collection?.Owner);
		m_LootHolderRef = lootHolderRef;
	}

	public bool Equals(ItemsCollectionRef other)
	{
		if (other != null)
		{
			return m_OwnerRef.Equals(other.m_OwnerRef);
		}
		return false;
	}

	static ItemsCollectionRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ItemsCollectionRef>())
		{
			MemoryPackFormatterProvider.Register(new ItemsCollectionRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ItemsCollectionRef[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ItemsCollectionRef>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ItemsCollectionRef? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_OwnerRef);
		writer.WritePackable(in value.m_LootHolderRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ItemsCollectionRef? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef value2;
		LootFromPointOfInterestHolderRef value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_OwnerRef;
				value3 = value.m_LootHolderRef;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_00a0;
			}
			value2 = reader.ReadPackable<EntityRef>();
			value3 = reader.ReadPackable<LootFromPointOfInterestHolderRef>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ItemsCollectionRef), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef);
				value3 = null;
			}
			else
			{
				value2 = value.m_OwnerRef;
				value3 = value.m_LootHolderRef;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a0;
			}
		}
		value = new ItemsCollectionRef
		{
			m_OwnerRef = value2,
			m_LootHolderRef = value3
		};
		return;
		IL_00a0:
		value.m_OwnerRef = value2;
		value.m_LootHolderRef = value3;
	}
}
