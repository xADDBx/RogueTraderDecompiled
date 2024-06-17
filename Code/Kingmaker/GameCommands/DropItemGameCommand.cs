using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class DropItemGameCommand : GameCommand, IMemoryPackable<DropItemGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DropItemGameCommandFormatter : MemoryPackFormatter<DropItemGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DropItemGameCommand value)
		{
			DropItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DropItemGameCommand value)
		{
			DropItemGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_Split;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_SplitCount;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private DropItemGameCommand()
	{
	}

	[JsonConstructor]
	public DropItemGameCommand(ItemEntity item, bool split, int splitCount)
	{
		m_Item = new EntityRef<ItemEntity>(item);
		m_Split = split;
		m_SplitCount = splitCount;
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null)
		{
			GameCommandHelper.DropItem(m_Item.Entity, m_Split, m_SplitCount);
		}
	}

	static DropItemGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DropItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new DropItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DropItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DropItemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DropItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_Item);
		writer.WriteUnmanaged(in value.m_Split, in value.m_SplitCount);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DropItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		bool value3;
		int value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				value3 = value.m_Split;
				value4 = value.m_SplitCount;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<bool>(out value3);
				reader.ReadUnmanaged<int>(out value4);
				goto IL_00c9;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			reader.ReadUnmanaged<bool, int>(out value3, out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DropItemGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<ItemEntity>);
				value3 = false;
				value4 = 0;
			}
			else
			{
				value2 = value.m_Item;
				value3 = value.m_Split;
				value4 = value.m_SplitCount;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c9;
			}
		}
		value = new DropItemGameCommand
		{
			m_Item = value2,
			m_Split = value3,
			m_SplitCount = value4
		};
		return;
		IL_00c9:
		value.m_Item = value2;
		value.m_Split = value3;
		value.m_SplitCount = value4;
	}
}
