using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Enums;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.QuestSystem;

[JsonObject(MemberSerialization.OptIn)]
public class Quest : EntityFact<QuestBook>, IHashable
{
	[GameStateInclude]
	private readonly Dictionary<BlueprintQuestObjective, QuestBookEntityEntry> m_Objectives = new Dictionary<BlueprintQuestObjective, QuestBookEntityEntry>();

	[JsonProperty]
	private int m_NextObjectiveOrder = 1;

	[JsonProperty]
	private QuestState m_State;

	[JsonProperty]
	private bool m_IsInUiSelected;

	[JsonProperty]
	private bool m_IsViewed;

	private bool m_IsSilentFailInProgress;

	public bool IsViewed
	{
		get
		{
			return m_IsViewed;
		}
		set
		{
			if (m_IsViewed != value)
			{
				m_IsViewed = value;
				EventBus.RaiseEvent(delegate(ISetQuestViewedHandler l)
				{
					l.HandleSetQuestViewed(this);
				});
			}
		}
	}

	[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
	[UsedImplicitly]
	[GameStateIgnore("This adapter uses LINQ, so original m_Objectives used for serialization")]
	private List<QuestBookEntityEntry> PersistentObjectives
	{
		get
		{
			return m_Objectives.Select((KeyValuePair<BlueprintQuestObjective, QuestBookEntityEntry> objective) => objective.Value).ToList();
		}
		set
		{
			m_Objectives.Clear();
			foreach (QuestBookEntityEntry item in value)
			{
				if (item.Blueprint == null)
				{
					PFLog.Default.Warning("Failed to load objective blueprint. Quest: " + item.Blueprint);
				}
				else
				{
					m_Objectives.Add(item.Blueprint, item);
				}
			}
		}
	}

	public QuestState State
	{
		get
		{
			if (m_State != QuestState.Updated)
			{
				return m_State;
			}
			return QuestState.Started;
		}
	}

	public TimeSpan? TimeToFail
	{
		get
		{
			QuestState state = State;
			if (state == QuestState.Completed || state == QuestState.Failed)
			{
				return null;
			}
			IEnumerable<TimeSpan?> source = (from o in Objectives
				where o.IsVisible && o.State == QuestObjectiveState.Started && o.TimeToFail.HasValue
				select o.TimeToFail).ToList();
			if (source.Any())
			{
				return source.Min();
			}
			return null;
		}
	}

	public bool IsFakeFail
	{
		get
		{
			TimeSpan? timeToFail = TimeToFail;
			if (timeToFail.HasValue)
			{
				return Objectives.FirstOrDefault(delegate(QuestBookEntityEntry o)
				{
					TimeSpan? timeToFail2 = o.TimeToFail;
					TimeSpan? timeSpan = timeToFail;
					if (timeToFail2.HasValue != timeSpan.HasValue)
					{
						return false;
					}
					return !timeToFail2.HasValue || timeToFail2.GetValueOrDefault() == timeSpan.GetValueOrDefault();
				})?.Blueprint.IsFakeFail ?? false;
			}
			return false;
		}
	}

	public bool IsInUiSelected
	{
		get
		{
			return m_IsInUiSelected;
		}
		set
		{
			m_IsInUiSelected = value;
		}
	}

	public bool NeedToAttention => Objectives.Any((QuestBookEntityEntry o) => o.NeedToAttention && (o.ParentObjective?.NeedToAttention ?? true));

	public IEnumerable<QuestBookEntityEntry> Objectives
	{
		get
		{
			foreach (KeyValuePair<BlueprintQuestObjective, QuestBookEntityEntry> objective in m_Objectives)
			{
				yield return objective.Value;
			}
		}
	}

	public new BlueprintQuest Blueprint => (BlueprintQuest)base.Blueprint;

	public Quest(BlueprintQuest blueprintQuest)
		: base((BlueprintFact)blueprintQuest)
	{
		foreach (BlueprintQuestObjective allObjective in blueprintQuest.AllObjectives)
		{
			m_Objectives[allObjective] = new QuestBookEntityEntry(this, allObjective);
		}
	}

	protected Quest(JsonConstructorMark _)
	{
	}

	public bool IsRelatesToCompanion(BlueprintUnit companion)
	{
		return Blueprint.GetComponents<QuestRelatesToCompanionStory>().Any((QuestRelatesToCompanionStory c) => c.Companion == companion);
	}

	protected override void OnAttach()
	{
		foreach (KeyValuePair<BlueprintQuestObjective, QuestBookEntityEntry> objective in m_Objectives)
		{
			base.Owner.Facts.Add(objective.Value);
		}
	}

	protected override void OnDetach()
	{
		foreach (KeyValuePair<BlueprintQuestObjective, QuestBookEntityEntry> objective in m_Objectives)
		{
			base.Owner.Facts.Remove(objective.Value);
		}
	}

	protected override void OnPostLoad()
	{
		foreach (BlueprintQuestObjective allObjective in Blueprint.AllObjectives)
		{
			if (!m_Objectives.ContainsKey(allObjective))
			{
				QuestBookEntityEntry questBookEntityEntry2 = (m_Objectives[allObjective] = new QuestBookEntityEntry(this, allObjective));
				QuestBookEntityEntry fact = questBookEntityEntry2;
				base.Owner.Facts.Add(fact);
			}
		}
	}

	protected override void OnDispose()
	{
		m_Objectives.Clear();
	}

	[CanBeNull]
	public QuestBookEntityEntry TryGetObjective(BlueprintQuestObjective blueprintObjective)
	{
		if (blueprintObjective == null)
		{
			UberDebug.LogError("Error: Quest " + Blueprint.name + " has a null addendum");
			return null;
		}
		m_Objectives.TryGetValue(blueprintObjective, out var value);
		return value;
	}

	private QuestBookEntityEntry GetObjective(BlueprintQuestObjective blueprintObjective)
	{
		return TryGetObjective(blueprintObjective) ?? throw new Exception("Can't find objective in quest");
	}

	public void OnObjectiveStateChange(QuestBookEntityEntry objective)
	{
		if (m_State == QuestState.Started)
		{
			EventBus.RaiseEvent(delegate(IQuestHandler l)
			{
				l.HandleQuestUpdated(this);
			});
		}
		if (m_State == QuestState.None && objective.State != 0 && !objective.Blueprint.IsAddendum)
		{
			m_State = QuestState.Started;
			CallComponents(delegate(IQuestLogic logic)
			{
				logic.OnStarted();
			});
			EventBus.RaiseEvent(delegate(IQuestHandler l)
			{
				l.HandleQuestStarted(this);
			});
		}
		switch (objective.State)
		{
		case QuestObjectiveState.None:
			PFLog.Default.Error("Objective can't change state to None");
			break;
		case QuestObjectiveState.Started:
			objective.Order = m_NextObjectiveOrder++;
			break;
		case QuestObjectiveState.Completed:
			OnObjectiveFinished(objective);
			break;
		case QuestObjectiveState.Failed:
			OnObjectiveFinished(objective);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void OnObjectiveOpened(QuestBookEntityEntry objective)
	{
		switch (objective.State)
		{
		case QuestObjectiveState.None:
			if (objective.CanStart())
			{
				objective.Start();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case QuestObjectiveState.Started:
		case QuestObjectiveState.Completed:
		case QuestObjectiveState.Failed:
			break;
		}
	}

	private void OnObjectiveFinished(QuestBookEntityEntry objective)
	{
		bool flag = objective.State == QuestObjectiveState.Completed;
		bool flag2 = objective.Blueprint.NextObjectives.Empty();
		if (objective.Blueprint.IsFinishParent && (!flag || flag2))
		{
			QuestBookEntityEntry parentObjective = objective.ParentObjective;
			if (objective.Blueprint.IsAddendum && parentObjective != null)
			{
				if (!parentObjective.IsClue)
				{
					if (flag)
					{
						parentObjective.Complete();
					}
					else
					{
						parentObjective.Fail();
					}
				}
			}
			else
			{
				OnQuestFinished(flag);
			}
			return;
		}
		if (flag)
		{
			foreach (BlueprintQuestObjective nextObjective in objective.Blueprint.NextObjectives)
			{
				QuestBookEntityEntry objective2 = GetObjective(nextObjective);
				if (objective2 != null)
				{
					OnObjectiveOpened(objective2);
				}
			}
		}
		if (flag2 && objective.Quest.Blueprint.Group == QuestGroupId.Rumours)
		{
			OnQuestFinished(flag);
		}
	}

	public void FailQuest(bool doFailSilently)
	{
		if (doFailSilently)
		{
			using (new BlueprintQuest.SilentQuestNotificationOverride(Blueprint, QuestNotificationState.Failed))
			{
				m_IsSilentFailInProgress = true;
				OnQuestFinished(completed: false);
				m_IsSilentFailInProgress = false;
				return;
			}
		}
		OnQuestFinished(completed: false);
	}

	protected virtual void OnQuestFinished(bool completed)
	{
		m_State = (completed ? QuestState.Completed : QuestState.Failed);
		if (completed)
		{
			CallComponents(delegate(IQuestLogic logic)
			{
				logic.OnCompleted();
			});
			EventBus.RaiseEvent(delegate(IQuestHandler l)
			{
				l.HandleQuestCompleted(this);
			});
		}
		else
		{
			CallComponents(delegate(IQuestLogic logic)
			{
				logic.OnFailed();
			});
			EventBus.RaiseEvent(delegate(IQuestHandler l)
			{
				l.HandleQuestFailed(this);
			});
		}
		m_Objectives.ForEach(delegate(KeyValuePair<BlueprintQuestObjective, QuestBookEntityEntry> pair)
		{
			QuestBookEntityEntry value = pair.Value;
			if (m_IsSilentFailInProgress)
			{
				using (new BlueprintQuestObjective.SilentQuestNotificationOverride(value.Blueprint, QuestNotificationState.Failed))
				{
					value.TryFailOnQuestFinished();
					return;
				}
			}
			value.TryFailOnQuestFinished();
		});
	}

	public void ResetTo(BlueprintQuestObjective makeStarted, IEnumerable<BlueprintQuestObjective> resetable)
	{
		m_State = QuestState.Started;
		foreach (BlueprintQuestObjective item in resetable)
		{
			if (m_Objectives.TryGetValue(item, out var value))
			{
				value.Reset();
			}
		}
		if (m_Objectives.TryGetValue(makeStarted, out var value2))
		{
			value2.Reset();
			value2.Start();
		}
	}

	public void Reset(ResetableSubobjectiveTypes types = ResetableSubobjectiveTypes.None)
	{
		m_State = QuestState.None;
		IEnumerable<BlueprintQuestObjective> enumerable = Blueprint.Objectives;
		if (types.HasFlag(ResetableSubobjectiveTypes.Addendum))
		{
			enumerable = enumerable.Concat(Blueprint.Addendums);
		}
		if (types.HasFlag(ResetableSubobjectiveTypes.Clue))
		{
			enumerable = enumerable.Concat(Blueprint.Clues);
		}
		foreach (BlueprintQuestObjective item in enumerable)
		{
			if (m_Objectives.TryGetValue(item, out var value))
			{
				value.Reset();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Dictionary<BlueprintQuestObjective, QuestBookEntityEntry> objectives = m_Objectives;
		if (objectives != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintQuestObjective, QuestBookEntityEntry> item in objectives)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				Hash128 val4 = ClassHasher<QuestBookEntityEntry>.GetHash128(item.Value);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		result.Append(ref m_NextObjectiveOrder);
		result.Append(ref m_State);
		result.Append(ref m_IsInUiSelected);
		result.Append(ref m_IsViewed);
		return result;
	}
}
