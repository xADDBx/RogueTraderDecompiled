using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.SectorMap;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Registry;
using UnityEngine;

namespace Kingmaker.Controllers.GlobalMap;

public class SectorMapController : IControllerEnable, IController, IControllerTick, IControllerDisable, IAreaHandler, ISubscriber, IControllerStart
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("SectorMapController");

	private SectorMapObjectEntity m_CurrentStarSystem;

	private SectorMapObjectEntity m_PreviousStarSystem;

	private readonly float m_ScanDuration = 3f;

	private bool m_IsScanning;

	private TimeSpan m_EndScanTime;

	private readonly Dictionary<SectorMapObjectEntity, List<SectorMapPassageEntity>> m_Passages = new Dictionary<SectorMapObjectEntity, List<SectorMapPassageEntity>>();

	public SectorMapVisualParameters VisualParameters;

	private readonly List<SectorMapObjectEntity> m_NeedToScan = new List<SectorMapObjectEntity>();

	private List<SectorMapObjectEntity> m_NeedToExplore = new List<SectorMapObjectEntity>();

	public bool IsInformationWindowInspectMode;

	public bool CanJumpToWarp => !(Game.Instance.LoadedAreaState?.Settings.CannotJumpToWarp);

	public SectorMapObjectEntity CurrentStarSystem => m_CurrentStarSystem;

	public SectorMapObjectEntity PreviousStarSystem => m_PreviousStarSystem;

	public bool IsScanning => m_IsScanning;

	public void OnStart()
	{
		InitCurrentStarSystem();
		m_PreviousStarSystem = m_CurrentStarSystem;
	}

	public void OnEnable()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.GlobalMap))
		{
			if (!Game.Instance.Player.WarpTravelState.IsInitialized)
			{
				Game.Instance.Player.WarpTravelState.Init();
			}
			RecalculatePassages();
		}
	}

	public void RecalculatePassages()
	{
		m_Passages.Clear();
		foreach (SectorMapPassageEntity item in Game.Instance.State.Entities.OfType<SectorMapPassageEntity>().ToList())
		{
			if (item.View?.StarSystem1Entity != null && item.View?.StarSystem2Entity != null)
			{
				if (!m_Passages.ContainsKey(item.View.StarSystem1Entity))
				{
					m_Passages[item.View.StarSystem1Entity] = new List<SectorMapPassageEntity>();
				}
				if (!m_Passages.ContainsKey(item.View.StarSystem2Entity))
				{
					m_Passages[item.View.StarSystem2Entity] = new List<SectorMapPassageEntity>();
				}
				m_Passages[item.View.StarSystem1Entity].Add(item);
				m_Passages[item.View.StarSystem2Entity].Add(item);
			}
		}
	}

	public void OnDisable()
	{
		Game.Instance.Player.PreviousVisitedArea = Game.Instance.CurrentlyLoadedArea;
		Game.Instance.Player.LastPositionOnPreviousVisitedArea = Game.Instance.Player.PlayerShip.Position;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!m_IsScanning)
		{
			return;
		}
		if (Game.Instance.TimeController.GameTime >= m_EndScanTime)
		{
			EndScan();
			return;
		}
		float pulseRadius = SectorMapView.Instance.ScanPulse.PulseRadius;
		while (m_NeedToScan.Any())
		{
			List<SectorMapObjectEntity> needToScan = m_NeedToScan;
			SectorMapObjectEntity sectorMapObjectEntity = needToScan[needToScan.Count - 1];
			if ((sectorMapObjectEntity.Position - m_CurrentStarSystem.Position).magnitude * 2f <= pulseRadius)
			{
				ScanObject(sectorMapObjectEntity);
				m_NeedToScan.RemoveAt(m_NeedToScan.Count - 1);
				continue;
			}
			break;
		}
	}

	private void ScanObject(SectorMapObjectEntity sectorMapObject)
	{
		SectorMapPassageEntity passage = (sectorMapObject.View.IsSystem ? FindPassageBetween(sectorMapObject, m_CurrentStarSystem) : null);
		passage?.Explore();
		if (!sectorMapObject.IsExplored)
		{
			FxHelper.SpawnFxOnGameObject(FxRoot.Instance.SystemExplorationFx, sectorMapObject.View.gameObject);
		}
		sectorMapObject.Explore();
		EventBus.RaiseEvent((ISectorMapObjectEntity)sectorMapObject, (Action<ISectorMapScanHandler>)delegate(ISectorMapScanHandler h)
		{
			h.HandleSectorMapObjectScanned(passage?.View);
		}, isCheckRuntime: true);
	}

	public void Scan()
	{
		if (m_CurrentStarSystem == null)
		{
			Logger.Error("Trying to scan from null sectorMapObject");
		}
		else
		{
			if (m_IsScanning)
			{
				return;
			}
			m_IsScanning = true;
			m_NeedToScan.Clear();
			foreach (SectorMapObjectEntity sectorMapObject in Game.Instance.State.SectorMapObjects)
			{
				if ((sectorMapObject.Position - m_CurrentStarSystem.Position).magnitude < Game.Instance.Player.WarpTravelState.ScanRadius && !sectorMapObject.IsHidden)
				{
					m_NeedToScan.Add(sectorMapObject);
				}
			}
			m_NeedToScan.Sort((SectorMapObjectEntity starSystem1, SectorMapObjectEntity starSystem2) => -(starSystem1.Position - m_CurrentStarSystem.Position).magnitude.CompareTo((starSystem2.Position - m_CurrentStarSystem.Position).magnitude));
			m_EndScanTime = Game.Instance.TimeController.GameTime + m_ScanDuration.Segments();
			EventBus.RaiseEvent((ISectorMapObjectEntity)m_CurrentStarSystem, (Action<ISectorMapScanHandler>)delegate(ISectorMapScanHandler h)
			{
				h.HandleScanStarted(Game.Instance.Player.WarpTravelState.ScanRadius, m_ScanDuration);
			}, isCheckRuntime: true);
		}
	}

	private void EndScan()
	{
		m_IsScanning = false;
		foreach (SectorMapObjectEntity item in m_NeedToScan)
		{
			ScanObject(item);
		}
		m_NeedToScan.Clear();
		m_CurrentStarSystem.IsScannedFrom = true;
		EventBus.RaiseEvent((ISectorMapObjectEntity)m_CurrentStarSystem, (Action<ISectorMapScanHandler>)delegate(ISectorMapScanHandler h)
		{
			h.HandleScanStopped();
		}, isCheckRuntime: true);
		ChangeNavigatorResourceCount(BlueprintWarhammerRoot.Instance.WarpRoutesSettings.NavigatorResourceGainedForScan);
	}

	public void WarpTravel(SectorMapObject sectorMapObjectTo)
	{
		if (sectorMapObjectTo.IsSystem)
		{
			SectorMapPassageEntity sectorMapPassageEntity = FindPassageBetween(m_CurrentStarSystem, sectorMapObjectTo.Data);
			if (sectorMapPassageEntity != null && sectorMapPassageEntity.IsExplored)
			{
				SectorMapObjectEntity currentStarSystem = m_CurrentStarSystem;
				Game.Instance.SectorMapTravelController.WarpTravel(currentStarSystem, sectorMapObjectTo.Data);
			}
		}
	}

	public static void VisitStarSystem(SectorMapObjectEntity starSystem)
	{
		Game.Instance.GameCommandQueue.VisitStarSystem(starSystem);
	}

	public void VisitStarSystemInternal(SectorMapObjectEntity starSystem)
	{
		if (LoadingProcess.Instance.IsLoadingInProcess)
		{
			Logger.Warning("Trying to visit system {0} while loading is already in progress", starSystem);
			return;
		}
		UpdateCurrentStarSystem(starSystem);
		starSystem.Visit();
		SectorMapObjectEntity currentStarSystem = m_CurrentStarSystem;
		if (currentStarSystem.View.StarSystemToTransit != null)
		{
			Game.Instance.LoadArea(currentStarSystem.View.StarSystemAreaPoint, AutoSaveMode.None);
		}
	}

	public void UpdateCurrentStarSystem(SectorMapObjectEntity starSystem)
	{
		m_PreviousStarSystem = m_CurrentStarSystem;
		m_CurrentStarSystem.SetDecalVisible(state: false);
		m_CurrentStarSystem = starSystem;
		Game.Instance.Player.CurrentStarSystem = (starSystem.Blueprint as BlueprintSectorMapPointStarSystem)?.StarSystemToTransit?.Get() as BlueprintStarSystemMap;
		if (m_PreviousStarSystem != m_CurrentStarSystem)
		{
			EventBus.RaiseEvent((ISectorMapObjectEntity)starSystem, (Action<ISectorMapStarSystemChangeHandler>)delegate(ISectorMapStarSystemChangeHandler h)
			{
				h.HandleStarSystemChanged();
			}, isCheckRuntime: true);
		}
	}

	public void JumpToSectorMap()
	{
		if (!(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap))
		{
			Logger.Warning("Trying to go to warp not from star system map");
		}
		else if (CanJumpToWarp)
		{
			Game.Instance.GameCommandQueue.LoadArea(BlueprintRoot.Instance.SectorMapArea.SectorMapEnterPoint, AutoSaveMode.None);
		}
	}

	public void JumpToShipArea()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		if (playerShip.Blueprint.ShipAreaEnterPoint != null)
		{
			Game.Instance.GameCommandQueue.LoadArea(playerShip.Blueprint.ShipAreaEnterPoint, AutoSaveMode.None);
		}
		else
		{
			Logger.Warning($"ShipAreaEnterPoint was not set in {playerShip.Blueprint}");
		}
	}

	private SectorMapObjectEntity GetMapObject(BlueprintSectorMapPoint blueprintPoint)
	{
		return Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity point) => point.View.Blueprint == blueprintPoint);
	}

	private SectorMapObjectEntity GetMapObject(BlueprintArea blueprint)
	{
		if (blueprint != null)
		{
			return Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity point) => point.View.StarSystemToTransit == blueprint);
		}
		return null;
	}

	public List<SectorMapObject> GetAllSectorMapObjects()
	{
		List<SectorMapObject> list = new List<SectorMapObject>();
		foreach (SectorMapObjectEntity sectorMapObject in Game.Instance.State.SectorMapObjects)
		{
			list.Add(sectorMapObject.View);
		}
		return list;
	}

	public List<SectorMapObject> GetAllStarSystems()
	{
		return GetAllSectorMapObjects().FindAll((SectorMapObject point) => point != null && point.IsSystem);
	}

	public SectorMapObject GetCurrentStarSystem()
	{
		return m_CurrentStarSystem.View;
	}

	public SectorMapObject GetPreviousStarSystem()
	{
		return PreviousStarSystem.View;
	}

	public List<SectorMapObject> GetStarSystemsToTravel()
	{
		List<SectorMapPassageEntity> list = AllPassagesForSystem(m_CurrentStarSystem);
		List<SectorMapObject> list2 = new List<SectorMapObject>();
		foreach (SectorMapPassageEntity item in list)
		{
			if (item.IsExplored)
			{
				if (item.View != null && item.View.StarSystem1Entity == m_CurrentStarSystem && item.View.StarSystem2Entity?.View != null)
				{
					list2.Add(item.View.StarSystem2Entity.View);
				}
				else if (item.View != null && item.View.StarSystem1Entity != m_CurrentStarSystem && item.View.StarSystem1Entity?.View != null)
				{
					list2.Add(item.View.StarSystem1Entity.View);
				}
			}
		}
		return list2;
	}

	public void OnAreaBeginUnloading()
	{
		m_IsScanning = false;
	}

	public void OnAreaDidLoad()
	{
		UpdateSectorMap();
	}

	public void UpdateSectorMap()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.GlobalMap))
		{
			InitCurrentStarSystem();
			VisualParameters = ObjectRegistry<SectorMapVisualParameters>.Instance.Single;
			Transform playerShip = VisualParameters.PlayerShip;
			if (!Game.Instance.SectorMapTravelController.IsTravelling)
			{
				playerShip.position = new Vector3(m_CurrentStarSystem.Position.x, playerShip.position.y, m_CurrentStarSystem.Position.z);
				playerShip.gameObject.SetActive(value: true);
			}
			Game.Instance.Player.PlayerShip.View.SetVisible(visible: false);
			Game.Instance.Player.PlayerShip.IsInGame = false;
			if (Game.Instance.Player.WarpTravelState.AllRoutesNotDeadlyFlag && !Game.Instance.Player.WarpTravelState.AllRoutesNotDeadlyChanged)
			{
				ChangeRoutesDifficulty(SectorMapPassageEntity.PassageDifficulty.Deadly, SectorMapPassageEntity.PassageDifficulty.Dangerous);
			}
		}
	}

	public List<SectorMapPassageEntity> AllPassagesForSystem(SectorMapObjectEntity starSystem)
	{
		m_Passages.TryGetValue(starSystem, out var value);
		return value ?? new List<SectorMapPassageEntity>();
	}

	public SectorMapPassageEntity FindPassageBetween(SectorMapObjectEntity starSystem1, SectorMapObjectEntity starSystem2)
	{
		if (starSystem1 == starSystem2)
		{
			return null;
		}
		return AllPassagesForSystem(starSystem1).FirstOrDefault((SectorMapPassageEntity passage) => passage.View.StarSystem1Entity == starSystem2 || passage.View.StarSystem2Entity == starSystem2);
	}

	public void ChangeScanRadius(float radius)
	{
		int increaseScanRadiusCost = BlueprintWarhammerRoot.Instance.WarpRoutesSettings.IncreaseScanRadiusCost;
		if (Game.Instance.Player.WarpTravelState.NavigatorResource < increaseScanRadiusCost)
		{
			Logger.Log("Not enough navigator resource");
			return;
		}
		Game.Instance.Player.WarpTravelState.ScanRadius = radius;
		ChangeNavigatorResourceCount(-increaseScanRadiusCost);
	}

	public SectorMapPassageEntity GenerateNewPassage(SectorMapObjectEntity from, SectorMapObjectEntity to)
	{
		int createNewPassageCost = Game.Instance.Player.WarpTravelState.CreateNewPassageCost;
		if (Game.Instance.Player.WarpTravelState.NavigatorResource < createNewPassageCost)
		{
			Logger.Log("Not enough navigator resource");
			return null;
		}
		if (FindPassageBetween(from, to) != null)
		{
			return null;
		}
		ChangeNavigatorResourceCount(-createNewPassageCost);
		SectorMapPassageEntity.PassageDifficulty difficulty = (Game.Instance.Player.WarpTravelState.AllRoutesNotDeadlyFlag ? SectorMapPassageEntity.PassageDifficulty.Dangerous : SectorMapPassageEntity.PassageDifficulty.Deadly);
		SectorMapPassageEntity sectorMapPassageEntity = PassagesGenerator.GeneratePassage(from.View, to.View, difficulty);
		sectorMapPassageEntity.Explore();
		to.Explore();
		m_Passages[sectorMapPassageEntity.View.StarSystem1Entity].Add(sectorMapPassageEntity);
		m_Passages[sectorMapPassageEntity.View.StarSystem2Entity].Add(sectorMapPassageEntity);
		EventBus.RaiseEvent((ISectorMapPassageEntity)sectorMapPassageEntity, (Action<ISectorMapPassageChangeHandler>)delegate(ISectorMapPassageChangeHandler h)
		{
			h.HandleNewPassageCreated();
		}, isCheckRuntime: true);
		return sectorMapPassageEntity;
	}

	public void LowerPassageDifficulty(SectorMapObjectEntity to, SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		SectorMapPassageEntity sectorMapPassageEntity = FindPassageBetween(CurrentStarSystem, to);
		if (sectorMapPassageEntity == null)
		{
			Logger.Log("No passage between " + CurrentStarSystem.Blueprint.Name + " and " + to.Blueprint.Name);
			return;
		}
		int num = (sectorMapPassageEntity.CurrentDifficulty - difficulty) * BlueprintWarhammerRoot.Instance.WarpRoutesSettings.LowerPassageDifficultyCost;
		if (Game.Instance.Player.WarpTravelState.NavigatorResource < num)
		{
			Logger.Log("Not enough navigator resource");
		}
		else if (sectorMapPassageEntity.CurrentDifficulty == SectorMapPassageEntity.PassageDifficulty.Safe)
		{
			Logger.Log("Passage from " + CurrentStarSystem.Blueprint.Name + " to " + to.Blueprint.Name + " is already the safest");
		}
		else if (difficulty >= sectorMapPassageEntity.CurrentDifficulty)
		{
			Logger.Log("New difficulty is more then current difficulty");
		}
		else
		{
			sectorMapPassageEntity.LowerDifficulty(difficulty);
			ChangeNavigatorResourceCount(-num);
			EventBus.RaiseEvent((ISectorMapPassageEntity)sectorMapPassageEntity, (Action<ISectorMapPassageChangeHandler>)delegate(ISectorMapPassageChangeHandler h)
			{
				h.HandlePassageLowerDifficulty();
			}, isCheckRuntime: true);
		}
	}

	private void InitCurrentStarSystem()
	{
		m_CurrentStarSystem = GetMapObject(Game.Instance.Player.CurrentStarSystem) ?? GetMapObject(BlueprintRoot.Instance.SectorMapArea.DefaultStarSystem);
		if (!m_CurrentStarSystem.IsExplored)
		{
			m_CurrentStarSystem.Explore();
		}
	}

	public void ChangeNavigatorResourceCount(int value)
	{
		Game.Instance.Player.WarpTravelState.NavigatorResource += value;
		EventBus.RaiseEvent(delegate(INavigatorResourceCountChangedHandler h)
		{
			h.HandleChaneNavigatorResourceCount(value);
		});
	}

	public void TryChangeDifficultyNotDeadly()
	{
		Game.Instance.Player.WarpTravelState.AllRoutesNotDeadlyFlag = true;
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap)
		{
			ChangeRoutesDifficulty(SectorMapPassageEntity.PassageDifficulty.Deadly, SectorMapPassageEntity.PassageDifficulty.Dangerous);
		}
	}

	private void ChangeRoutesDifficulty(SectorMapPassageEntity.PassageDifficulty prevDifficulty, SectorMapPassageEntity.PassageDifficulty newDifficulty)
	{
		foreach (List<SectorMapPassageEntity> value in m_Passages.Values)
		{
			foreach (SectorMapPassageEntity item in value)
			{
				if (item.CurrentDifficulty == prevDifficulty)
				{
					item.LowerDifficulty(newDifficulty);
					EventBus.RaiseEvent((ISectorMapPassageEntity)item, (Action<ISectorMapPassageChangeHandler>)delegate(ISectorMapPassageChangeHandler h)
					{
						h.HandlePassageLowerDifficulty();
					}, isCheckRuntime: true);
				}
			}
		}
		Game.Instance.Player.WarpTravelState.AllRoutesNotDeadlyChanged = true;
	}
}
