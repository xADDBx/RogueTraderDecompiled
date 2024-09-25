using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class AddForBuyVendorGameCommand : GameCommand, IMemoryPackable<AddForBuyVendorGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AddForBuyVendorGameCommandFormatter : MemoryPackFormatter<AddForBuyVendorGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AddForBuyVendorGameCommand value)
		{
			AddForBuyVendorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AddForBuyVendorGameCommand value)
		{
			AddForBuyVendorGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_Count;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_MakeDeal;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private AddForBuyVendorGameCommand()
	{
	}

	[JsonConstructor]
	public AddForBuyVendorGameCommand(ItemEntity itemEntity, int count, bool makeDeal)
	{
		m_Item = itemEntity;
		m_Count = count;
		m_MakeDeal = makeDeal;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.AddForBuy(m_Item, m_Count);
		if (m_MakeDeal)
		{
			Game.Instance.Vendor.MakeDealWithCurrentVendor();
		}
	}

	static AddForBuyVendorGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AddForBuyVendorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AddForBuyVendorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AddForBuyVendorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AddForBuyVendorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AddForBuyVendorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_Item);
		writer.WriteUnmanaged(in value.m_Count, in value.m_MakeDeal);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AddForBuyVendorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		int value3;
		bool value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				value3 = value.m_Count;
				value4 = value.m_MakeDeal;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<bool>(out value4);
				goto IL_00c9;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			reader.ReadUnmanaged<int, bool>(out value3, out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AddForBuyVendorGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<ItemEntity>);
				value3 = 0;
				value4 = false;
			}
			else
			{
				value2 = value.m_Item;
				value3 = value.m_Count;
				value4 = value.m_MakeDeal;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00c9;
			}
		}
		value = new AddForBuyVendorGameCommand
		{
			m_Item = value2,
			m_Count = value3,
			m_MakeDeal = value4
		};
		return;
		IL_00c9:
		value.m_Item = value2;
		value.m_Count = value3;
		value.m_MakeDeal = value4;
	}
}
