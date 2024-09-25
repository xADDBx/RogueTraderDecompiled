using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class TransferItemsToCargoGameCommand : GameCommand, IMemoryPackable<TransferItemsToCargoGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class TransferItemsToCargoGameCommandFormatter : MemoryPackFormatter<TransferItemsToCargoGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref TransferItemsToCargoGameCommand value)
		{
			TransferItemsToCargoGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TransferItemsToCargoGameCommand value)
		{
			TransferItemsToCargoGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private List<EntityRef<ItemEntity>> m_Items;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private TransferItemsToCargoGameCommand()
	{
	}

	[JsonConstructor]
	public TransferItemsToCargoGameCommand(List<EntityRef<ItemEntity>> items)
	{
		m_Items = items;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.CargoState.AddToCargo(m_Items.Select((EntityRef<ItemEntity> x) => x.Entity).ToList());
	}

	static TransferItemsToCargoGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TransferItemsToCargoGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new TransferItemsToCargoGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TransferItemsToCargoGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TransferItemsToCargoGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<EntityRef<ItemEntity>>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<EntityRef<ItemEntity>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref TransferItemsToCargoGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref TransferItemsToCargoGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TransferItemsToCargoGameCommand), 1, memberCount);
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
		value = new TransferItemsToCargoGameCommand
		{
			m_Items = value2
		};
		return;
		IL_006a:
		value.m_Items = value2;
	}
}
