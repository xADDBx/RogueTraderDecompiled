using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
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
	public readonly QuestObjective Objective;

	public readonly List<JournalQuestObjectiveAddendumVM> Addendums;

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

	public JournalQuestObjectiveVM(QuestObjective objective)
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
		QuestObjective questObjective = objective;
		ObjectiveNumber = ((questObjective == null) ? null : (questObjective.Quest?.Objectives?.Where((QuestObjective o) => o.IsVisible && o.State != 0 && !o.Blueprint.IsAddendum && !o.Blueprint.IsErrandObjective).ToList().IndexOf(objective) + 1)).GetValueOrDefault();
		RumourMapMarker rumourMapMarker = blueprintQuestObjective?.GetComponent<RumourMapMarker>();
		if (rumourMapMarker != null)
		{
			Destination = string.Join(", ", from mapPoint in rumourMapMarker.SectorMapPointsToVisit.Dereference()
				select mapPoint.Name);
		}
		IOrderedEnumerable<QuestObjective> orderedEnumerable = (from b in objective?.Blueprint?.Addendums?.Where((BlueprintQuestObjective b) => b != null)
			select objective?.Quest?.TryGetObjective(b) into a
			where a != null
			where a.IsVisible
			orderby a?.Order descending
			select a);
		Addendums = new List<JournalQuestObjectiveAddendumVM>();
		if (orderedEnumerable != null)
		{
			foreach (QuestObjective item in orderedEnumerable)
			{
				Addendums.Add(new JournalQuestObjectiveAddendumVM(item));
			}
		}
		QuestObjective questObjective2 = objective;
		IsFailed = questObjective2 != null && questObjective2.State == QuestObjectiveState.Failed;
		QuestObjective questObjective3 = objective;
		IsCompleted = questObjective3 != null && questObjective3.State == QuestObjectiveState.Completed;
		QuestObjective questObjective4 = objective;
		IsPostponed = questObjective4 != null && questObjective4.State == QuestObjectiveState.Postponed;
		QuestObjective questObjective5 = objective;
		int isHighlight;
		if (questObjective5 == null || questObjective5.State != QuestObjectiveState.Failed)
		{
			QuestObjective questObjective6 = objective;
			if (questObjective6 == null || questObjective6.State != QuestObjectiveState.Completed)
			{
				isHighlight = ((!IsViewed) ? 1 : 0);
				goto IL_0416;
			}
		}
		isHighlight = 1;
		goto IL_0416;
		IL_0416:
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
