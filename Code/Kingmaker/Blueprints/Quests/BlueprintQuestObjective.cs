using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Localization;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests;

[TypeId("9d27cf036b8dcef408e34b42d5234400")]
public class BlueprintQuestObjective : BlueprintFact, IQuestReference, IQuestObjectiveReference, IEditorCommentHolder
{
	public class SilentQuestNotificationOverride : IDisposable
	{
		[CanBeNull]
		private readonly BlueprintQuestObjective m_Objective;

		private readonly QuestNotificationState m_OriginalState;

		public SilentQuestNotificationOverride([CanBeNull] BlueprintQuestObjective objective, QuestNotificationState stateOverride)
		{
			m_Objective = objective;
			if (m_Objective != null)
			{
				m_OriginalState = m_Objective.m_SilentQuestNotification;
				m_Objective.m_SilentQuestNotification = stateOverride;
			}
		}

		public void Dispose()
		{
			if (m_Objective != null)
			{
				m_Objective.m_SilentQuestNotification = m_OriginalState;
			}
		}
	}

	public enum Type
	{
		Objective,
		Addendum,
		AddendumStartingAutomatically,
		Clue
	}

	[SerializeField]
	[HideInInspector]
	private List<BlueprintQuestObjectiveReference> m_Addendums = new List<BlueprintQuestObjectiveReference>();

	[SerializeField]
	[HideInInspector]
	private List<BlueprintQuestObjectiveReference> m_Clues = new List<BlueprintQuestObjectiveReference>();

	[SerializeField]
	private List<BlueprintAreaReference> m_Areas = new List<BlueprintAreaReference>();

	[NotNull]
	public LocalizedString Title;

	public List<BlueprintMultiEntranceEntry.Reference> MultiEntranceEntries;

	[NotNull]
	public LocalizedString Description;

	[NotNull]
	public LocalizedString Destination;

	[NotNull]
	public LocalizedString LoadingScreenHint;

	public int AutoFailDays;

	public bool IsFakeFail;

	public bool StartOnKingdomTime;

	[SerializeField]
	[HideInInspector]
	private bool m_FinishParent;

	[SerializeField]
	private bool m_Hidden;

	[SerializeField]
	[HideInInspector]
	private List<BlueprintQuestObjectiveReference> m_NextObjectives = new List<BlueprintQuestObjectiveReference>();

	[SerializeField]
	[InspectorReadOnly]
	private BlueprintQuestReference m_Quest;

	[SerializeField]
	[InspectorReadOnly]
	private Type m_Type;

	[SerializeField]
	private QuestNotificationState m_SilentQuestNotification;

	[HideInInspector]
	[SerializeField]
	private EditorCommentHolder m_EditorComment;

	private ReferenceListProxy<BlueprintQuestObjective, BlueprintQuestObjectiveReference> m_NextObjectivesProxy;

	private ReferenceListProxy<BlueprintQuestObjective, BlueprintQuestObjectiveReference> m_AddendumsProxy;

	private ReferenceListProxy<BlueprintQuestObjective, BlueprintQuestObjectiveReference> m_CluesProxy;

	[Header("Etude Range Counter")]
	public bool UiCounterEtude;

	[ShowIf("UiCounterEtude")]
	public BlueprintEtudeReference BlueprintCounterEtude;

	[ShowIf("UiCounterEtude")]
	public LocalizedString BlueprintCounterEtudeDescription;

	private LocalizedString m_ConfirmationDescription;

	private LocalizedString m_RejectionDescription;

	private ReferenceListProxy<BlueprintArea, BlueprintAreaReference> m_AreasProxy;

	public EditorCommentHolder EditorComment
	{
		get
		{
			return m_EditorComment;
		}
		set
		{
			m_EditorComment = value;
		}
	}

	public BlueprintQuest Quest => m_Quest?.Get();

	public IList<BlueprintQuestObjective> NextObjectives => m_NextObjectivesProxy ?? (m_NextObjectivesProxy = m_NextObjectives);

	public IList<BlueprintQuestObjective> Addendums => m_AddendumsProxy ?? (m_AddendumsProxy = m_Addendums);

	public IList<BlueprintQuestObjective> Clues => m_CluesProxy ?? (m_CluesProxy = m_Clues);

	public string ConfirmationDescription => m_ConfirmationDescription;

	public string RejectionDescription => m_RejectionDescription;

	public bool IsFinishParent => m_FinishParent;

	public bool IsErrandObjective
	{
		get
		{
			if (!IsAddendum && m_Quest != null)
			{
				BlueprintQuest quest = Quest;
				if (quest == null)
				{
					return false;
				}
				return quest.Type == QuestType.Errand;
			}
			return false;
		}
	}

	public bool IsAddendum
	{
		get
		{
			if (m_Type != 0)
			{
				return m_Type != Type.Clue;
			}
			return false;
		}
	}

	public bool IsClue => m_Type == Type.Clue;

	public bool IsAutomaticallyStartingAddendum => m_Type == Type.AddendumStartingAutomatically;

	public bool IsHidden
	{
		get
		{
			return m_Hidden;
		}
		set
		{
			m_Hidden = value;
		}
	}

	public IList<BlueprintArea> Areas => m_AreasProxy ?? (m_AreasProxy = m_Areas);

	public LocalizedString GetTitile()
	{
		if (!IsErrandObjective)
		{
			QuestGroupId? questGroupId = Quest?.Group;
			if (!questGroupId.HasValue || questGroupId.GetValueOrDefault() != QuestGroupId.Rumours)
			{
				return Title;
			}
		}
		return m_Quest?.Get().Title;
	}

	public LocalizedString GetDescription()
	{
		if (!IsErrandObjective)
		{
			QuestGroupId? questGroupId = Quest?.Group;
			if (!questGroupId.HasValue || questGroupId.GetValueOrDefault() != QuestGroupId.Rumours)
			{
				return Description;
			}
		}
		return m_Quest.Get().Description;
	}

	public LocalizedString GetLoadingScreenHint()
	{
		if (!IsErrandObjective)
		{
			QuestGroupId? questGroupId = Quest?.Group;
			if ((!questGroupId.HasValue || questGroupId.GetValueOrDefault() != QuestGroupId.Rumours) && !string.IsNullOrWhiteSpace(LoadingScreenHint))
			{
				return LoadingScreenHint;
			}
		}
		return m_Quest.Get().LoadingScreenHint;
	}

	public bool IsSilentQuestNotification(QuestNotificationState state)
	{
		return (m_SilentQuestNotification & state) != 0;
	}

	public override string ToString()
	{
		return $"{base.ToString()} [{Quest}]";
	}

	public QuestObjectiveReferenceType GetUsagesFor(BlueprintQuestObjective questObj)
	{
		if (m_NextObjectives.HasItem((BlueprintQuestObjectiveReference r) => r.Is(questObj)))
		{
			return QuestObjectiveReferenceType.Give;
		}
		return QuestObjectiveReferenceType.None;
	}

	public QuestReferenceType GetUsagesFor(BlueprintQuest quest)
	{
		if (quest == Quest && IsFinishParent && !IsAddendum)
		{
			return QuestReferenceType.Complete;
		}
		return QuestReferenceType.None;
	}

	protected override System.Type GetFactType()
	{
		return typeof(QuestBookEntityEntry);
	}

	public void RemoveNullReferences()
	{
		int num = m_NextObjectives.RemoveAll((BlueprintQuestObjectiveReference i) => i == null);
		if (num > 0)
		{
			PFLog.Default.Warning(this, "Quest objective '{0}': removed {1} missing next objectives", this, num);
		}
		int num2 = m_Addendums.RemoveAll((BlueprintQuestObjectiveReference i) => i == null);
		if (num2 > 0)
		{
			PFLog.Default.Warning(this, "Quest objective '{0}': removed {1} missing addendums", this, num2);
		}
	}

	public void SetQuest(BlueprintQuest quest)
	{
		if (Quest != quest)
		{
			m_Quest = quest.ToReference<BlueprintQuestReference>();
			SetDirty();
		}
	}

	public void SetIsAddendum(bool isAddendum)
	{
		if (isAddendum && m_Type == Type.Objective)
		{
			m_Type = Type.Addendum;
		}
		else if (!isAddendum)
		{
			m_Type = Type.Objective;
		}
		SetDirty();
	}

	public void SetIsClue(bool isCLue)
	{
		if (isCLue && m_Type != Type.Clue)
		{
			m_Type = Type.Clue;
		}
		else if (!isCLue)
		{
			m_Type = Type.Objective;
		}
		SetDirty();
	}

	public void SetStartAddendumAutomatically(bool value)
	{
		if (m_Type == Type.Objective)
		{
			PFLog.Default.Warning("Can't process this objective as addendum");
			return;
		}
		Type type = m_Type;
		m_Type = ((!value) ? Type.Addendum : Type.AddendumStartingAutomatically);
		if (type != m_Type)
		{
			SetDirty();
		}
	}

	public void SetIsFinishParent(bool finishParent)
	{
		if (finishParent != m_FinishParent)
		{
			m_FinishParent = finishParent;
			SetDirty();
		}
	}

	public void AddNextObjective(BlueprintQuestObjective objective)
	{
		if (objective == this)
		{
			PFLog.Default.Error("Objective can't be next for itself!");
			return;
		}
		if (m_NextObjectives.HasItem((BlueprintQuestObjectiveReference r) => r.Is(objective)))
		{
			PFLog.Default.Warning("Objective already is next");
			return;
		}
		m_NextObjectives.Add(objective.ToReference<BlueprintQuestObjectiveReference>());
		if (Quest != null)
		{
			Quest.LinkObjective(objective);
		}
		SetDirty();
	}

	public void RemoveNextObjective(BlueprintQuestObjective objective)
	{
		if (!m_NextObjectives.HasItem((BlueprintQuestObjectiveReference r) => r.Is(objective)))
		{
			PFLog.Default.Warning("Objective not next");
			return;
		}
		m_NextObjectives.RemoveAll((BlueprintQuestObjectiveReference r) => r.Is(objective));
		SetDirty();
	}

	public void AddNextObjectiveFromMenu(object userdata)
	{
		AddNextObjective((BlueprintQuestObjective)userdata);
	}

	public void AddAddendum(BlueprintQuestObjective objective)
	{
		if (objective == this)
		{
			PFLog.Default.Error("Objective can't be addendum for itself!");
			return;
		}
		if (Addendums.Contains(objective))
		{
			PFLog.Default.Warning("Addendum already added");
			return;
		}
		Addendums.Add(objective);
		objective.SetIsAddendum(isAddendum: true);
		if (Quest != null)
		{
			Quest.LinkObjective(objective);
		}
		SetDirty();
	}

	public void RemoveAddendum(BlueprintQuestObjective objective)
	{
		if (!Addendums.Contains(objective))
		{
			PFLog.Default.Warning("Addendum not found");
			return;
		}
		Addendums.Remove(objective);
		SetDirty();
	}

	public void AddAddendumFromMenu(object userdata)
	{
		AddAddendum((BlueprintQuestObjective)userdata);
	}

	public void AddClue(BlueprintQuestObjective objective)
	{
		if (objective == this)
		{
			PFLog.Default.Error("Objective can't be clue for itself!");
			return;
		}
		if (Clues.Contains(objective))
		{
			PFLog.Default.Warning("Clue already added");
			return;
		}
		Clues.Add(objective);
		objective.SetIsClue(isCLue: true);
		if (Quest != null)
		{
			Quest.LinkObjective(objective);
		}
		SetDirty();
	}

	public void RemoveClue(BlueprintQuestObjective objective)
	{
		if (!Clues.Contains(objective))
		{
			PFLog.Default.Warning("Clue not found");
			return;
		}
		Clues.Remove(objective);
		SetDirty();
	}

	public void AddCluesFromMenu(object userdata)
	{
		AddClue((BlueprintQuestObjective)userdata);
	}
}
