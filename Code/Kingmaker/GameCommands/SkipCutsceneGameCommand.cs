using Kingmaker.Controllers;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SkipCutsceneGameCommand : GameCommand, IMemoryPackable<SkipCutsceneGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SkipCutsceneGameCommandFormatter : MemoryPackFormatter<SkipCutsceneGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SkipCutsceneGameCommand value)
		{
			SkipCutsceneGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SkipCutsceneGameCommand value)
		{
			SkipCutsceneGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	protected override void ExecuteInternal()
	{
		CutsceneController.SkipCutsceneInternal();
	}

	static SkipCutsceneGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SkipCutsceneGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SkipCutsceneGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SkipCutsceneGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SkipCutsceneGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SkipCutsceneGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref SkipCutsceneGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SkipCutsceneGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new SkipCutsceneGameCommand();
	}
}
