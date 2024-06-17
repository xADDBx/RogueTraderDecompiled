using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class PartyFormationIndexGameCommand : GameCommand, IMemoryPackable<PartyFormationIndexGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PartyFormationIndexGameCommandFormatter : MemoryPackFormatter<PartyFormationIndexGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PartyFormationIndexGameCommand value)
		{
			PartyFormationIndexGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PartyFormationIndexGameCommand value)
		{
			PartyFormationIndexGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_FormationIndex;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PartyFormationIndexGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PartyFormationIndexGameCommand(int m_formationIndex)
	{
		m_FormationIndex = m_formationIndex;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.FormationManager.CurrentFormationIndex = m_FormationIndex;
	}

	static PartyFormationIndexGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationIndexGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PartyFormationIndexGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationIndexGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PartyFormationIndexGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PartyFormationIndexGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_FormationIndex);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PartyFormationIndexGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int>(out value2);
			}
			else
			{
				value2 = value.m_FormationIndex;
				reader.ReadUnmanaged<int>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PartyFormationIndexGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_FormationIndex : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new PartyFormationIndexGameCommand(value2);
	}
}
