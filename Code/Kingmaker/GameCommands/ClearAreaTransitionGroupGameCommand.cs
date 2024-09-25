using Kingmaker.UnitLogic.Commands;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class ClearAreaTransitionGroupGameCommand : GameCommand, IMemoryPackable<ClearAreaTransitionGroupGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ClearAreaTransitionGroupGameCommandFormatter : MemoryPackFormatter<ClearAreaTransitionGroupGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ClearAreaTransitionGroupGameCommand value)
		{
			ClearAreaTransitionGroupGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ClearAreaTransitionGroupGameCommand value)
		{
			ClearAreaTransitionGroupGameCommand.Deserialize(ref reader, ref value);
		}
	}

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public ClearAreaTransitionGroupGameCommand()
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.GroupCommands.ClearDuplicates(typeof(AreaTransitionGroupCommand));
	}

	static ClearAreaTransitionGroupGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ClearAreaTransitionGroupGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ClearAreaTransitionGroupGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ClearAreaTransitionGroupGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ClearAreaTransitionGroupGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ClearAreaTransitionGroupGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref ClearAreaTransitionGroupGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ClearAreaTransitionGroupGameCommand), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new ClearAreaTransitionGroupGameCommand();
	}
}
