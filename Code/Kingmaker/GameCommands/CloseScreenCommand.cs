using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CloseScreenCommand : GameCommandWithSynchronized, IMemoryPackable<CloseScreenCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CloseScreenCommandFormatter : MemoryPackFormatter<CloseScreenCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CloseScreenCommand value)
		{
			CloseScreenCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CloseScreenCommand value)
		{
			CloseScreenCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly byte m_Screen;

	[MemoryPackConstructor]
	private CloseScreenCommand(byte m_screen)
	{
		m_Screen = m_screen;
	}

	public CloseScreenCommand(IScreenUIHandler.ScreenType screen, bool isSynchronized)
		: this((byte)screen)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IScreenUIHandler h)
		{
			h.CloseScreen((IScreenUIHandler.ScreenType)m_Screen);
		});
	}

	static CloseScreenCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CloseScreenCommand>())
		{
			MemoryPackFormatterProvider.Register(new CloseScreenCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CloseScreenCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CloseScreenCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CloseScreenCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Screen);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CloseScreenCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		byte value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<byte>(out value2);
			}
			else
			{
				value2 = value.m_Screen;
				reader.ReadUnmanaged<byte>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CloseScreenCommand), 1, memberCount);
				return;
			}
			value2 = (byte)((value != null) ? value.m_Screen : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<byte>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CloseScreenCommand(value2);
	}
}
