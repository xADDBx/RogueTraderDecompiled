using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateSystemMapPlanet : TooltipBaseTemplate
{
	private readonly MapObjectView m_SystemMapObject;

	private readonly PlanetView m_PlanetView;

	private readonly BlueprintPlanet m_BlueprintPlanet;

	private readonly bool m_IsScanned;

	private readonly Colony m_Colony;

	private readonly BlueprintStarSystemMap m_BlueprintStarSystemMap;

	public TooltipTemplateSystemMapPlanet(MapObjectView mapObject, PlanetView planetView, BlueprintPlanet blueprintPlanet = null, BlueprintStarSystemMap blueprintStarSystemMap = null)
	{
		try
		{
			m_SystemMapObject = mapObject;
			m_PlanetView = planetView;
			m_BlueprintPlanet = blueprintPlanet;
			m_BlueprintStarSystemMap = blueprintStarSystemMap;
			if (m_BlueprintPlanet != null)
			{
				m_IsScanned = Game.Instance.Player.StarSystemsState.ScannedPlanets.Where((PlanetExplorationInfo info) => info.Planet == m_BlueprintPlanet).EmptyIfNull().Any();
				List<ColoniesState.ColonyData> colonies = Game.Instance.Player.ColoniesState.Colonies;
				m_Colony = colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Planet == m_BlueprintPlanet)?.Colony;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {mapObject?.name}: {arg}");
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		Color pictureColor = new Color(0.172549f, 0.172549f, 0.1607843f);
		if (m_BlueprintPlanet != null)
		{
			if (m_IsScanned || m_BlueprintPlanet.IsScannedOnStart)
			{
				yield return new TooltipBrickTitle(string.IsNullOrWhiteSpace(m_BlueprintPlanet.Name) ? "Empty Name" : m_BlueprintPlanet.Name, TooltipTitleType.H1, TextAlignmentOptions.Left, TextAnchor.MiddleLeft);
			}
			else
			{
				yield return new TooltipBrickPicture(UIConfig.Instance.UIIcons.TooltipIcons.UnknownPlanet, pictureColor);
			}
			yield break;
		}
		PlanetEntity data = m_PlanetView.Data;
		if (data.IsScanned || data.IsScannedOnStart)
		{
			yield return new TooltipBrickTitle(string.IsNullOrWhiteSpace(data.Name) ? "Empty Name" : data.Name, TooltipTitleType.H1, TextAlignmentOptions.Left, TextAnchor.MiddleLeft);
		}
		else
		{
			yield return new TooltipBrickPicture(UIConfig.Instance.UIIcons.TooltipIcons.UnknownPlanet, pictureColor);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (m_SystemMapObject != null && m_SystemMapObject.gameObject.GetComponent<StarSystemObjectView>().Data.Blueprint is BlueprintStar)
		{
			return list;
		}
		AddVisitInfo(list);
		bool num = ((m_BlueprintPlanet != null) ? m_IsScanned : m_SystemMapObject.gameObject.GetComponent<StarSystemObjectView>().Data.IsScanned);
		if (num)
		{
			AddColonyInfo(list);
		}
		AddQuestsInfo(list);
		AddRumoursInfo(list);
		if (!num)
		{
			return list;
		}
		AddPointsOfInterestInfo(list);
		AddResourcesInfo(list);
		AddEventsInfo(list);
		return list;
	}

	private void AddQuestsInfo(List<ITooltipBrick> bricks)
	{
		List<QuestObjective> questsForPlanet = UIUtilitySpaceQuests.GetQuestsForPlanet(m_BlueprintPlanet ?? m_PlanetView.Data.Blueprint);
		if (questsForPlanet != null && !questsForPlanet.Empty())
		{
			List<string> list = questsForPlanet.Where((QuestObjective quest) => !string.IsNullOrWhiteSpace(quest.Blueprint.GetTitile())).Select((QuestObjective quest, int index) => $"{index + 1}. " + quest.Blueprint.GetTitile()).ToList();
			if (list.Any())
			{
				string text = string.Join(Environment.NewLine, list);
				bricks.Add(new TooltipBrickTitle(UIStrings.Instance.QuesJournalTexts.Quests, TooltipTitleType.H3, TextAlignmentOptions.Left, TextAnchor.MiddleLeft));
				bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Quest, text));
			}
		}
	}

	private void AddRumoursInfo(List<ITooltipBrick> bricks)
	{
		List<QuestObjective> rumoursForPlanet = UIUtilitySpaceQuests.GetRumoursForPlanet(m_BlueprintPlanet ?? m_PlanetView.Data.Blueprint);
		if (rumoursForPlanet != null && !rumoursForPlanet.Empty())
		{
			List<string> list = rumoursForPlanet.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile())).Select((QuestObjective rumour, int index) => $"{index + 1}. " + rumour.Blueprint.GetTitile()).ToList();
			if (list.Any())
			{
				string text = string.Join(Environment.NewLine, list);
				bricks.Add(new TooltipBrickTitle(UIStrings.Instance.QuesJournalTexts.Rumours, TooltipTitleType.H3, TextAlignmentOptions.Left, TextAnchor.MiddleLeft));
				bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Rumour, text));
			}
		}
	}

	private void AddVisitInfo(List<ITooltipBrick> bricks)
	{
		bool flag = ((m_BlueprintPlanet != null) ? m_IsScanned : m_SystemMapObject.gameObject.GetComponent<StarSystemObjectView>().Data.IsScanned);
		bricks.Add(new TooltipBrickTitle(flag ? string.Concat("- ", UIStrings.Instance.ExplorationTexts.ExploAlreadyExplored, " -") : string.Concat("- ", UIStrings.Instance.ExplorationTexts.ExploNotExplored, " -"), TooltipTitleType.H6, TextAlignmentOptions.Left, TextAnchor.MiddleLeft));
		if (!flag)
		{
			bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Colonized, UIStrings.Instance.SystemMap.ScanRequired));
		}
	}

	private void AddColonyInfo(List<ITooltipBrick> bricks)
	{
		bool flag;
		bool flag2;
		if (m_BlueprintPlanet != null)
		{
			flag = m_Colony != null;
			flag2 = m_Colony == null && !Game.Instance.Player.ColoniesState.ForbidColonization && m_BlueprintPlanet.GetComponent<ColonyComponent>() != null;
		}
		else
		{
			PlanetView component = m_SystemMapObject.gameObject.GetComponent<PlanetView>();
			flag = component != null && component.Data.Colony != null;
			flag2 = component != null && component.Data.CanBeColonized;
		}
		if (flag)
		{
			bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Colonized, UIStrings.Instance.SystemMap.PlanetColonized));
		}
		else if (flag2)
		{
			bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Colonized, UIStrings.Instance.SystemMap.PlanetNotColonized));
		}
	}

	private void AddResourcesInfo(List<ITooltipBrick> bricks)
	{
		Dictionary<BlueprintResource, int> dictionary = new Dictionary<BlueprintResource, int>();
		if (m_BlueprintPlanet != null)
		{
			ResourceData[] array = m_BlueprintPlanet.Resources.EmptyIfNull();
			foreach (ResourceData resourceData in array)
			{
				dictionary.Add(resourceData.Resource.Get(), resourceData.Count);
			}
		}
		else
		{
			dictionary = m_SystemMapObject.gameObject.GetComponent<StarSystemObjectView>().Data.ResourcesOnObject;
		}
		if (dictionary == null || dictionary.Count <= 0)
		{
			return;
		}
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.ExplorationTexts.ExploObjectResources, TooltipTitleType.H3, TextAlignmentOptions.Left, TextAnchor.MiddleLeft));
		dictionary.ForEach(delegate(KeyValuePair<BlueprintResource, int> resourceRef)
		{
			BlueprintResource key = resourceRef.Key;
			if (key != null)
			{
				bricks.Add(new TooltipBrickResourceInfo(key, resourceRef.Value));
			}
		});
	}

	private void AddEventsInfo(List<ITooltipBrick> bricks)
	{
		PlanetView planetView = null;
		bool flag;
		if (m_BlueprintPlanet != null)
		{
			flag = m_Colony != null;
		}
		else
		{
			planetView = m_SystemMapObject.gameObject.GetComponent<PlanetView>();
			flag = planetView != null && planetView.Data.Colony != null;
		}
		if (!flag)
		{
			return;
		}
		List<BlueprintColonyEvent> list = ((m_BlueprintPlanet != null) ? m_Colony.StartedEvents : ((planetView != null) ? planetView.Data.Colony.StartedEvents : null));
		if (list == null || list.Count == 0)
		{
			return;
		}
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.CombatTexts.CombatLogEventsFilter, TooltipTitleType.H3, TextAlignmentOptions.Left, TextAnchor.MiddleLeft));
		foreach (BlueprintColonyEvent item in list)
		{
			bricks.Add(new TooltipBrickEvent(item));
		}
	}

	private void AddPointsOfInterestInfo(List<ITooltipBrick> bricks)
	{
		List<BlueprintPointOfInterest> list = new List<BlueprintPointOfInterest>();
		IEnumerable<BasePointOfInterest> enumerable = null;
		bool flag;
		if (m_BlueprintPlanet != null && m_BlueprintStarSystemMap != null)
		{
			BlueprintComponentsEnumerator<BasePointOfInterestComponent> components = m_BlueprintPlanet.GetComponents<BasePointOfInterestComponent>();
			if (components.Empty())
			{
				return;
			}
			foreach (BasePointOfInterestComponent item in components)
			{
				Game.Instance.Player.StarSystemsState.InteractedPoints.TryGetValue(m_BlueprintStarSystemMap, out var value);
				if (item.PointBlueprint.IsVisible() && (value == null || !value.Contains(item.PointBlueprint)))
				{
					list.Add(item.m_PointBlueprint);
				}
			}
			flag = list.Any();
		}
		else
		{
			enumerable = m_SystemMapObject.gameObject.GetComponent<StarSystemObjectView>().Data.PointOfInterests.Where((BasePointOfInterest p) => p.IsVisible() && p.Status != BasePointOfInterest.ExplorationStatus.Explored);
			flag = enumerable.Any() && !m_SystemMapObject.gameObject.GetComponent<StarSystemObjectView>().Data.IsFullyExplored;
		}
		if (!flag)
		{
			return;
		}
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.ExplorationTexts.ExploPointsOfInterest, TooltipTitleType.H3, TextAlignmentOptions.Left, TextAnchor.MiddleLeft));
		if (m_BlueprintPlanet != null)
		{
			foreach (BlueprintPointOfInterest item2 in list)
			{
				bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Info, (!string.IsNullOrWhiteSpace(item2.Name)) ? item2.Name : UIStrings.Instance.ExplorationTexts.GetPointOfInterestTypeName(item2)));
			}
			return;
		}
		if (enumerable == null)
		{
			return;
		}
		foreach (BasePointOfInterest item3 in enumerable)
		{
			bricks.Add(new TooltipBrickUnifiedStatus(UnifiedStatus.Info, (!string.IsNullOrWhiteSpace(item3.Blueprint.Name)) ? item3.Blueprint.Name : UIStrings.Instance.ExplorationTexts.GetPointOfInterestTypeName(item3.Blueprint)));
		}
	}
}
