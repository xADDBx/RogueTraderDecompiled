using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.EntitySystem;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SetQuestViewedGameCommand : GameCommand, IMemoryPackable<SetQuestViewedGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetQuestViewedGameCommandFormatter : MemoryPackFormatter<SetQuestViewedGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetQuestViewedGameCommand value)
		{
			SetQuestViewedGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetQuestViewedGameCommand value)
		{
			SetQuestViewedGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityFactRef<Quest> m_QuestRef;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SetQuestViewedGameCommand()
	{
	}

	[JsonConstructor]
	public SetQuestViewedGameCommand(Quest quest)
	{
		m_QuestRef = quest;
	}

	protected override void ExecuteInternal()
	{
		Quest fact = m_QuestRef.Fact;
		if (fact != null)
		{
			fact.IsViewed = true;
		}
	}

	static SetQuestViewedGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetQuestViewedGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetQuestViewedGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetQuestViewedGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetQuestViewedGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetQuestViewedGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_QuestRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetQuestViewedGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityFactRef<Quest> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_QuestRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityFactRef<Quest>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetQuestViewedGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_QuestRef : default(EntityFactRef<Quest>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0070;
			}
		}
		value = new SetQuestViewedGameCommand
		{
			m_QuestRef = value2
		};
		return;
		IL_0070:
		value.m_QuestRef = value2;
	}
}
