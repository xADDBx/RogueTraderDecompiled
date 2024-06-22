using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class RequestPauseGameCommand : GameCommand, IMemoryPackable<RequestPauseGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RequestPauseGameCommandFormatter : MemoryPackFormatter<RequestPauseGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RequestPauseGameCommand value)
		{
			RequestPauseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RequestPauseGameCommand value)
		{
			RequestPauseGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly bool ToPause;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private RequestPauseGameCommand()
	{
	}

	[MemoryPackConstructor]
	public RequestPauseGameCommand(bool toPause)
	{
		ToPause = toPause;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		Game.Instance.PauseController.SetPlayer(playerOrEmpty, ToPause);
	}

	static RequestPauseGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RequestPauseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RequestPauseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RequestPauseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RequestPauseGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RequestPauseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.ToPause);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RequestPauseGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<bool>(out value2);
			}
			else
			{
				value2 = value.ToPause;
				reader.ReadUnmanaged<bool>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RequestPauseGameCommand), 1, memberCount);
				return;
			}
			value2 = value != null && value.ToPause;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new RequestPauseGameCommand(value2);
	}
}
