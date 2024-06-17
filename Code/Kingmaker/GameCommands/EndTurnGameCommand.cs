using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class EndTurnGameCommand : GameCommand, IMemoryPackable<EndTurnGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class EndTurnGameCommandFormatter : MemoryPackFormatter<EndTurnGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EndTurnGameCommand value)
		{
			EndTurnGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EndTurnGameCommand value)
		{
			EndTurnGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	protected override void ExecuteInternal()
	{
		Game.Instance.TurnController.TryEndPlayerTurn();
	}

	static EndTurnGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EndTurnGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new EndTurnGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EndTurnGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EndTurnGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EndTurnGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref EndTurnGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EndTurnGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new EndTurnGameCommand();
	}
}
