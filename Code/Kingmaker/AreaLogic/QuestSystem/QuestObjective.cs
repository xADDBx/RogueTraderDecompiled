using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

namespace Kingmaker.AreaLogic.QuestSystem;

[JsonObject(MemberSerialization.OptIn)]
public class QuestObjective : EntityFact<QuestBook>, ITimedEvent, ISubscriber, IHashable
{
	[CanBeNull]
	private Quest m_Quest;

	[JsonProperty]
	private QuestObjectiveState m_State;

	[JsonProperty]
	private bool m_IsVisible;

	[JsonProperty]
	private bool m_IsCollapse;

	[JsonProperty]
	private TimeSpan m_ObjectiveStartTime;

	[CanBeNull]
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	internal IList<QuestObjective> m_Addendums;

	[CanBeNull]
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	internal IList<QuestObjective> m_NextObjectives;

	[JsonProperty]
	[GameStateIgnore]
	private bool m_NeedToAttention;

	[JsonProperty]
	private bool m_IsViewed;

	[CanBeNull]
	public QuestObjective ParentObjective { get; private set; }

	public Quest Quest => m_Quest ?? (m_Quest = base.Manager?.GetNotFromCache<Quest>(Blueprint.Quest));

	[JsonProperty]
	public int Order { get; set; }

	public QuestObjectiveState State => m_State;

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
				QuestObjective questObjective = Quest.TryGetObjective(addendum);
				if (questObjective != null && questObjective.NeedToAttention)
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

	public QuestObjective(Quest quest, BlueprintQuestObjective blueprintQuestObjective)
		: base((BlueprintFact)blueprintQuestObjective)
	{
		base.SuppressActivationOnAttach = true;
		m_IsVisible = !blueprintQuestObjective.IsAddendum;
	}

	protected QuestObjective(JsonConstructorMark _)
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

	public void Start()
	{
		if (Quest.State == QuestState.Completed || Quest.State == QuestState.Failed)
		{
			return;
		}
		SetState(QuestObjectiveState.Started);
		Activate();
		CallComponents(delegate(IQuestObjectiveLogic logic)
		{
			logic.OnStarted();
		});
		Quest.OnObjectiveStateChange(this);
		EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
		{
			l.HandleQuestObjectiveStarted(this);
		});
		TryBecameVisible();
		foreach (BlueprintQuestObjective addendum in Blueprint.Addendums)
		{
			QuestObjective questObjective = Quest.TryGetObjective(addendum);
			if (questObjective != null)
			{
				questObjective.ParentObjective = this;
				if (questObjective.State == QuestObjectiveState.None && questObjective.Blueprint.IsAutomaticallyStartingAddendum)
				{
					questObjective.Start();
				}
				questObjective.TryBecameVisible();
			}
		}
		m_ObjectiveStartTime = Game.Instance.Player.GameTime;
	}

	public void Complete()
	{
		if (Quest.State != QuestState.Completed && Quest.State != QuestState.Failed)
		{
			SetState(QuestObjectiveState.Completed);
			CallComponents(delegate(IQuestObjectiveLogic logic)
			{
				logic.OnCompleted();
			});
			Deactivate();
			EventBus.RaiseEvent(delegate(IQuestObjectiveHandler l)
			{
				l.HandleQuestObjectiveCompleted(this);
			});
			Quest.OnObjectiveStateChange(this);
			Blueprint.CallComponents(delegate(IQuestObjectiveCallback c)
			{
				c.OnComplete();
			});
		}
	}

	public void Fail()
	{
		if (Quest.State != QuestState.Completed && Quest.State != QuestState.Failed)
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
			Quest.OnObjectiveStateChange(this);
			Blueprint.CallComponents(delegate(IQuestObjectiveCallback c)
			{
				c.OnFail();
			});
		}
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

	public void HandleTimePassed()
	{
	}

	public void TryFailOnQuestFinished()
	{
		if (!Blueprint.IsAddendum && m_State == QuestObjectiveState.Started)
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

	protected override void OnPostLoad()
	{
		m_Addendums = null;
		m_NextObjectives = null;
		foreach (BlueprintQuestObjective addendum in Blueprint.Addendums)
		{
			QuestObjective questObjective = Quest.TryGetObjective(addendum);
			if (questObjective == null)
			{
				UberDebug.LogError("Error: Quest Objective " + Blueprint.name + " has a null addendum");
			}
			else
			{
				questObjective.ParentObjective = this;
			}
		}
		base.OnPostLoad();
	}

	private void TryBecameVisible()
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
				l.HandleQuestObjectiveBecameVisible(this);
			});
		}
	}

	protected virtual bool ReadyToBecameVisible()
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

	public override string ToString()
	{
		string arg = (Blueprint.IsAddendum ? "Addendum" : "Objective");
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
		IList<QuestObjective> addendums = m_Addendums;
		if (addendums != null)
		{
			for (int i = 0; i < addendums.Count; i++)
			{
				Hash128 val2 = ClassHasher<QuestObjective>.GetHash128(addendums[i]);
				result.Append(ref val2);
			}
		}
		IList<QuestObjective> nextObjectives = m_NextObjectives;
		if (nextObjectives != null)
		{
			for (int j = 0; j < nextObjectives.Count; j++)
			{
				Hash128 val3 = ClassHasher<QuestObjective>.GetHash128(nextObjectives[j]);
				result.Append(ref val3);
			}
		}
		int val4 = Order;
		result.Append(ref val4);
		result.Append(ref m_IsViewed);
		return result;
	}
}
