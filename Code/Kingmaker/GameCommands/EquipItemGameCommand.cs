using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class EquipItemGameCommand : GameCommand, IMemoryPackable<EquipItemGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class EquipItemGameCommandFormatter : MemoryPackFormatter<EquipItemGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EquipItemGameCommand value)
		{
			EquipItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EquipItemGameCommand value)
		{
			EquipItemGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<MechanicEntity> m_Entity;

	[JsonProperty]
	[MemoryPackInclude]
	private ItemSlotRef m_To;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private EquipItemGameCommand()
	{
	}

	[JsonConstructor]
	public EquipItemGameCommand(ItemEntity item, MechanicEntity entity, ItemSlotRef to)
	{
		m_Item = new EntityRef<ItemEntity>(item);
		m_Entity = entity;
		m_To = to;
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null && m_Entity.Entity != null)
		{
			ItemSlot itemSlot;
			if (m_To == null)
			{
				GameCommandHelper.EquipItemAutomatically(m_Item, m_Entity.Entity as BaseUnitEntity);
			}
			else if (GameCommandHelper.TryGetEquipSlot(m_Entity.Entity, m_To, out itemSlot))
			{
				GameCommandHelper.TryInsertItem(itemSlot, m_Item.Entity);
			}
		}
	}

	static EquipItemGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EquipItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new EquipItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EquipItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EquipItemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EquipItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_Item);
		writer.WritePackable(in value.m_Entity);
		writer.WritePackable(in value.m_To);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EquipItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		EntityRef<MechanicEntity> value3;
		ItemSlotRef value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				value3 = value.m_Entity;
				value4 = value.m_To;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadPackable(ref value4);
				goto IL_00d3;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			value3 = reader.ReadPackable<EntityRef<MechanicEntity>>();
			value4 = reader.ReadPackable<ItemSlotRef>();
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EquipItemGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<ItemEntity>);
				value3 = default(EntityRef<MechanicEntity>);
				value4 = null;
			}
			else
			{
				value2 = value.m_Item;
				value3 = value.m_Entity;
				value4 = value.m_To;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00d3;
			}
		}
		value = new EquipItemGameCommand
		{
			m_Item = value2,
			m_Entity = value3,
			m_To = value4
		};
		return;
		IL_00d3:
		value.m_Item = value2;
		value.m_Entity = value3;
		value.m_To = value4;
	}
}
