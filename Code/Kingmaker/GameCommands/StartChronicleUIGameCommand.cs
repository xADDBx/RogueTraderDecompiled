using Kingmaker.Globalmap.Colonization;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class StartChronicleUIGameCommand : GameCommandWithSynchronized, IMemoryPackable<StartChronicleUIGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StartChronicleUIGameCommandFormatter : MemoryPackFormatter<StartChronicleUIGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StartChronicleUIGameCommand value)
		{
			StartChronicleUIGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StartChronicleUIGameCommand value)
		{
			StartChronicleUIGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly ColonyRef m_ColonyRef;

	public override bool IsForcedSynced => true;

	[MemoryPackConstructor]
	private StartChronicleUIGameCommand(ColonyRef m_colonyRef)
	{
		m_ColonyRef = m_colonyRef;
	}

	public StartChronicleUIGameCommand(Colony colony, bool isSynchronized)
		: this((ColonyRef)colony)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		if (!m_ColonyRef.TryGet(out var colony))
		{
			PFLog.Net.Error("[StartChronicleGameCommand] Colony not found! " + m_ColonyRef);
		}
		else
		{
			ColonyChronicleExtensions.TryStartChronicle(colony);
		}
	}

	static StartChronicleUIGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StartChronicleUIGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StartChronicleUIGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StartChronicleUIGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StartChronicleUIGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StartChronicleUIGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref StartChronicleUIGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StartChronicleUIGameCommand), 1, memberCount);
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
		value = new StartChronicleUIGameCommand(value2);
	}
}
