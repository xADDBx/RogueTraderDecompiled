using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.EntitySystem;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SetQuestObjectiveViewedGameCommand : GameCommand, IMemoryPackable<SetQuestObjectiveViewedGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetQuestObjectiveViewedGameCommandFormatter : MemoryPackFormatter<SetQuestObjectiveViewedGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetQuestObjectiveViewedGameCommand value)
		{
			SetQuestObjectiveViewedGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetQuestObjectiveViewedGameCommand value)
		{
			SetQuestObjectiveViewedGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityFactRef<QuestObjective> m_QuestObjectiveRef;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SetQuestObjectiveViewedGameCommand()
	{
	}

	[JsonConstructor]
	public SetQuestObjectiveViewedGameCommand(QuestObjective questObjective)
	{
		m_QuestObjectiveRef = questObjective;
	}

	protected override void ExecuteInternal()
	{
		QuestObjective fact = m_QuestObjectiveRef.Fact;
		if (fact != null)
		{
			fact.IsViewed = true;
		}
	}

	static SetQuestObjectiveViewedGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetQuestObjectiveViewedGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetQuestObjectiveViewedGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetQuestObjectiveViewedGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetQuestObjectiveViewedGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetQuestObjectiveViewedGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_QuestObjectiveRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetQuestObjectiveViewedGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityFactRef<QuestObjective> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_QuestObjectiveRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityFactRef<QuestObjective>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetQuestObjectiveViewedGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_QuestObjectiveRef : default(EntityFactRef<QuestObjective>));
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
		value = new SetQuestObjectiveViewedGameCommand
		{
			m_QuestObjectiveRef = value2
		};
		return;
		IL_0070:
		value.m_QuestObjectiveRef = value2;
	}
}
