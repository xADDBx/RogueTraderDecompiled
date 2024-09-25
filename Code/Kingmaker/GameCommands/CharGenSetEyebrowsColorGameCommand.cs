using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetEyebrowsColorGameCommand : GameCommand, IMemoryPackable<CharGenSetEyebrowsColorGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetEyebrowsColorGameCommandFormatter : MemoryPackFormatter<CharGenSetEyebrowsColorGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetEyebrowsColorGameCommand value)
		{
			CharGenSetEyebrowsColorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetEyebrowsColorGameCommand value)
		{
			CharGenSetEyebrowsColorGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_Index;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetEyebrowsColorGameCommand(int m_index)
	{
		m_Index = m_index;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetEyebrowsColor(m_Index);
		});
	}

	static CharGenSetEyebrowsColorGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetEyebrowsColorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetEyebrowsColorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetEyebrowsColorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetEyebrowsColorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetEyebrowsColorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Index);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetEyebrowsColorGameCommand? value)
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
				value2 = value.m_Index;
				reader.ReadUnmanaged<int>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetEyebrowsColorGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Index : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSetEyebrowsColorGameCommand(value2);
	}
}
