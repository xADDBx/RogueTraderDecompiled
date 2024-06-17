using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class PingDialogAnswerGameCommand : GameCommand, IMemoryPackable<PingDialogAnswerGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PingDialogAnswerGameCommandFormatter : MemoryPackFormatter<PingDialogAnswerGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PingDialogAnswerGameCommand value)
		{
			PingDialogAnswerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingDialogAnswerGameCommand value)
		{
			PingDialogAnswerGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private string m_Answer;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_IsHover;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingDialogAnswerGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PingDialogAnswerGameCommand(string m_answer, bool m_ishover)
		: this()
	{
		m_Answer = m_answer;
		m_IsHover = m_ishover;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingDialogAnswerLocally(playerOrEmpty, m_Answer, m_IsHover);
	}

	static PingDialogAnswerGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingDialogAnswerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingDialogAnswerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingDialogAnswerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingDialogAnswerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PingDialogAnswerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.m_Answer);
		writer.WriteUnmanaged(in value.m_IsHover);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingDialogAnswerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		string answer;
		if (memberCount == 2)
		{
			if (value == null)
			{
				answer = reader.ReadString();
				reader.ReadUnmanaged<bool>(out value2);
			}
			else
			{
				answer = value.m_Answer;
				value2 = value.m_IsHover;
				answer = reader.ReadString();
				reader.ReadUnmanaged<bool>(out value2);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingDialogAnswerGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				answer = null;
				value2 = false;
			}
			else
			{
				answer = value.m_Answer;
				value2 = value.m_IsHover;
			}
			if (memberCount != 0)
			{
				answer = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value2);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new PingDialogAnswerGameCommand(answer, value2);
	}
}
