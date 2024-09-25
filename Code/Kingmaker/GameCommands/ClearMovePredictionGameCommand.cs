using Kingmaker.Controllers.Units;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class ClearMovePredictionGameCommand : GameCommandWithSynchronized, IMemoryPackable<ClearMovePredictionGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ClearMovePredictionGameCommandFormatter : MemoryPackFormatter<ClearMovePredictionGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ClearMovePredictionGameCommand value)
		{
			ClearMovePredictionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ClearMovePredictionGameCommand value)
		{
			ClearMovePredictionGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonConstructor]
	[MemoryPackConstructor]
	private ClearMovePredictionGameCommand()
	{
	}

	public ClearMovePredictionGameCommand(bool isSynchronized)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		UnitCommandsRunner.CancelMoveCommandLocal();
	}

	static ClearMovePredictionGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ClearMovePredictionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ClearMovePredictionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ClearMovePredictionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ClearMovePredictionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ClearMovePredictionGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref ClearMovePredictionGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ClearMovePredictionGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new ClearMovePredictionGameCommand();
	}
}
