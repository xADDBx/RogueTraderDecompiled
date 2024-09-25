using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints;

[TypeId("bcfc8eac636c52e468a36d023c5ccd8e")]
public class BlueprintMultiEntranceEntry : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintMultiEntranceEntry>
	{
	}

	public LocalizedString Name;

	[SerializeField]
	private ConditionsChecker m_Condition;

	[SerializeField]
	[Tooltip("If field is not empty, then entry is visible but can be non-interactable")]
	private ConditionsChecker m_InteractableCondition;

	[SerializeField]
	private ActionList m_Actions;

	public bool IsVisible
	{
		get
		{
			if (m_Condition != null && m_Condition.HasConditions)
			{
				return m_Condition.Check();
			}
			return true;
		}
	}

	public bool IsInteractable
	{
		get
		{
			if (IsVisible)
			{
				if (m_InteractableCondition != null && m_InteractableCondition.HasConditions)
				{
					return m_InteractableCondition.Check();
				}
				return true;
			}
			return false;
		}
	}

	public void Enter()
	{
		m_Actions.Run();
	}

	public List<BlueprintQuestObjective> GetLinkedObjectives()
	{
		List<BlueprintQuestObjective> list = new List<BlueprintQuestObjective>();
		foreach (Quest quest in Game.Instance.Player.QuestBook.Quests)
		{
			QuestState state = quest.State;
			if (state == QuestState.Completed || state == QuestState.Failed || state == QuestState.None)
			{
				continue;
			}
			foreach (QuestObjective objective in quest.Objectives)
			{
				QuestObjectiveState state2 = objective.State;
				if (state2 == QuestObjectiveState.Completed || state2 == QuestObjectiveState.Failed || state2 == QuestObjectiveState.None || objective.Blueprint.MultiEntranceEntries == null || !objective.IsActive || !objective.IsVisible)
				{
					continue;
				}
				foreach (Reference multiEntranceEntry in objective.Blueprint.MultiEntranceEntries)
				{
					if (multiEntranceEntry.Is(this))
					{
						list.Add(objective.Blueprint);
					}
				}
			}
		}
		return list;
	}

	public bool CheckCurrentlyEntryLocation()
	{
		BlueprintAreaEnterPoint areaEnterPoint = base.ElementsArray.OfType<TeleportParty>().FirstOrDefault((TeleportParty t) => t?.exitPositon != null)?.exitPositon;
		if (areaEnterPoint != null)
		{
			if (Game.Instance.CurrentlyLoadedArea == areaEnterPoint.Area)
			{
				if (!Game.Instance.CurrentlyLoadedArea.Parts.Any((BlueprintAreaPartReference p) => p.Get() == areaEnterPoint.AreaPart))
				{
					return Game.Instance.CurrentlyLoadedArea == areaEnterPoint.AreaPart;
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
