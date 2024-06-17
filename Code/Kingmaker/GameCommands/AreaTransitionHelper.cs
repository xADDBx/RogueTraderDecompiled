using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.StarSystem;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.GameCommands;

public static class AreaTransitionHelper
{
	private const float ApproachRadiusToTransitionPoint = 1.5f;

	public static void StartAreaTransition([NotNull] MapObjectEntity mapObjectEntity)
	{
		if (Game.Instance.Player.IsInCombat || Game.Instance.CurrentMode == GameModeType.Dialog)
		{
			return;
		}
		AreaTransitionPart areaTransition = mapObjectEntity.GetOptional<AreaTransitionPart>();
		if (areaTransition != null)
		{
			Vector3 position = mapObjectEntity.Position;
			Game.Instance.GameCommandQueue.ClearAreaTransitionGroupDuplicates();
			UIAccess.SelectionManager.SelectAll();
			List<BaseUnitEntity> list = Game.Instance.SelectionCharacter.SelectedUnits.ToTempList();
			List<EntityRef<BaseUnitEntity>> unitRefs = ((IEnumerable<BaseUnitEntity>)list).Select((Func<BaseUnitEntity, EntityRef<BaseUnitEntity>>)((BaseUnitEntity u) => u)).ToList();
			Guid groupGuid = Guid.NewGuid();
			UnitCommandsRunner.MoveSelectedUnitsToPointRT(ObstacleAnalyzer.GetDeepNavmeshPoint(position), ClickGroundHandler.GetDirection(position, list), Game.Instance.IsControllerGamepad, preview: false, BlueprintRoot.Instance.Formations.MinSpaceFactor, list, delegate(BaseUnitEntity unit, MoveCommandSettings s)
			{
				RunUnitTransitionCommand(groupGuid, unitRefs, unit, areaTransition, s.Destination, s.SpeedLimit);
			});
		}
	}

	private static void RunUnitTransitionCommand(Guid groupGuid, List<EntityRef<BaseUnitEntity>> units, BaseUnitEntity unit, AreaTransitionPart transition, Vector3 position, float? speedLimit)
	{
		PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, position, 1.5f, 1, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else
			{
				UnitAreaTransitionParams cmdParams = new UnitAreaTransitionParams(groupGuid, units, position, transition)
				{
					SpeedLimit = speedLimit,
					IsSynchronized = true,
					CanBeAccelerated = true
				};
				if (path.vectorPath.Count > 0)
				{
					UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, position, 1.5f)
					{
						IsSynchronized = true,
						CanBeAccelerated = true
					};
					if (unit.Blueprint is BlueprintStarship { SpeedOnStarSystemMap: var speedOnStarSystemMap })
					{
						unitMoveToParams.OverrideSpeed = speedOnStarSystemMap / StarSystemTimeController.TimeMultiplier;
					}
					unit.Commands.Run(unitMoveToParams);
					unit.Commands.AddToQueue(cmdParams);
				}
				else
				{
					unit.Commands.Run(cmdParams);
				}
				if (Game.Instance.IsPaused)
				{
					UnitCommandsRunner.ShowDestination(unit, position);
				}
				EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
				{
					h.HandleAreaTransition();
				});
			}
		});
	}
}
