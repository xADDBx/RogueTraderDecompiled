using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class TransferItemToInventoryGameCommand : GameCommand, IMemoryPackable<TransferItemToInventoryGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class TransferItemToInventoryGameCommandFormatter : MemoryPackFormatter<TransferItemToInventoryGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref TransferItemToInventoryGameCommand value)
		{
			TransferItemToInventoryGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TransferItemToInventoryGameCommand value)
		{
			TransferItemToInventoryGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private List<EntityRef<ItemEntity>> m_Items;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private TransferItemToInventoryGameCommand()
	{
	}

	[JsonConstructor]
	public TransferItemToInventoryGameCommand(List<EntityRef<ItemEntity>> items)
	{
		m_Items = items;
	}

	protected override void ExecuteInternal()
	{
		foreach (EntityRef<ItemEntity> item in m_Items)
		{
			item.Entity?.SetToCargoAutomatically(toCargo: false);
		}
	}

	static TransferItemToInventoryGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TransferItemToInventoryGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new TransferItemToInventoryGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TransferItemToInventoryGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TransferItemToInventoryGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<EntityRef<ItemEntity>>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<EntityRef<ItemEntity>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref TransferItemToInventoryGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.m_Items));
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TransferItemToInventoryGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<EntityRef<ItemEntity>> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Items;
				ListFormatter.DeserializePackable(ref reader, ref value2);
				goto IL_006a;
			}
			value2 = ListFormatter.DeserializePackable<EntityRef<ItemEntity>>(ref reader);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TransferItemToInventoryGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Items : null);
			if (memberCount != 0)
			{
				ListFormatter.DeserializePackable(ref reader, ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new TransferItemToInventoryGameCommand
		{
			m_Items = value2
		};
		return;
		IL_006a:
		value.m_Items = value2;
	}
}
