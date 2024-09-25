using JetBrains.Annotations;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class DialogAnswerGameCommand : GameCommand, IMemoryPackable<DialogAnswerGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DialogAnswerGameCommandFormatter : MemoryPackFormatter<DialogAnswerGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DialogAnswerGameCommand value)
		{
			DialogAnswerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DialogAnswerGameCommand value)
		{
			DialogAnswerGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private int m_Tick;

	[JsonProperty]
	[MemoryPackInclude]
	private string m_Answer;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private DialogAnswerGameCommand()
	{
	}

	[JsonConstructor]
	public DialogAnswerGameCommand(int tick, [NotNull] string answer)
	{
		m_Tick = tick;
		m_Answer = answer;
	}

	protected override void ExecuteInternal()
	{
		if (!Game.Instance.DialogController.CuePlayScheduled && m_Tick >= Game.Instance.DialogController.CurrentCueUpdateTick)
		{
			Game.Instance.DialogController.SelectAnswer(m_Answer);
		}
	}

	static DialogAnswerGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DialogAnswerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new DialogAnswerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DialogAnswerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DialogAnswerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DialogAnswerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(2, in value.m_Tick);
		writer.WriteString(value.m_Answer);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DialogAnswerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		string answer;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Tick;
				answer = value.m_Answer;
				reader.ReadUnmanaged<int>(out value2);
				answer = reader.ReadString();
				goto IL_0099;
			}
			reader.ReadUnmanaged<int>(out value2);
			answer = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DialogAnswerGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				answer = null;
			}
			else
			{
				value2 = value.m_Tick;
				answer = value.m_Answer;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					answer = reader.ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_0099;
			}
		}
		value = new DialogAnswerGameCommand
		{
			m_Tick = value2,
			m_Answer = answer
		};
		return;
		IL_0099:
		value.m_Tick = value2;
		value.m_Answer = answer;
	}
}
