using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SwapSlotsGameCommand : GameCommand, IMemoryPackable<SwapSlotsGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SwapSlotsGameCommandFormatter : MemoryPackFormatter<SwapSlotsGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SwapSlotsGameCommand value)
		{
			SwapSlotsGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SwapSlotsGameCommand value)
		{
			SwapSlotsGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private ItemSlotRef m_From;

	[JsonProperty]
	[MemoryPackInclude]
	private ItemSlotRef m_To;

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<MechanicEntity> m_Owner;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_IsLoot;

	public override bool IsSynchronized
	{
		get
		{
			if (!m_From.IsEquipment && !m_To.IsEquipment)
			{
				return !m_From.ItemsCollectionRef.Equals(m_To.ItemsCollectionRef);
			}
			return true;
		}
	}

	[MemoryPackConstructor]
	private SwapSlotsGameCommand()
	{
	}

	[JsonConstructor]
	public SwapSlotsGameCommand(MechanicEntity entity, ItemSlotRef from, ItemSlotRef to, bool isLoot)
	{
		m_From = from;
		m_To = to;
		m_Owner = new EntityRef<MechanicEntity>(entity);
		m_IsLoot = isLoot;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.TrySwapSlots(m_From, m_To, m_Owner.Entity, m_IsLoot);
	}

	static SwapSlotsGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SwapSlotsGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SwapSlotsGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SwapSlotsGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SwapSlotsGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SwapSlotsGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
		writer.WritePackable(in value.m_Owner);
		writer.WriteUnmanaged(in value.m_IsLoot);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SwapSlotsGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemSlotRef value2;
		ItemSlotRef value3;
		EntityRef<MechanicEntity> value4;
		bool value5;
		if (memberCount == 4)
		{
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_Owner;
				value5 = value.m_IsLoot;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadPackable(ref value4);
				reader.ReadUnmanaged<bool>(out value5);
				goto IL_0101;
			}
			value2 = reader.ReadPackable<ItemSlotRef>();
			value3 = reader.ReadPackable<ItemSlotRef>();
			value4 = reader.ReadPackable<EntityRef<MechanicEntity>>();
			reader.ReadUnmanaged<bool>(out value5);
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SwapSlotsGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = default(EntityRef<MechanicEntity>);
				value5 = false;
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_Owner;
				value5 = value.m_IsLoot;
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
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<bool>(out value5);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0101;
			}
		}
		value = new SwapSlotsGameCommand
		{
			m_From = value2,
			m_To = value3,
			m_Owner = value4,
			m_IsLoot = value5
		};
		return;
		IL_0101:
		value.m_From = value2;
		value.m_To = value3;
		value.m_Owner = value4;
		value.m_IsLoot = value5;
	}
}
