using Kingmaker.Controllers;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SkipBarkGameCommand : GameCommand, IMemoryPackable<SkipBarkGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SkipBarkGameCommandFormatter : MemoryPackFormatter<SkipBarkGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SkipBarkGameCommand value)
		{
			SkipBarkGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SkipBarkGameCommand value)
		{
			SkipBarkGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	protected override void ExecuteInternal()
	{
		CutsceneController.SkipBarkBanter();
	}

	static SkipBarkGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SkipBarkGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SkipBarkGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SkipBarkGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SkipBarkGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SkipBarkGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref SkipBarkGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SkipBarkGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new SkipBarkGameCommand();
	}
}
