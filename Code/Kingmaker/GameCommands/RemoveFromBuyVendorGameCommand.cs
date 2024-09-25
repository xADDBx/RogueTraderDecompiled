using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class RemoveFromBuyVendorGameCommand : GameCommand, IMemoryPackable<RemoveFromBuyVendorGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RemoveFromBuyVendorGameCommandFormatter : MemoryPackFormatter<RemoveFromBuyVendorGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RemoveFromBuyVendorGameCommand value)
		{
			RemoveFromBuyVendorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RemoveFromBuyVendorGameCommand value)
		{
			RemoveFromBuyVendorGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_Count;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private RemoveFromBuyVendorGameCommand()
	{
	}

	[JsonConstructor]
	public RemoveFromBuyVendorGameCommand(ItemEntity itemEntity, int count)
	{
		m_Item = itemEntity;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.RemoveFromBuy(m_Item, m_Count);
	}

	static RemoveFromBuyVendorGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveFromBuyVendorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RemoveFromBuyVendorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveFromBuyVendorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RemoveFromBuyVendorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RemoveFromBuyVendorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Item);
		writer.WriteUnmanaged(in value.m_Count);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RemoveFromBuyVendorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		int value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				value3 = value.m_Count;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				goto IL_00a1;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			reader.ReadUnmanaged<int>(out value3);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RemoveFromBuyVendorGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<ItemEntity>);
				value3 = 0;
			}
			else
			{
				value2 = value.m_Item;
				value3 = value.m_Count;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a1;
			}
		}
		value = new RemoveFromBuyVendorGameCommand
		{
			m_Item = value2,
			m_Count = value3
		};
		return;
		IL_00a1:
		value.m_Item = value2;
		value.m_Count = value3;
	}
}
