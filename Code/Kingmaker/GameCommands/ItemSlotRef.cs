using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class ItemSlotRef : IMemoryPackable<ItemSlotRef>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ItemSlotRefFormatter : MemoryPackFormatter<ItemSlotRef>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ItemSlotRef value)
		{
			ItemSlotRef.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ItemSlotRef value)
		{
			ItemSlotRef.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EquipSlotType m_SlotType;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_SetIndex;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_SlotIndex;

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_ItemRef;

	[JsonProperty]
	[MemoryPackInclude]
	private ItemsCollectionRef m_CollectionRef;

	[JsonProperty]
	[MemoryPackInclude]
	private ShipComponentSlotType m_ShipComponentSlotType;

	[JsonProperty]
	[MemoryPackInclude]
	private LootFromPointOfInterestHolderRef m_LootHolderRef;

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintAugmentSlotReference m_isAugment;

	[MemoryPackIgnore]
	public EquipSlotType EquipSlotType => m_SlotType;

	[MemoryPackIgnore]
	public int SetIndex => m_SetIndex;

	[MemoryPackIgnore]
	public int SlotIndex => m_SlotIndex;

	[MemoryPackIgnore]
	public ItemEntity Item => m_ItemRef;

	[MemoryPackIgnore]
	public bool IsEquipment => m_SlotIndex == -1;

	[MemoryPackIgnore]
	public BlueprintAugmentSlot IsAugment => m_isAugment?.Get();

	[MemoryPackIgnore]
	public ItemsCollectionRef ItemsCollectionRef => m_CollectionRef;

	[MemoryPackIgnore]
	public ItemsCollection ItemsCollection => m_CollectionRef.ItemsCollection;

	[MemoryPackIgnore]
	public ShipComponentSlotType ShipComponentSlotType => m_ShipComponentSlotType;

	[MemoryPackConstructor]
	private ItemSlotRef()
	{
	}

	[JsonConstructor]
	public ItemSlotRef(EquipSlotType slotType, int setIndex, int slotIndex, ItemEntity item, ItemsCollection collection, ShipComponentSlotType shipComponentSlotType, LootFromPointOfInterestHolderRef lootHolderRef = null, BlueprintAugmentSlot isAugment = null)
	{
		m_SlotType = slotType;
		m_SetIndex = setIndex;
		m_SlotIndex = slotIndex;
		m_ItemRef = item;
		m_CollectionRef = collection.ToCollectionRef(lootHolderRef);
		m_ShipComponentSlotType = shipComponentSlotType;
		m_LootHolderRef = lootHolderRef;
		m_isAugment = isAugment?.ToReference<BlueprintAugmentSlotReference>();
	}

	static ItemSlotRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ItemSlotRef>())
		{
			MemoryPackFormatterProvider.Register(new ItemSlotRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ItemSlotRef[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ItemSlotRef>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EquipSlotType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<EquipSlotType>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ShipComponentSlotType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<ShipComponentSlotType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ItemSlotRef? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(8, in value.m_SlotType, in value.m_SetIndex, in value.m_SlotIndex);
		writer.WritePackable(in value.m_ItemRef);
		writer.WritePackable(in value.m_CollectionRef);
		writer.WriteUnmanaged(in value.m_ShipComponentSlotType);
		writer.WritePackable(in value.m_LootHolderRef);
		writer.WritePackable(in value.m_isAugment);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ItemSlotRef? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EquipSlotType value2;
		int value3;
		int value4;
		EntityRef<ItemEntity> value5;
		ItemsCollectionRef value6;
		ShipComponentSlotType value7;
		LootFromPointOfInterestHolderRef value8;
		BlueprintAugmentSlotReference value9;
		if (memberCount == 8)
		{
			if (value != null)
			{
				value2 = value.m_SlotType;
				value3 = value.m_SetIndex;
				value4 = value.m_SlotIndex;
				value5 = value.m_ItemRef;
				value6 = value.m_CollectionRef;
				value7 = value.m_ShipComponentSlotType;
				value8 = value.m_LootHolderRef;
				value9 = value.m_isAugment;
				reader.ReadUnmanaged<EquipSlotType>(out value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<int>(out value4);
				reader.ReadPackable(ref value5);
				reader.ReadPackable(ref value6);
				reader.ReadUnmanaged<ShipComponentSlotType>(out value7);
				reader.ReadPackable(ref value8);
				reader.ReadPackable(ref value9);
				goto IL_01be;
			}
			reader.ReadUnmanaged<EquipSlotType, int, int>(out value2, out value3, out value4);
			value5 = reader.ReadPackable<EntityRef<ItemEntity>>();
			value6 = reader.ReadPackable<ItemsCollectionRef>();
			reader.ReadUnmanaged<ShipComponentSlotType>(out value7);
			value8 = reader.ReadPackable<LootFromPointOfInterestHolderRef>();
			value9 = reader.ReadPackable<BlueprintAugmentSlotReference>();
		}
		else
		{
			if (memberCount > 8)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ItemSlotRef), 8, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = EquipSlotType.PrimaryHand;
				value3 = 0;
				value4 = 0;
				value5 = default(EntityRef<ItemEntity>);
				value6 = null;
				value7 = ShipComponentSlotType.PlasmaDrives;
				value8 = null;
				value9 = null;
			}
			else
			{
				value2 = value.m_SlotType;
				value3 = value.m_SetIndex;
				value4 = value.m_SlotIndex;
				value5 = value.m_ItemRef;
				value6 = value.m_CollectionRef;
				value7 = value.m_ShipComponentSlotType;
				value8 = value.m_LootHolderRef;
				value9 = value.m_isAugment;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<EquipSlotType>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						if (memberCount != 3)
						{
							reader.ReadPackable(ref value5);
							if (memberCount != 4)
							{
								reader.ReadPackable(ref value6);
								if (memberCount != 5)
								{
									reader.ReadUnmanaged<ShipComponentSlotType>(out value7);
									if (memberCount != 6)
									{
										reader.ReadPackable(ref value8);
										if (memberCount != 7)
										{
											reader.ReadPackable(ref value9);
											_ = 8;
										}
									}
								}
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_01be;
			}
		}
		value = new ItemSlotRef
		{
			m_SlotType = value2,
			m_SetIndex = value3,
			m_SlotIndex = value4,
			m_ItemRef = value5,
			m_CollectionRef = value6,
			m_ShipComponentSlotType = value7,
			m_LootHolderRef = value8,
			m_isAugment = value9
		};
		return;
		IL_01be:
		value.m_SlotType = value2;
		value.m_SetIndex = value3;
		value.m_SlotIndex = value4;
		value.m_ItemRef = value5;
		value.m_CollectionRef = value6;
		value.m_ShipComponentSlotType = value7;
		value.m_LootHolderRef = value8;
		value.m_isAugment = value9;
	}
}
