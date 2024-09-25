using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SetPauseGameCommand : GameCommand, IMemoryPackable<SetPauseGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetPauseGameCommandFormatter : MemoryPackFormatter<SetPauseGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetPauseGameCommand value)
		{
			SetPauseGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetPauseGameCommand value)
		{
			SetPauseGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_ToPause;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	protected SetPauseGameCommand()
	{
	}

	[MemoryPackConstructor]
	public SetPauseGameCommand(bool m_toPause)
	{
		m_ToPause = m_toPause;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.PauseController.SetManualPause(m_ToPause);
	}

	static SetPauseGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetPauseGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetPauseGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetPauseGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetPauseGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetPauseGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_ToPause);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetPauseGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<bool>(out value2);
			}
			else
			{
				value2 = value.m_ToPause;
				reader.ReadUnmanaged<bool>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetPauseGameCommand), 1, memberCount);
				return;
			}
			value2 = value != null && value.m_ToPause;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new SetPauseGameCommand(value2);
	}
}
