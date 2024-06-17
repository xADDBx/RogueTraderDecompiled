using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CloseChangeGroupGameCommand : GameCommand, IMemoryPackable<CloseChangeGroupGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CloseChangeGroupGameCommandFormatter : MemoryPackFormatter<CloseChangeGroupGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CloseChangeGroupGameCommand value)
		{
			CloseChangeGroupGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CloseChangeGroupGameCommand value)
		{
			CloseChangeGroupGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	[JsonConstructor]
	public CloseChangeGroupGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICloseChangeGroupHandler h)
		{
			h.HandleCloseChangeGroup();
		});
	}

	static CloseChangeGroupGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CloseChangeGroupGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CloseChangeGroupGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CloseChangeGroupGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CloseChangeGroupGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CloseChangeGroupGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteObjectHeader(0);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CloseChangeGroupGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		if (memberCount == 0)
		{
			if (value != null)
			{
				return;
			}
		}
		else
		{
			if (memberCount > 0)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CloseChangeGroupGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new CloseChangeGroupGameCommand();
	}
}
