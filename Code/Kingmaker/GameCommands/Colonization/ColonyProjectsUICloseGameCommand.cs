using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands.Colonization;

[MemoryPackable(GenerateType.Object)]
public class ColonyProjectsUICloseGameCommand : GameCommand, IMemoryPackable<ColonyProjectsUICloseGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ColonyProjectsUICloseGameCommandFormatter : MemoryPackFormatter<ColonyProjectsUICloseGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ColonyProjectsUICloseGameCommand value)
		{
			ColonyProjectsUICloseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ColonyProjectsUICloseGameCommand value)
		{
			ColonyProjectsUICloseGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	[JsonConstructor]
	public ColonyProjectsUICloseGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler h)
		{
			h.HandleColonyProjectsUIClose();
		});
	}

	static ColonyProjectsUICloseGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ColonyProjectsUICloseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ColonyProjectsUICloseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ColonyProjectsUICloseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ColonyProjectsUICloseGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ColonyProjectsUICloseGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref ColonyProjectsUICloseGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ColonyProjectsUICloseGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new ColonyProjectsUICloseGameCommand();
	}
}
