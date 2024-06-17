using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class ScanOnSectorMapGameCommand : GameCommand, IMemoryPackable<ScanOnSectorMapGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ScanOnSectorMapGameCommandFormatter : MemoryPackFormatter<ScanOnSectorMapGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ScanOnSectorMapGameCommand value)
		{
			ScanOnSectorMapGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ScanOnSectorMapGameCommand value)
		{
			ScanOnSectorMapGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public ScanOnSectorMapGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.SectorMapController.Scan();
	}

	static ScanOnSectorMapGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ScanOnSectorMapGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ScanOnSectorMapGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ScanOnSectorMapGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ScanOnSectorMapGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ScanOnSectorMapGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref ScanOnSectorMapGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ScanOnSectorMapGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new ScanOnSectorMapGameCommand();
	}
}
