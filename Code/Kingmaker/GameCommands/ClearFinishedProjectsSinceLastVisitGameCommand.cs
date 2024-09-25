using Kingmaker.Globalmap.Colonization;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class ClearFinishedProjectsSinceLastVisitGameCommand : GameCommand, IMemoryPackable<ClearFinishedProjectsSinceLastVisitGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ClearFinishedProjectsSinceLastVisitGameCommandFormatter : MemoryPackFormatter<ClearFinishedProjectsSinceLastVisitGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ClearFinishedProjectsSinceLastVisitGameCommand value)
		{
			ClearFinishedProjectsSinceLastVisitGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ClearFinishedProjectsSinceLastVisitGameCommand value)
		{
			ClearFinishedProjectsSinceLastVisitGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly ColonyRef m_ColonyRef;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	public ClearFinishedProjectsSinceLastVisitGameCommand(ColonyRef m_colonyRef)
	{
		m_ColonyRef = m_colonyRef;
	}

	protected override void ExecuteInternal()
	{
		if (!m_ColonyRef.TryGet(out var colony))
		{
			PFLog.Net.Error("[StartChronicleGameCommand] Colony not found! " + m_ColonyRef);
		}
		else
		{
			colony.ClearFinishedProjectsSinceLastVisit();
		}
	}

	static ClearFinishedProjectsSinceLastVisitGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ClearFinishedProjectsSinceLastVisitGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ClearFinishedProjectsSinceLastVisitGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ClearFinishedProjectsSinceLastVisitGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ClearFinishedProjectsSinceLastVisitGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ClearFinishedProjectsSinceLastVisitGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref ClearFinishedProjectsSinceLastVisitGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ClearFinishedProjectsSinceLastVisitGameCommand), 1, memberCount);
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
		value = new ClearFinishedProjectsSinceLastVisitGameCommand(value2);
	}
}
