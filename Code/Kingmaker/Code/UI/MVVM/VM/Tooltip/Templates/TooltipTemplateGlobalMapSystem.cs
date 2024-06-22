using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateGlobalMapSystem : TooltipBaseTemplate
{
	private readonly SectorMapObject m_SectorMapObject;

	private readonly BlueprintStarSystemMap m_Area;

	public TooltipTemplateGlobalMapSystem(SectorMapObject mapObject)
	{
		m_SectorMapObject = mapObject;
		m_Area = m_SectorMapObject.StarSystemBlueprint.StarSystemToTransit?.Get() as BlueprintStarSystemMap;
	}

	public TooltipTemplateGlobalMapSystem(BlueprintStarSystemMap area)
	{
		m_Area = area;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return null;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string title = (string.IsNullOrWhiteSpace(m_Area?.Name) ? (string.IsNullOrWhiteSpace(ObjectExtensions.Or(m_SectorMapObject, null)?.Name) ? string.Empty : ObjectExtensions.Or(m_SectorMapObject, null)?.Name) : m_Area?.Name);
		list.Add(new TooltipBrickTitle(title));
		if (m_SectorMapObject != null && !m_SectorMapObject.Data.IsVisited)
		{
			list.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Colonized, UIStrings.Instance.GlobalMap.UnknownSystem));
			AddSectorMapRumoursInRangeInfo(list);
			AddQuestsInfo(list);
			AddRumoursInfo(list);
			return list;
		}
		AddColonizationStatus(list);
		AddSectorMapRumoursInRangeInfo(list);
		AddQuestsInfo(list);
		AddRumoursInfo(list);
		AddActiveAnomaliesInfo(list, UIStrings.Instance.GlobalMap.HasEnemiesInSystem, allAnomalies: false, BlueprintAnomaly.AnomalyObjectType.Enemy);
		AddPlanetsInfo(list);
		AddOtherObjectsInfo(list);
		AddAdditionalAnomaliesInfo(list);
		return list;
	}

	private void AddResearchPercentInfo(List<ITooltipBrick> bricks)
	{
		string arg = $"{UIUtilityGetSpaceSystemExplored.GetPercentExplored(m_Area)}%";
		bricks.Add(new TooltipBrickIconAndTextWithCustomColors(string.Format(UIStrings.Instance.SystemMap.PercentExplored, arg), UIConfig.Instance.IconAndTextCustomColors.MagnifyingGlass, Color.black, Color.black, UIConfig.Instance.IconAndTextCustomColors.LightGreen));
	}

	private void AddColonizationStatus(List<ITooltipBrick> bricks)
	{
		if (((m_SectorMapObject != null) ? Game.Instance.ColonizationController.GetColony(m_SectorMapObject) : Game.Instance.ColonizationController.GetColonyWithBlueprint(m_Area)) != null)
		{
			bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Colonized, UIStrings.Instance.GlobalMap.SystemColonized));
		}
	}

	private void AddQuestsInfo(List<ITooltipBrick> bricks)
	{
		List<QuestObjective> list = ((m_SectorMapObject != null) ? UIUtilitySpaceQuests.GetQuestsForSystem(m_SectorMapObject) : UIUtilitySpaceQuests.GetQuestsForSystemWithBlueprint(m_Area));
		List<QuestObjective> questsForSpaceSystem = UIUtilitySpaceQuests.GetQuestsForSpaceSystem(m_Area);
		if ((!list.Empty() && list != null) || (!questsForSpaceSystem.Empty() && questsForSpaceSystem != null))
		{
			List<string> questsStringList = UIUtilitySpaceQuests.GetQuestsStringList(list, questsForSpaceSystem);
			if (questsStringList.Any())
			{
				string text = string.Join(Environment.NewLine, questsStringList);
				bricks.Add(new TooltipBrickTitle(UIStrings.Instance.QuesJournalTexts.Quests, TooltipTitleType.H3, TextAlignmentOptions.Left));
				bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Quest, text));
			}
		}
	}

	private void AddRumoursInfo(List<ITooltipBrick> bricks)
	{
		List<QuestObjective> list = ((m_SectorMapObject != null) ? UIUtilitySpaceQuests.GetRumoursForSystem(m_SectorMapObject) : UIUtilitySpaceQuests.GetRumoursForSystemWithBlueprint(m_Area));
		if (list != null && list.Any())
		{
			List<string> list2 = list.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile())).Select((QuestObjective rumour, int index) => $"{index + 1}. " + rumour.Blueprint.GetTitile()).ToList();
			if (list2.Any())
			{
				string text = string.Join(Environment.NewLine, list2);
				bricks.Add(new TooltipBrickTitle(UIStrings.Instance.QuesJournalTexts.Rumours, TooltipTitleType.H3, TextAlignmentOptions.Left));
				bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Rumour, text));
			}
		}
	}

	private void AddSectorMapRumoursInRangeInfo(List<ITooltipBrick> bricks)
	{
		if (m_SectorMapObject == null)
		{
			return;
		}
		List<QuestObjective> rumoursForSectorMap = UIUtilitySpaceQuests.GetRumoursForSectorMap(m_SectorMapObject);
		if (rumoursForSectorMap != null && rumoursForSectorMap.Any())
		{
			List<string> list = rumoursForSectorMap.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile().Text)).Select((QuestObjective rumour, int index) => rumour.Blueprint.GetTitile().Text).ToList();
			if (list.Any())
			{
				string text = string.Join(", ", list);
				bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Rumour, UIStrings.Instance.GlobalMap.WithinRumourRange.Text + ": " + text));
			}
		}
	}

	private void AddPlanetsInfo(List<ITooltipBrick> bricks)
	{
		List<BlueprintPlanet> list = ((m_SectorMapObject != null) ? m_SectorMapObject.Data.Planets : m_Area.Planets.Dereference().ToList());
		Game.Instance.Player.StarSystemsState.ScannedPlanets.Where((PlanetExplorationInfo info) => info.StarSystemMap == m_Area).EmptyIfNull();
		if (list != null && list.Count != 0)
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.GlobalMap.Planets, TooltipTitleType.H3, TextAlignmentOptions.Left));
			list.ForEach(delegate(BlueprintPlanet planet)
			{
				bricks.Add(new TooltipBrickPlanetInfo(planet, m_Area));
			});
		}
	}

	private void AddOtherObjectsInfo(List<ITooltipBrick> bricks)
	{
		List<BlueprintArtificialObject> list = ((m_SectorMapObject != null) ? m_SectorMapObject.Data.OtherObjects : m_Area.OtherObjects.Dereference().ToList());
		if (list != null && list.Count != 0)
		{
			bricks.Add(new TooltipBrickSpace());
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.GlobalMap.AsteroidsFieldDetected, TooltipTitleType.H3, TextAlignmentOptions.Left));
			list.ForEach(delegate(BlueprintArtificialObject obj)
			{
				bricks.Add(new TooltipBrickOtherObjectsInfo(obj, m_Area));
			});
		}
	}

	private void AddActiveAnomaliesInfo(List<ITooltipBrick> bricks, LocalizedString label, bool allAnomalies = true, BlueprintAnomaly.AnomalyObjectType type = BlueprintAnomaly.AnomalyObjectType.Default)
	{
		List<BlueprintAnomaly> list = ((m_SectorMapObject != null) ? m_SectorMapObject.Data.Anomalies : m_Area.AnomaliesResearchProgress.Select((AnomalyToCondition a) => a.Anomaly.Get()).ToList());
		if (list != null && list.Count != 0 && Game.Instance?.Player?.StarSystemsState?.InteractedAnomalies != null && ObjectExtensions.Or(m_SectorMapObject, null)?.Data?.StarSystemArea != null)
		{
			Game.Instance.Player.StarSystemsState.InteractedAnomalies.TryGetValue(m_SectorMapObject.Data.StarSystemArea, out var ia);
			ia = ia.EmptyIfNull().ToList();
			if (list.Where((BlueprintAnomaly an) => !ia.Contains(an) && !an.HideInUI && (allAnomalies || an.AnomalyType == type)).ToList().Count != 0)
			{
				bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Enemies, label));
			}
		}
	}

	private void AddAdditionalAnomaliesInfo(List<ITooltipBrick> bricks)
	{
		List<BlueprintAnomaly> list = ((!(m_SectorMapObject != null)) ? m_Area?.Anomalies?.Dereference()?.Where((BlueprintAnomaly anomaly) => anomaly.ShowOnGlobalMap).EmptyIfNull().ToList() : ObjectExtensions.Or(m_SectorMapObject, null)?.Data?.AnomaliesForGlobalMap);
		if (list == null || list.Count == 0)
		{
			return;
		}
		Game.Instance.Player.StarSystemsState.InteractedAnomalies.TryGetValue(m_SectorMapObject.Data.StarSystemArea ?? m_Area, out var value);
		foreach (BlueprintAnomaly item in value.EmptyIfNull())
		{
			if (list.Contains(item) && !item.HideInUI)
			{
				bricks.Add(new TooltipBrickAnomalyInfo(item, m_Area));
			}
		}
	}
}
