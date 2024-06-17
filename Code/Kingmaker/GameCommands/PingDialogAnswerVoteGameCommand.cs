using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class PingDialogAnswerVoteGameCommand : GameCommand, IMemoryPackable<PingDialogAnswerVoteGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PingDialogAnswerVoteGameCommandFormatter : MemoryPackFormatter<PingDialogAnswerVoteGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PingDialogAnswerVoteGameCommand value)
		{
			PingDialogAnswerVoteGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingDialogAnswerVoteGameCommand value)
		{
			PingDialogAnswerVoteGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private string m_Answer;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingDialogAnswerVoteGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PingDialogAnswerVoteGameCommand(string m_answer)
		: this()
	{
		m_Answer = m_answer;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingDialogAnswerVoteLocally(playerOrEmpty, m_Answer);
	}

	static PingDialogAnswerVoteGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingDialogAnswerVoteGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingDialogAnswerVoteGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingDialogAnswerVoteGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingDialogAnswerVoteGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PingDialogAnswerVoteGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.m_Answer);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingDialogAnswerVoteGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string answer;
		if (memberCount == 1)
		{
			if (value == null)
			{
				answer = reader.ReadString();
			}
			else
			{
				answer = value.m_Answer;
				answer = reader.ReadString();
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingDialogAnswerVoteGameCommand), 1, memberCount);
				return;
			}
			answer = ((value != null) ? value.m_Answer : null);
			if (memberCount != 0)
			{
				answer = reader.ReadString();
				_ = 1;
			}
			_ = value;
		}
		value = new PingDialogAnswerVoteGameCommand(answer);
	}
}
