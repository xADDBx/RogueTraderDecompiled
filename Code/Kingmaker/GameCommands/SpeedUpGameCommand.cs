using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SpeedUpGameCommand : GameCommand, IMemoryPackable<SpeedUpGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SpeedUpGameCommandFormatter : MemoryPackFormatter<SpeedUpGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SpeedUpGameCommand value)
		{
			SpeedUpGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SpeedUpGameCommand value)
		{
			SpeedUpGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public SpeedUpGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.UISettings.DoSpeedUp();
	}

	static SpeedUpGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SpeedUpGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SpeedUpGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SpeedUpGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SpeedUpGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SpeedUpGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref SpeedUpGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SpeedUpGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new SpeedUpGameCommand();
	}
}
