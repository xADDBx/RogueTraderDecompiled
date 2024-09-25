using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class ClearPointerModeGameCommand : GameCommand, IMemoryPackable<ClearPointerModeGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ClearPointerModeGameCommandFormatter : MemoryPackFormatter<ClearPointerModeGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ClearPointerModeGameCommand value)
		{
			ClearPointerModeGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ClearPointerModeGameCommand value)
		{
			ClearPointerModeGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public ClearPointerModeGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.ClickEventsController.ClearPointerMode();
	}

	static ClearPointerModeGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ClearPointerModeGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ClearPointerModeGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ClearPointerModeGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ClearPointerModeGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ClearPointerModeGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref ClearPointerModeGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ClearPointerModeGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new ClearPointerModeGameCommand();
	}
}
