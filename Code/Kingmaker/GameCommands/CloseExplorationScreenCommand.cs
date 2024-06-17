using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class CloseExplorationScreenCommand : GameCommand, IMemoryPackable<CloseExplorationScreenCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CloseExplorationScreenCommandFormatter : MemoryPackFormatter<CloseExplorationScreenCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CloseExplorationScreenCommand value)
		{
			CloseExplorationScreenCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CloseExplorationScreenCommand value)
		{
			CloseExplorationScreenCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IExplorationUIHandler h)
		{
			h.CloseExplorationScreen();
		});
	}

	static CloseExplorationScreenCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CloseExplorationScreenCommand>())
		{
			MemoryPackFormatterProvider.Register(new CloseExplorationScreenCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CloseExplorationScreenCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CloseExplorationScreenCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CloseExplorationScreenCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref CloseExplorationScreenCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CloseExplorationScreenCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new CloseExplorationScreenCommand();
	}
}
