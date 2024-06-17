using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenTryAdvanceStatGameCommand : GameCommand, IMemoryPackable<CharGenTryAdvanceStatGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenTryAdvanceStatGameCommandFormatter : MemoryPackFormatter<CharGenTryAdvanceStatGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenTryAdvanceStatGameCommand value)
		{
			CharGenTryAdvanceStatGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenTryAdvanceStatGameCommand value)
		{
			CharGenTryAdvanceStatGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly StatType m_StatType;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly bool m_Advance;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenTryAdvanceStatGameCommand(StatType m_statType, bool m_advance)
	{
		m_StatType = m_statType;
		m_Advance = m_advance;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAttributesPhaseHandler h)
		{
			h.HandleTryAdvanceStat(m_StatType, m_Advance);
		});
	}

	static CharGenTryAdvanceStatGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenTryAdvanceStatGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenTryAdvanceStatGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenTryAdvanceStatGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenTryAdvanceStatGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StatType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<StatType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenTryAdvanceStatGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_StatType, in value.m_Advance);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenTryAdvanceStatGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		StatType value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<StatType, bool>(out value2, out value3);
			}
			else
			{
				value2 = value.m_StatType;
				value3 = value.m_Advance;
				reader.ReadUnmanaged<StatType>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenTryAdvanceStatGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = StatType.Unknown;
				value3 = false;
			}
			else
			{
				value2 = value.m_StatType;
				value3 = value.m_Advance;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<StatType>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenTryAdvanceStatGameCommand(value2, value3);
	}
}
