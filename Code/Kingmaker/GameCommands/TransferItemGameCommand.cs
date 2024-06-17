using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class TransferItemGameCommand : GameCommand, IMemoryPackable<TransferItemGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class TransferItemGameCommandFormatter : MemoryPackFormatter<TransferItemGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref TransferItemGameCommand value)
		{
			TransferItemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TransferItemGameCommand value)
		{
			TransferItemGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private ItemsCollectionRef m_To;

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_Count;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private TransferItemGameCommand()
	{
	}

	[JsonConstructor]
	public TransferItemGameCommand(ItemsCollectionRef to, ItemEntity item, int count)
	{
		m_To = to;
		m_Item = item;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null)
		{
			GameCommandHelper.TransferCount(m_To.ItemsCollection, m_Item, m_Count);
		}
	}

	static TransferItemGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TransferItemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new TransferItemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TransferItemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TransferItemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref TransferItemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_To);
		writer.WritePackable(in value.m_Item);
		writer.WriteUnmanaged(in value.m_Count);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TransferItemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemsCollectionRef value2;
		EntityRef<ItemEntity> value3;
		int value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_To;
				value3 = value.m_Item;
				value4 = value.m_Count;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<int>(out value4);
				goto IL_00ce;
			}
			value2 = reader.ReadPackable<ItemsCollectionRef>();
			value3 = reader.ReadPackable<EntityRef<ItemEntity>>();
			reader.ReadUnmanaged<int>(out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TransferItemGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = default(EntityRef<ItemEntity>);
				value4 = 0;
			}
			else
			{
				value2 = value.m_To;
				value3 = value.m_Item;
				value4 = value.m_Count;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00ce;
			}
		}
		value = new TransferItemGameCommand
		{
			m_To = value2,
			m_Item = value3,
			m_Count = value4
		};
		return;
		IL_00ce:
		value.m_To = value2;
		value.m_Item = value3;
		value.m_Count = value4;
	}
}
