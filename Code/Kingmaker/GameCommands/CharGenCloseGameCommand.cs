using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenCloseGameCommand : GameCommand, IMemoryPackable<CharGenCloseGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenCloseGameCommandFormatter : MemoryPackFormatter<CharGenCloseGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenCloseGameCommand value)
		{
			CharGenCloseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenCloseGameCommand value)
		{
			CharGenCloseGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly bool m_WithComplete;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly bool m_SyncPortrait;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenCloseGameCommand(bool m_withComplete, bool m_syncPortrait)
	{
		m_WithComplete = m_withComplete;
		m_SyncPortrait = m_syncPortrait;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenCloseHandler h)
		{
			h.HandleClose(m_WithComplete, m_SyncPortrait);
		});
	}

	static CharGenCloseGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenCloseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenCloseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenCloseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenCloseGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenCloseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_WithComplete, in value.m_SyncPortrait);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenCloseGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<bool, bool>(out value2, out value3);
			}
			else
			{
				value2 = value.m_WithComplete;
				value3 = value.m_SyncPortrait;
				reader.ReadUnmanaged<bool>(out value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenCloseGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = false;
				value3 = false;
			}
			else
			{
				value2 = value.m_WithComplete;
				value3 = value.m_SyncPortrait;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenCloseGameCommand(value2, value3);
	}
}
