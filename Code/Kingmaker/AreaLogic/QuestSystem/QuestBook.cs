using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Quests;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.DotNetExtensions;
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
		QuestBookEntityEntry questBookEntityEntry = EnsureObjective(bpObjective);
		if (questBookEntityEntry == null)
		{
			return;
		}
		CheckHiddenClues(questBookEntityEntry);
		if (questBookEntityEntry.State != QuestObjectiveState.Started)
		{
			if (questBookEntityEntry.State != 0)
			{
				PFLog.Default.Warning("Quest objective has invalid state");
			}
			else
			{
				questBookEntityEntry.Start(silentStart);
			}
		}
	}

	public void CompleteObjective(BlueprintQuestObjective bpObjective)
	{
		QuestBookEntityEntry questBookEntityEntry = EnsureObjective(bpObjective);
		if (questBookEntityEntry == null)
		{
			return;
		}
		if (questBookEntityEntry.State == QuestObjectiveState.None)
		{
			questBookEntityEntry.Start();
		}
		if (!questBookEntityEntry.IsClue)
		{
			if (questBookEntityEntry.State != QuestObjectiveState.Started)
			{
				PFLog.Default.Warning("Quest objective has invalid state");
			}
			else
			{
				questBookEntityEntry.Complete();
			}
		}
	}

	public void FailObjective(BlueprintQuestObjective bpObjective)
	{
		QuestBookEntityEntry questBookEntityEntry = EnsureObjective(bpObjective);
		if (questBookEntityEntry == null)
		{
			return;
		}
		if (questBookEntityEntry.State == QuestObjectiveState.None)
		{
			questBookEntityEntry.Start();
		}
		if (!questBookEntityEntry.IsClue)
		{
			if (questBookEntityEntry.State != QuestObjectiveState.Started)
			{
				PFLog.Default.Warning("Quest objective has invalid state");
			}
			else
			{
				questBookEntityEntry.Fail();
			}
		}
	}

	public void ResetObjective(BlueprintQuestObjective bpObjective)
	{
		QuestBookEntityEntry questBookEntityEntry = EnsureObjective(bpObjective);
		if (questBookEntityEntry != null && !questBookEntityEntry.IsClue && questBookEntityEntry.State != 0)
		{
			questBookEntityEntry.Reset();
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
	public QuestBookEntityEntry GetObjective(BlueprintQuestObjective bpObjective)
	{
		Quest questInternal = GetQuestInternal(bpObjective.Quest);
		if (questInternal == null)
		{
			return null;
		}
		QuestBookEntityEntry questBookEntityEntry = questInternal.TryGetObjective(bpObjective);
		if (questBookEntityEntry == null)
		{
			PFLog.Default.Error("Objective not found");
		}
		return questBookEntityEntry;
	}

	private QuestBookEntityEntry EnsureObjective(BlueprintQuestObjective bpObjective)
	{
		Quest quest = GetQuestInternal(bpObjective.Quest);
		if (quest == null)
		{
			if (bpObjective.IsClue)
			{
				return AddClueServiceFact(bpObjective);
			}
			quest = BlueprintQuest.CreateNewQuest(bpObjective.Quest);
			Facts.Add(quest);
		}
		QuestBookEntityEntry questBookEntityEntry = quest.TryGetObjective(bpObjective);
		if (questBookEntityEntry == null)
		{
			throw new Exception("Can't find objective in quest");
		}
		QuestBookEntityEntry questBookEntityEntry2 = quest.Objectives.FirstOrDefault((QuestBookEntityEntry o) => o.Blueprint.Clues.Contains(bpObjective));
		if (questBookEntityEntry2 != null && !questBookEntityEntry2.IsActive && bpObjective.IsClue && !Facts.List.Contains((EntityFact f) => f is ClueServiceFact && f.Blueprint == bpObjective))
		{
			return AddClueServiceFact(bpObjective);
		}
		return questBookEntityEntry;
	}

	private QuestBookEntityEntry AddClueServiceFact(BlueprintQuestObjective bpObjective)
	{
		ClueServiceFact fact = new ClueServiceFact(bpObjective);
		Facts.Add(fact);
		return null;
	}

	private void CheckHiddenClues(QuestBookEntityEntry objective)
	{
		if (objective.Blueprint.Clues.Count <= 0)
		{
			return;
		}
		foreach (ClueServiceFact item in from serviceClueFact in objective.Blueprint.Clues.Select((BlueprintQuestObjective blueprintClue) => Facts.Get((ClueServiceFact csf) => csf.Blueprint == blueprintClue)).ToList()
			where serviceClueFact != null && serviceClueFact.Blueprint != null
			select serviceClueFact)
		{
			GiveObjective(item.Blueprint as BlueprintQuestObjective);
		}
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

	public void ResetQuest(BlueprintQuest bp, ResetableSubobjectiveTypes types = ResetableSubobjectiveTypes.None)
	{
		GetQuest(bp)?.Reset(types);
	}

	public void ResetQuest(BlueprintQuest bp, BlueprintQuestObjective start, IEnumerable<BlueprintQuestObjective> resetable)
	{
		GetQuest(bp)?.ResetTo(start, resetable);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
