using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class StopSpeedUpGameCommand : GameCommand, IMemoryPackable<StopSpeedUpGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StopSpeedUpGameCommandFormatter : MemoryPackFormatter<StopSpeedUpGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StopSpeedUpGameCommand value)
		{
			StopSpeedUpGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StopSpeedUpGameCommand value)
		{
			StopSpeedUpGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public StopSpeedUpGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.UISettings.StopSpeedUp();
	}

	static StopSpeedUpGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StopSpeedUpGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StopSpeedUpGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StopSpeedUpGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StopSpeedUpGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StopSpeedUpGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref StopSpeedUpGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StopSpeedUpGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new StopSpeedUpGameCommand();
	}
}
