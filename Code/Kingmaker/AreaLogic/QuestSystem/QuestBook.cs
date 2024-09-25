using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

[JsonObject(MemberSerialization.OptIn)]
public class QuestBook : Entity, IHashable
{
	public IEnumerable<Quest> Quests => from q in Facts.GetAllNotFromCache<Quest>()
		where q.State != QuestState.None
		select q;

	public void GiveObjective(BlueprintQuestObjective bpObjective, bool silentStart = false)
	{
		QuestObjective questObjective = EnsureObjective(bpObjective);
		if (questObjective.State != QuestObjectiveState.Started)
		{
			if (questObjective.State != 0)
			{
				PFLog.Default.Warning("Quest objective has invalid state");
			}
			else
			{
				questObjective.Start(silentStart);
			}
		}
	}

	public void CompleteObjective(BlueprintQuestObjective bpObjective)
	{
		QuestObjective questObjective = EnsureObjective(bpObjective);
		if (questObjective.State == QuestObjectiveState.None)
		{
			questObjective.Start();
		}
		if (questObjective.State != QuestObjectiveState.Started)
		{
			PFLog.Default.Warning("Quest objective has invalid state");
		}
		else
		{
			questObjective.Complete();
		}
	}

	public void FailObjective(BlueprintQuestObjective bpObjective)
	{
		QuestObjective questObjective = EnsureObjective(bpObjective);
		if (questObjective.State == QuestObjectiveState.None)
		{
			questObjective.Start();
		}
		if (questObjective.State != QuestObjectiveState.Started)
		{
			PFLog.Default.Warning("Quest objective has invalid state");
		}
		else
		{
			questObjective.Fail();
		}
	}

	public void ResetObjective(BlueprintQuestObjective bpObjective)
	{
		QuestObjective questObjective = EnsureObjective(bpObjective);
		if (questObjective.State != 0)
		{
			questObjective.Reset();
		}
	}

	[CanBeNull]
	private Quest GetQuestInternal(BlueprintQuest quest)
	{
		foreach (EntityFact item in Facts.List)
		{
			if (item is Quest quest2 && quest2.Blueprint == quest)
			{
				return quest2;
			}
		}
		return null;
	}

	public Quest GetQuest(BlueprintQuest quest)
	{
		return GetQuestInternal(quest);
	}

	public QuestState GetQuestState(BlueprintQuest bpQuest)
	{
		return GetQuestInternal(bpQuest)?.State ?? QuestState.None;
	}

	public QuestObjectiveState GetObjectiveState(BlueprintQuestObjective bpObjective)
	{
		Quest questInternal = GetQuestInternal(bpObjective.Quest);
		if (questInternal == null)
		{
			return QuestObjectiveState.None;
		}
		return (questInternal.TryGetObjective(bpObjective) ?? throw new Exception("Can't find objective in quest")).State;
	}

	[CanBeNull]
	public QuestObjective GetObjective(BlueprintQuestObjective bpObjective)
	{
		Quest questInternal = GetQuestInternal(bpObjective.Quest);
		if (questInternal == null)
		{
			return null;
		}
		QuestObjective questObjective = questInternal.TryGetObjective(bpObjective);
		if (questObjective == null)
		{
			PFLog.Default.Error("Objective not found");
		}
		return questObjective;
	}

	private QuestObjective EnsureObjective(BlueprintQuestObjective bpObjective)
	{
		Quest quest = GetQuestInternal(bpObjective.Quest);
		if (quest == null)
		{
			quest = BlueprintQuest.CreateNewQuest(bpObjective.Quest);
			Facts.Add(quest);
		}
		return quest.TryGetObjective(bpObjective) ?? throw new Exception("Can't find objective in quest");
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	protected QuestBook(JsonConstructorMark _)
		: base(_)
	{
	}

	public QuestBook()
		: base("quest_book", isInGame: true)
	{
	}

	public void ResetQuest(BlueprintQuest bp, BlueprintQuestObjective start, IEnumerable<BlueprintQuestObjective> reset)
	{
		if ((bool)start)
		{
			GetQuest(bp)?.Uncomplete(start, reset);
		}
		else
		{
			GetQuest(bp)?.Remove();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
