using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Settings;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;

public class JournalQuestObjectiveVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly QuestBookEntityEntry Objective;

	public readonly List<JournalQuestObjectiveAddendumVM> Addendums;

	public readonly List<JournalQuestObjectiveAddendumVM> Clues;

	public readonly string Title;

	public readonly string Description;

	public readonly string Destination;

	public bool HasEtudeCounter;

	public int CurrentEtudeCounter;

	public int MinEtudeCounter;

	public string EtudeCounterDescription;

	public readonly bool IsFailed;

	public readonly bool IsCompleted;

	public readonly bool IsPostponed;

	public readonly bool IsHighlight;

	public readonly int ObjectiveNumber;

	public readonly float FontMultiplier = FontSizeMultiplier;

	public bool IsAttention => Objective.NeedToAttention;

	public bool IsCollapse
	{
		get
		{
			return Objective.IsCollapse;
		}
		set
		{
			Objective.IsCollapse = value;
		}
	}

	public bool IsViewed => Objective.IsViewed;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public JournalQuestObjectiveVM(QuestBookEntityEntry objective)
	{
		Objective = objective;
		BlueprintQuestObjective blueprintQuestObjective = objective?.Blueprint;
		Title = blueprintQuestObjective?.GetTitile();
		Description = blueprintQuestObjective?.GetDescription();
		string text = string.Empty;
		if (objective != null)
		{
			BlueprintQuestObjective blueprint = objective.Blueprint;
			if (blueprint != null && blueprint.Areas != null && objective.Blueprint.Areas.Any())
			{
				text = string.Join(", ", (from mapPoint in objective.Blueprint?.Areas?.Where((BlueprintArea mapPoint) => mapPoint != null)
					select mapPoint.Name) ?? Array.Empty<string>());
			}
		}
		Destination = ((!string.IsNullOrWhiteSpace(blueprintQuestObjective?.Destination)) ? ((string)blueprintQuestObjective?.Destination) : ((!string.IsNullOrWhiteSpace(text)) ? text : string.Empty));
		QuestBookEntityEntry questBookEntityEntry = objective;
		ObjectiveNumber = ((questBookEntityEntry == null) ? null : (questBookEntityEntry.Quest?.Objectives?.Where((QuestBookEntityEntry o) => o.IsVisible && o.State != 0 && !o.Blueprint.IsAddendum && !o.Blueprint.IsErrandObjective).ToList().IndexOf(objective) + 1)).GetValueOrDefault();
		RumourMapMarker rumourMapMarker = blueprintQuestObjective?.GetComponent<RumourMapMarker>();
		if (rumourMapMarker != null)
		{
			Destination = string.Join(", ", from mapPoint in rumourMapMarker.SectorMapPointsToVisit.Dereference()
				select mapPoint.Name);
		}
		IOrderedEnumerable<QuestBookEntityEntry> orderedEnumerable = (from b in objective?.Blueprint?.Addendums?.Where((BlueprintQuestObjective b) => b != null)
			select objective?.Quest?.TryGetObjective(b) into a
			where a != null
			where a.IsVisible
			orderby a?.Order descending
			select a);
		IOrderedEnumerable<QuestBookEntityEntry> orderedEnumerable2 = (from b in objective?.Blueprint?.Clues?.Where((BlueprintQuestObjective b) => b != null)
			select objective?.Quest?.TryGetObjective(b) into a
			where a != null
			where a.IsVisible
			where a.IsActive
			orderby a?.Order descending
			select a);
		Addendums = new List<JournalQuestObjectiveAddendumVM>();
		if (orderedEnumerable != null)
		{
			foreach (QuestBookEntityEntry item in orderedEnumerable)
			{
				Addendums.Add(new JournalQuestObjectiveAddendumVM(item));
			}
		}
		Clues = new List<JournalQuestObjectiveAddendumVM>();
		if (orderedEnumerable2 != null)
		{
			foreach (QuestBookEntityEntry item2 in orderedEnumerable2)
			{
				if (item2 != null)
				{
					Clues.Add(new JournalQuestObjectiveAddendumVM(item2));
				}
			}
		}
		QuestBookEntityEntry questBookEntityEntry2 = objective;
		IsFailed = questBookEntityEntry2 != null && questBookEntityEntry2.State == QuestObjectiveState.Failed;
		QuestBookEntityEntry questBookEntityEntry3 = objective;
		IsCompleted = questBookEntityEntry3 != null && questBookEntityEntry3.State == QuestObjectiveState.Completed;
		QuestBookEntityEntry questBookEntityEntry4 = objective;
		IsPostponed = questBookEntityEntry4 != null && questBookEntityEntry4.State == QuestObjectiveState.Postponed;
		QuestBookEntityEntry questBookEntityEntry5 = objective;
		int isHighlight;
		if (questBookEntityEntry5 == null || questBookEntityEntry5.State != QuestObjectiveState.Failed)
		{
			QuestBookEntityEntry questBookEntityEntry6 = objective;
			if (questBookEntityEntry6 == null || questBookEntityEntry6.State != QuestObjectiveState.Completed)
			{
				isHighlight = ((!IsViewed) ? 1 : 0);
				goto IL_055b;
			}
		}
		isHighlight = 1;
		goto IL_055b;
		IL_055b:
		IsHighlight = (byte)isHighlight != 0;
		if (blueprintQuestObjective != null)
		{
			SetCounterEtude(blueprintQuestObjective);
		}
	}

	private void SetCounterEtude(BlueprintQuestObjective blueprint)
	{
		if (blueprint == null || !blueprint.UiCounterEtude || blueprint.BlueprintCounterEtude == null)
		{
			return;
		}
		Condition condition = blueprint.BlueprintCounterEtude?.Get()?.ActivationCondition?.Conditions?.FirstOrDefault((Condition c) => c is FlagInRange);
		HasEtudeCounter = condition != null;
		if (HasEtudeCounter && condition is FlagInRange flagInRange)
		{
			CurrentEtudeCounter = Mathf.Min(flagInRange.Flag.Value, flagInRange.MinValue);
			MinEtudeCounter = flagInRange.MinValue;
			if (blueprint.BlueprintCounterEtudeDescription != null)
			{
				EtudeCounterDescription = blueprint.BlueprintCounterEtudeDescription;
			}
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
