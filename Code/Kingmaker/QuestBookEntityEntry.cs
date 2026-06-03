using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking.Serialization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker;

public class QuestBookEntityEntry : EntityFact<QuestBook>, ITimedEvent, ISubscriber, IHashable
{
	[CanBeNull]
	protected Quest m_Quest;

	[JsonProperty]
	protected QuestObjectiveState m_State;

	[JsonProperty]
	protected bool m_IsVisible;

	[JsonProperty]
	private bool m_IsCollapse;

	[JsonProperty]
	protected TimeSpan m_ObjectiveStartTime;

	[CanBeNull]
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	internal IList<QuestBookEntityEntry> m_Addendums;

	[CanBeNull]
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	internal IList<QuestBookEntityEntry> m_NextObjectives;

	[JsonProperty]
	[GameStateIgnore]
	private bool m_NeedToAttention;

	[JsonProperty]
	private bool m_IsViewed;

	[CanBeNull]
	public QuestBookEntityEntry ParentObjective { get; set; }

	public Quest Quest => m_Quest ?? (m_Quest = base.Manager?.GetNotFromCache<Quest>(Blueprint.Quest));

	[JsonProperty]
	public int Order { get; set; }

	public QuestObjectiveState State => m_State;

	public bool IsClue => Blueprint.IsClue;

	public bool IsVisible
	{
		get
		{
			if (!Blueprint.IsHidden && m_State != 0)
			{
				return m_IsVisible;
			}
			return false;
		}
	}

	public new BlueprintQuestObjective Blueprint => (BlueprintQuestObjective)base.Blueprint;

	public TimeSpan? TimeToFail
	{
		get
		{
			if (Blueprint.AutoFailDays <= 0)
			{
				return null;
			}
			return m_ObjectiveStartTime.Add(TimeSpan.FromDays(Blueprint.AutoFailDays)) - Game.Instance.Player.GameTime;
		}
	}

	public bool NeedToAttention
	{
		get
		{
			if (!IsVisible)
			{
				m_NeedToAttention = false;
				return false;
			}
			if (m_NeedToAttention)
			{
				return true;
			}
			foreach (BlueprintQuestObjective addendum in Blueprint.Addendums)
			{
				QuestBookEntityEntry questBookEntityEntry = Quest.TryGetObjective(addendum);
				if (questBookEntityEntry != null && questBookEntityEntry.NeedToAttention)
				{
					return true;
				}
			}
			return false;
		}
		set
		{
			m_NeedToAttention = value;
		}
	}

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
				EventBus.RaiseEvent(delegate(ISetQuestObjectiveViewedHandler l)
				{
					l.HandleSetQuestObjectiveViewed(this);
				});
			}
		}
	}

	public bool IsCollapse
	{
		get
		{
			return m_IsCollapse;
		}
		set
		{
			m_IsCollapse = value;
		}
	}

	public QuestBookEntityEntry(Quest quest, BlueprintQuestObjective blueprintQuestObjective)
		: base((BlueprintFact)blueprintQuestObjective)
	{
		m_Quest = quest;
		base.SuppressActivationOnAttach = true;
		m_IsVisible = !blueprintQuestObjective.IsAddendum;
	}

	public QuestBookEntityEntry(JsonConstructorMark _)
	{
	}

	protected override void OnPostLoad()
	{
		m_Addendums = null;
		m_NextObjectives = null;
		foreach (BlueprintQuestObjective addendum in Blueprint.Addendums)
		{
			QuestBookEntityEntry questBookEntityEntry = Quest.TryGetObjective(addendum);
			if (questBookEntityEntry == null)
			{
				UberDebug.LogError("Error: Quest Objective " + Blueprint.name + " has a null addendum");
			}
			else
			{
				questBookEntityEntry.ParentObjective = this;
			}
		}
		base.OnPostLoad();
	}

	public virtual void Start(bool silentStart = false)
	{
		ObjectiveActivateHandler(QuestObjectiveState.Started, delegate
		{
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnStarted();
			});
		}, "Cannot start quest objective: already started");
		TryBecameVisible(silentStart);
		StartNotificationHandler(silentStart);
		if (!IsClue)
		{
			AddendumStartProcessHandler(silentStart);
		}
		m_ObjectiveStartTime = Game.Instance.Player.GameTime;
	}

	public void Complete()
	{
		ObjectiveDeactivateHandler(QuestObjectiveState.Completed, delegate
		{
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnCompleted();
			});
		});
		Blueprint.CallComponents(delegate(IQuestObjectiveCallback c)
		{
			c.OnComplete();
		});
	}

	public void Fail()
	{
		ObjectiveDeactivateHandler(QuestObjectiveState.Failed, delegate
		{
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnFailed();
			});
		});
		Blueprint.CallComponents(delegate(IQuestObjectiveCallback c)
		{
			c.OnFail();
		});
	}

	public void Reset()
	{
		m_IsVisible = !Blueprint.IsAddendum;
		m_State = QuestObjectiveState.None;
		NeedToAttention = false;
		if (base.Active)
		{
			Deactivate();
		}
	}

	public void TryFailOnQuestFinished()
	{
		if (!Blueprint.IsAddendum && !Blueprint.IsClue && m_State == QuestObjectiveState.Started)
		{
			SetState(QuestObjectiveState.Failed);
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnFailed();
			});
			Deactivate();
			EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveFailed(this);
			});
			Blueprint.CallComponents(delegate(IQuestObjectiveCallback c)
			{
				c.OnFail();
			});
		}
	}

	private void TryBecameVisible(bool silentStart = false)
	{
		if (!m_IsVisible && !Blueprint.IsHidden && ReadyToBecameVisible())
		{
			m_IsVisible = true;
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnBecameVisible();
			});
			EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveBecameVisible(this, silentStart);
			});
		}
	}

	private bool ReadyToBecameVisible()
	{
		if (State == QuestObjectiveState.None)
		{
			return false;
		}
		if (ParentObjective != null && ParentObjective.State == QuestObjectiveState.None)
		{
			return false;
		}
		return true;
	}

	protected void AddendumStartProcessHandler(bool silentStart)
	{
		foreach (BlueprintQuestObjective addendum in Blueprint.Addendums)
		{
			QuestBookEntityEntry questBookEntityEntry = Quest.TryGetObjective(addendum);
			if (questBookEntityEntry != null)
			{
				questBookEntityEntry.ParentObjective = this;
				if (questBookEntityEntry.State == QuestObjectiveState.None && questBookEntityEntry.Blueprint.IsAutomaticallyStartingAddendum)
				{
					questBookEntityEntry.Start();
				}
				questBookEntityEntry.TryBecameVisible(silentStart);
			}
		}
	}

	private void ObjectiveActivateHandler(QuestObjectiveState stateToSet, Action onActivate, string msg = "")
	{
		if (Quest.State != QuestState.Completed && Quest.State != QuestState.Failed)
		{
			SetState(stateToSet);
			onActivate?.Invoke();
			Activate();
			Quest.OnObjectiveStateChange(this);
		}
	}

	protected void ObjectiveDeactivateHandler(QuestObjectiveState stateToSet, Action onDeactivate)
	{
		if (Quest.State == QuestState.Completed || Quest.State == QuestState.Failed)
		{
			return;
		}
		SetState(stateToSet);
		onDeactivate?.Invoke();
		Deactivate();
		switch (stateToSet)
		{
		case QuestObjectiveState.Completed:
			EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveCompleted(this);
			});
			break;
		case QuestObjectiveState.Failed:
			EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveFailed(this);
			});
			break;
		}
		Quest.OnObjectiveStateChange(this);
	}

	public void HandleTimePassed()
	{
	}

	public bool CanStart()
	{
		return Blueprint.ComponentsArray.OfType<IQuestObjectiveStartCondition>().All((IQuestObjectiveStartCondition condition) => condition.CanStart());
	}

	private void SetState(QuestObjectiveState state)
	{
		m_State = state;
		NeedToAttention = true;
	}

	private void StartNotificationHandler(bool silentStart)
	{
		if (IsClue)
		{
			ClueServiceFact clueServiceFact = base.Owner.Facts.Get((ClueServiceFact csf) => csf.Blueprint == Blueprint);
			if (clueServiceFact == null)
			{
				QuestBookEntityEntry questBookEntityEntry = Quest.Objectives.FirstOrDefault((QuestBookEntityEntry o) => o.Blueprint.Clues.Contains(Blueprint));
				if (questBookEntityEntry != null && base.Owner.Facts.Contains(questBookEntityEntry) && questBookEntityEntry.IsActive)
				{
					EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
					{
						l.HandleQuestObjectiveStarted(this, silentStart);
					});
				}
			}
			else
			{
				base.Owner.Facts.Remove(clueServiceFact);
			}
		}
		else
		{
			EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveStarted(this, silentStart);
			});
		}
	}

	public override string ToString()
	{
		string arg = "Objective";
		if (Blueprint.IsAddendum)
		{
			arg = "Addendum";
		}
		else if (Blueprint.IsClue)
		{
			arg = "Clue";
		}
		return $"{arg}#{State}#" + base.ToString();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_State);
		result.Append(ref m_IsVisible);
		result.Append(ref m_IsCollapse);
		result.Append(ref m_ObjectiveStartTime);
		IList<QuestBookEntityEntry> addendums = m_Addendums;
		if (addendums != null)
		{
			for (int i = 0; i < addendums.Count; i++)
			{
				Hash128 val2 = ClassHasher<QuestBookEntityEntry>.GetHash128(addendums[i]);
				result.Append(ref val2);
			}
		}
		IList<QuestBookEntityEntry> nextObjectives = m_NextObjectives;
		if (nextObjectives != null)
		{
			for (int j = 0; j < nextObjectives.Count; j++)
			{
				Hash128 val3 = ClassHasher<QuestBookEntityEntry>.GetHash128(nextObjectives[j]);
				result.Append(ref val3);
			}
		}
		int val4 = Order;
		result.Append(ref val4);
		result.Append(ref m_IsViewed);
		return result;
	}
}
