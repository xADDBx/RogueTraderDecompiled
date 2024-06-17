using Kingmaker.Controllers.SpaceCombat;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class ExitSpaceCombatGameCommand : GameCommand, IMemoryPackable<ExitSpaceCombatGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ExitSpaceCombatGameCommandFormatter : MemoryPackFormatter<ExitSpaceCombatGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ExitSpaceCombatGameCommand value)
		{
			ExitSpaceCombatGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ExitSpaceCombatGameCommand value)
		{
			ExitSpaceCombatGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly bool m_ForceOpenVoidshipUpgrade;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private ExitSpaceCombatGameCommand()
	{
	}

	[JsonConstructor]
	public ExitSpaceCombatGameCommand(bool forceOpenVoidshipUpgrade)
	{
		m_ForceOpenVoidshipUpgrade = forceOpenVoidshipUpgrade;
	}

	protected override void ExecuteInternal()
	{
		ExitSpaceCombatController.ExitSpaceCombat(m_ForceOpenVoidshipUpgrade);
	}

	static ExitSpaceCombatGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ExitSpaceCombatGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ExitSpaceCombatGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ExitSpaceCombatGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ExitSpaceCombatGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ExitSpaceCombatGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_ForceOpenVoidshipUpgrade);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ExitSpaceCombatGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		if (memberCount == 1)
		{
			bool value2;
			if (value != null)
			{
				value2 = value.m_ForceOpenVoidshipUpgrade;
				reader.ReadUnmanaged<bool>(out value2);
				return;
			}
			reader.ReadUnmanaged<bool>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ExitSpaceCombatGameCommand), 1, memberCount);
				return;
			}
			bool value2 = value != null && value.m_ForceOpenVoidshipUpgrade;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				return;
			}
		}
		value = new ExitSpaceCombatGameCommand();
	}
}
