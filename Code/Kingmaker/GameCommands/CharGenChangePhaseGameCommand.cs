using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenChangePhaseGameCommand : GameCommand, IMemoryPackable<CharGenChangePhaseGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenChangePhaseGameCommandFormatter : MemoryPackFormatter<CharGenChangePhaseGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenChangePhaseGameCommand value)
		{
			CharGenChangePhaseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenChangePhaseGameCommand value)
		{
			CharGenChangePhaseGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly CharGenPhaseType m_PhaseType;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenChangePhaseGameCommand(CharGenPhaseType m_phaseType)
	{
		m_PhaseType = m_phaseType;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenChangePhaseHandler h)
		{
			h.HandlePhaseChange(m_PhaseType);
		});
	}

	static CharGenChangePhaseGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangePhaseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenChangePhaseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangePhaseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenChangePhaseGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenPhaseType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CharGenPhaseType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenChangePhaseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_PhaseType);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenChangePhaseGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CharGenPhaseType value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CharGenPhaseType>(out value2);
			}
			else
			{
				value2 = value.m_PhaseType;
				reader.ReadUnmanaged<CharGenPhaseType>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenChangePhaseGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_PhaseType : CharGenPhaseType.Pregen);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CharGenPhaseType>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenChangePhaseGameCommand(value2);
	}
}
