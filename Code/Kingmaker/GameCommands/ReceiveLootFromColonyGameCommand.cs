using Kingmaker.Globalmap.Colonization;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class ReceiveLootFromColonyGameCommand : GameCommand, IMemoryPackable<ReceiveLootFromColonyGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ReceiveLootFromColonyGameCommandFormatter : MemoryPackFormatter<ReceiveLootFromColonyGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ReceiveLootFromColonyGameCommand value)
		{
			ReceiveLootFromColonyGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ReceiveLootFromColonyGameCommand value)
		{
			ReceiveLootFromColonyGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly ColonyRef m_ColonyRef;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	public ReceiveLootFromColonyGameCommand(ColonyRef m_colonyRef)
	{
		m_ColonyRef = m_colonyRef;
	}

	protected override void ExecuteInternal()
	{
		if (!m_ColonyRef.TryGet(out var colony))
		{
			PFLog.Net.Error("[SelectColonyGameCommand] Colony not found! " + m_ColonyRef);
		}
		else
		{
			colony.LootToReceive.RewardItems();
		}
	}

	static ReceiveLootFromColonyGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ReceiveLootFromColonyGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ReceiveLootFromColonyGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ReceiveLootFromColonyGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ReceiveLootFromColonyGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ReceiveLootFromColonyGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_ColonyRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ReceiveLootFromColonyGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ColonyRef value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<ColonyRef>();
			}
			else
			{
				value2 = value.m_ColonyRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ReceiveLootFromColonyGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_ColonyRef : default(ColonyRef));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new ReceiveLootFromColonyGameCommand(value2);
	}
}
