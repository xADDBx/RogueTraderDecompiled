using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Kingmaker.Signals;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SignalGameCommand : GameCommand, IMemoryPackable<SignalGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SignalGameCommandFormatter : MemoryPackFormatter<SignalGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SignalGameCommand value)
		{
			SignalGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SignalGameCommand value)
		{
			SignalGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly uint Key;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	public SignalGameCommand(uint key)
	{
		Key = key;
	}

	[JsonConstructor]
	private SignalGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		SignalService.Instance.Write(Key, playerOrEmpty);
	}

	static SignalGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SignalGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SignalGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SignalGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SignalGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SignalGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.Key);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SignalGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		uint value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<uint>(out value2);
			}
			else
			{
				value2 = value.Key;
				reader.ReadUnmanaged<uint>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SignalGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.Key : 0u);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<uint>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SignalGameCommand(value2);
	}
}
