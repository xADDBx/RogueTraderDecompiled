using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class PartyFormationResetGameCommand : GameCommand, IMemoryPackable<PartyFormationResetGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PartyFormationResetGameCommandFormatter : MemoryPackFormatter<PartyFormationResetGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PartyFormationResetGameCommand value)
		{
			PartyFormationResetGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PartyFormationResetGameCommand value)
		{
			PartyFormationResetGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_FormationIndex;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PartyFormationResetGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PartyFormationResetGameCommand(int m_formationIndex)
	{
		m_FormationIndex = m_formationIndex;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.FormationManager.ResetCustomFormation(m_FormationIndex);
	}

	static PartyFormationResetGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationResetGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PartyFormationResetGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationResetGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PartyFormationResetGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PartyFormationResetGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref PartyFormationResetGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PartyFormationResetGameCommand), 1, memberCount);
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
		value = new PartyFormationResetGameCommand(value2);
	}
}
