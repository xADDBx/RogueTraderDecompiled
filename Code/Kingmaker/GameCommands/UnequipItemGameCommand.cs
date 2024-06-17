using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class UnequipItemGameCommand : GameCommand, IMemoryPackable<UnequipItemGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnequipItemGameCommandFormatter : MemoryPackFormatter<UnequipItemGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnequipItemGameCommand value)
		{
			UnequipItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnequipItemGameCommand value)
		{
			UnequipItemGameCommand.Deserialize(ref reader, ref value);
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

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private UnequipItemGameCommand()
	{
	}

	[JsonConstructor]
	public UnequipItemGameCommand(MechanicEntity owner, ItemSlotRef from, ItemSlotRef to)
	{
		m_From = from;
		m_To = to;
		m_Owner = new EntityRef<MechanicEntity>(owner);
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.UnequipItem(m_Owner, m_From, m_To);
	}

	static UnequipItemGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnequipItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new UnequipItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnequipItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnequipItemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UnequipItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
		writer.WritePackable(in value.m_Owner);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnequipItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemSlotRef value2;
		ItemSlotRef value3;
		EntityRef<MechanicEntity> value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_Owner;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadPackable(ref value4);
				goto IL_00cd;
			}
			value2 = reader.ReadPackable<ItemSlotRef>();
			value3 = reader.ReadPackable<ItemSlotRef>();
			value4 = reader.ReadPackable<EntityRef<MechanicEntity>>();
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnequipItemGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = default(EntityRef<MechanicEntity>);
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_Owner;
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
				goto IL_00cd;
			}
		}
		value = new UnequipItemGameCommand
		{
			m_From = value2,
			m_To = value3,
			m_Owner = value4
		};
		return;
		IL_00cd:
		value.m_From = value2;
		value.m_To = value3;
		value.m_Owner = value4;
	}
}
