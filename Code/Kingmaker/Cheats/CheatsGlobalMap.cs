using System.Linq;
using Core.Cheats;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Cheats;

internal static class CheatsGlobalMap
{
	[Cheat(Name = "reveal_map", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RevealGlobalMap()
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
		{
			foreach (SectorMapObjectEntity item in Game.Instance.State.SectorMapObjects.ToList())
			{
				if (!item.IsExplored)
				{
					item.Explore();
				}
			}
			{
				foreach (SectorMapPassageEntity item2 in Game.Instance.State.Entities.OfType<SectorMapPassageEntity>())
				{
					if (!item2.IsExplored)
					{
						item2.Explore();
					}
				}
				return;
			}
		}
		PFLog.Default.Log("Cannot reveal not on global map area");
	}

	[Cheat(Name = "move_to_system", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void MoveToSystemOnGlobalMap(string systemPointBlueprintName)
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
		{
			SectorMapObjectEntity sectorMapObjectEntity = TryFindPointByName(systemPointBlueprintName);
			if (sectorMapObjectEntity != null)
			{
				Transform playerShip = Game.Instance.SectorMapController.VisualParameters.PlayerShip;
				playerShip.position = new Vector3(sectorMapObjectEntity.Position.x, playerShip.position.y, sectorMapObjectEntity.Position.z);
				playerShip.gameObject.SetActive(value: true);
				sectorMapObjectEntity.Explore();
				Game.Instance.SectorMapController.UpdateCurrentStarSystem(sectorMapObjectEntity);
				CameraRig.Instance.ScrollTo(Game.Instance.SectorMapController.GetCurrentStarSystem().Data.Position);
			}
		}
		else
		{
			PFLog.Default.Log("Cannot move not on global map area");
		}
	}

	[Cheat(Name = "lower_passage_difficulty", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void LowerPassageDifficulty(string systemPointBlueprintName, int difficulty)
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
		{
			SectorMapObjectEntity sectorMapObjectEntity = TryFindPointByName(systemPointBlueprintName);
			if (sectorMapObjectEntity != null)
			{
				Game.Instance.GameCommandQueue.LowerWarpRouteDifficulty(sectorMapObjectEntity, (SectorMapPassageEntity.PassageDifficulty)difficulty);
			}
		}
		else
		{
			PFLog.Default.Log("Cannot use cheat not on global map area");
		}
	}

	[Cheat(Name = "create_new_passage", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CreateNewPassage(string systemPointBlueprintName)
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
		{
			SectorMapObjectEntity sectorMapObjectEntity = TryFindPointByName(systemPointBlueprintName);
			if (sectorMapObjectEntity != null)
			{
				if (Game.Instance.SectorMapController.FindPassageBetween(Game.Instance.SectorMapController.CurrentStarSystem, sectorMapObjectEntity) != null)
				{
					PFLog.Default.Log("Passage from " + Game.Instance.SectorMapController.CurrentStarSystem.View.Blueprint.Name + " ot " + sectorMapObjectEntity.View.Blueprint.Name + " already exists");
				}
				else
				{
					Game.Instance.GameCommandQueue.CreateNewWarpRoute(Game.Instance.SectorMapController.CurrentStarSystem, sectorMapObjectEntity);
				}
			}
		}
		else
		{
			PFLog.Default.Log("Cannot use cheat not on global map area");
		}
	}

	[Cheat(Name = "set_scan_radius", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetScanRadius(int value)
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
		{
			Game.Instance.SectorMapController.ChangeScanRadius(value);
		}
		else
		{
			PFLog.Default.Log("Cannot use cheat not on global map area");
		}
	}

	[Cheat(Name = "add_navigator_resource", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddNavigatorResource(int value)
	{
		Game.Instance.SectorMapController.ChangeNavigatorResourceCount(value);
	}

	private static SectorMapObjectEntity TryFindPointByName(string systemPointBlueprintName)
	{
		BlueprintSectorMapPointStarSystem pointBlueprint = Utilities.GetBlueprint<BlueprintSectorMapPointStarSystem>(systemPointBlueprintName);
		if (pointBlueprint == null)
		{
			PFLog.Default.Log("Cannot find point blueprint with " + systemPointBlueprintName);
			return null;
		}
		SectorMapObjectEntity sectorMapObjectEntity = Game.Instance.State.SectorMapObjects.All.FirstOrDefault((SectorMapObjectEntity p) => p.Blueprint == pointBlueprint);
		if (sectorMapObjectEntity == null)
		{
			PFLog.Default.Log("Cannot find point " + systemPointBlueprintName + " on map");
			return null;
		}
		return sectorMapObjectEntity;
	}
}
