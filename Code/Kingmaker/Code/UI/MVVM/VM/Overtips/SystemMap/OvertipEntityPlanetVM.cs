using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;

public class OvertipEntityPlanetVM : OvertipEntityVM, IExplorationUIHandler, ISubscriber, IGameModeHandler, IAreaLoadingStagesHandler
{
	public readonly MapObjectEntity PlanetObject;

	public readonly ReactiveProperty<bool> HasColony = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasQuest = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasRumour = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> QuestObjectiveName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> RumourObjectiveName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> HasResource = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasExtractor = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasPoi = new ReactiveProperty<bool>();

	public readonly BoolReactiveProperty PlanetIsScanned = new BoolReactiveProperty();

	public readonly ReactiveProperty<PlanetView> PlanetView = new ReactiveProperty<PlanetView>();

	public readonly ReactiveProperty<StarSystemObjectEntity> StarSystemObjectEntity = new ReactiveProperty<StarSystemObjectEntity>();

	public readonly AutoDisposingList<TooltipBrickResourceInfoVM> Resources = new AutoDisposingList<TooltipBrickResourceInfoVM>();

	public readonly AutoDisposingList<TooltipBrickResourceInfoVM> ExtractorResources = new AutoDisposingList<TooltipBrickResourceInfoVM>();

	public readonly ReactiveProperty<Colony> Colony = new ReactiveProperty<Colony>();

	public readonly ReactiveProperty<bool> UpdateColonizeHint = new ReactiveProperty<bool>();

	public readonly List<string> PoiNamesList = new List<string>();

	public readonly StringReactiveProperty PlanetName = new StringReactiveProperty();

	protected override Vector3 GetEntityPosition()
	{
		return PlanetObject.Position;
	}

	public OvertipEntityPlanetVM(MapObjectEntity planetObjectData)
	{
		AddDisposable(EventBus.Subscribe(this));
		PlanetObject = planetObjectData;
		PlanetView.Value = PlanetObject.View.gameObject.GetComponent<PlanetView>();
		SetPlanetIconsState();
		PlanetIsScanned.Value = CheckScanned();
	}

	protected override void DisposeImplementation()
	{
		Resources.Clear();
		ExtractorResources.Clear();
	}

	public void RequestVisit()
	{
		VisitPlanet();
	}

	private void VisitPlanet()
	{
		Game.Instance.GameCommandQueue.MoveShip(PlanetObject, MoveShipGameCommand.VisitType.MovePlayerShip);
	}

	private void SetPlanetIconsState()
	{
		StarSystemObjectEntity data = PlanetObject.View.gameObject.GetComponent<StarSystemObjectView>().Data;
		StarSystemObjectEntity.Value = data;
		bool isScanned = data.IsScanned;
		PlanetView planetView = PlanetView.Value;
		HasColony.Value = planetView != null && planetView.Data.Colony != null;
		if (planetView.Data.Colony != null)
		{
			Colony.Value = planetView.Data.Colony;
		}
		List<QuestObjective> questsForPlanet = UIUtilitySpaceQuests.GetQuestsForPlanet(planetView.Data.Blueprint);
		HasQuest.Value = questsForPlanet != null && !questsForPlanet.Empty();
		if (HasQuest.Value)
		{
			List<string> list = questsForPlanet?.Where((QuestObjective quest) => !string.IsNullOrWhiteSpace(quest.Blueprint.GetTitile())).Select((QuestObjective quest, int index) => $"{index + 1}. " + quest.Blueprint.GetTitile()).ToList();
			if (list != null && list.Any())
			{
				QuestObjectiveName.Value = string.Join(Environment.NewLine, list);
			}
		}
		List<QuestObjective> rumoursForPlanet = UIUtilitySpaceQuests.GetRumoursForPlanet(planetView.Data.Blueprint);
		HasRumour.Value = rumoursForPlanet != null && !rumoursForPlanet.Empty();
		if (HasRumour.Value)
		{
			List<string> list2 = rumoursForPlanet?.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile())).Select((QuestObjective rumour, int index) => $"{index + 1}. " + rumour.Blueprint.GetTitile()).ToList();
			if (list2 != null && list2.Any())
			{
				RumourObjectiveName.Value = string.Join(Environment.NewLine, list2);
			}
		}
		Dictionary<BlueprintResource, int> resources = data.ResourcesOnObject;
		List<ColoniesState.MinerData> miners = Game.Instance.Player.ColoniesState.Miners;
		List<ColoniesState.MinerData> minersResourcesPlanet = (from m in miners
			where m.Sso == planetView.Data.Blueprint && resources.ContainsKey(m.Resource)
			group m by m.Resource into @group
			select @group.FirstOrDefault()).ToList();
		Dictionary<BlueprintResource, int> dictionary = resources.Where((KeyValuePair<BlueprintResource, int> pair) => minersResourcesPlanet.All((ColoniesState.MinerData miner) => miner.Resource != pair.Key)).ToDictionary((KeyValuePair<BlueprintResource, int> pair) => pair.Key, (KeyValuePair<BlueprintResource, int> pair) => pair.Value);
		AddResourceInfo(dictionary);
		AddExtractorResourceInfo(minersResourcesPlanet);
		ReactiveProperty<bool> hasResource = HasResource;
		List<KeyValuePair<BlueprintResource, int>> list3 = dictionary.ToList();
		hasResource.SetValueAndForceNotify(list3 != null && list3.Count > 0 && isScanned);
		HasExtractor.SetValueAndForceNotify(minersResourcesPlanet != null && minersResourcesPlanet.Count > 0 && isScanned);
		HasPoi.Value = PoiIsVisible(data, isScanned);
		UpdateColonizeHint.Value = !UpdateColonizeHint.Value;
	}

	private void AddResourceInfo(Dictionary<BlueprintResource, int> resources)
	{
		Resources.Clear();
		foreach (KeyValuePair<BlueprintResource, int> resource in resources)
		{
			BlueprintResource key = resource.Key;
			if (key != null)
			{
				Resources.Add(new TooltipBrickResourceInfoVM(key, resource.Value));
			}
		}
	}

	private void AddExtractorResourceInfo(List<ColoniesState.MinerData> miners)
	{
		ExtractorResources.Clear();
		foreach (ColoniesState.MinerData miner in miners)
		{
			ExtractorResources.Add(new TooltipBrickResourceInfoVM(miner.Resource, ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(miner)));
		}
	}

	private bool PoiIsVisible(StarSystemObjectEntity star, bool scan)
	{
		bool num = star.PointOfInterests.Count > 0 && !star.IsFullyExplored && scan;
		bool result = false;
		PoiNamesList.Clear();
		if (num)
		{
			IEnumerable<BasePointOfInterest> source = star.PointOfInterests.Where((BasePointOfInterest p) => p.IsVisible() && p.Status != BasePointOfInterest.ExplorationStatus.Explored);
			if (source.Count() != 0)
			{
				source.ForEach(delegate(BasePointOfInterest p)
				{
					PoiNamesList.Add("- " + ((!string.IsNullOrWhiteSpace(p.Blueprint.Name)) ? p.Blueprint.Name : UIStrings.Instance.ExplorationTexts.GetPointOfInterestTypeName(p.Blueprint)));
				});
				result = true;
			}
		}
		return result;
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
	}

	public void CloseExplorationScreen()
	{
		CheckIconStateAndScan();
	}

	private bool CheckScanned()
	{
		SetPlanetName();
		PlanetEntity data = PlanetView.Value.Data;
		if (!data.IsScanned)
		{
			return data.IsScannedOnStart;
		}
		return true;
	}

	private void SetPlanetName()
	{
		PlanetName.Value = (string.IsNullOrWhiteSpace(PlanetObject.View.Data.Name) ? "Empty Name" : PlanetObject.View.Data.Name);
	}

	private void CheckIconStateAndScan()
	{
		SetPlanetIconsState();
		PlanetIsScanned.Value = CheckScanned();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			CheckIconStateAndScan();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			CheckIconStateAndScan();
		}
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			CheckIconStateAndScan();
		}
	}
}
