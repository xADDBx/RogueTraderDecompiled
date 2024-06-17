using Kingmaker.Controllers.GlobalMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class StopStarSystemStarShipGameCommand : GameCommandWithSynchronized, IMemoryPackable<StopStarSystemStarShipGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StopStarSystemStarShipGameCommandFormatter : MemoryPackFormatter<StopStarSystemStarShipGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StopStarSystemStarShipGameCommand value)
		{
			StopStarSystemStarShipGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StopStarSystemStarShipGameCommand value)
		{
			StopStarSystemStarShipGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonConstructor]
	[MemoryPackConstructor]
	private StopStarSystemStarShipGameCommand()
	{
	}

	public StopStarSystemStarShipGameCommand(bool isSynchronized)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		StarSystemMapMoveController.StopPlayerShip();
	}

	static StopStarSystemStarShipGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StopStarSystemStarShipGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StopStarSystemStarShipGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StopStarSystemStarShipGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StopStarSystemStarShipGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StopStarSystemStarShipGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref StopStarSystemStarShipGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StopStarSystemStarShipGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new StopStarSystemStarShipGameCommand();
	}
}
