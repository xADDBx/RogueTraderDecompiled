using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.StarSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Formations;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Controllers.Units;

public static class UnitCommandsRunner
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnitCommandsRunner");

	private static readonly List<BaseUnitEntity> UnitWaitAgentList = new List<BaseUnitEntity>();

	private static VirtualMoveCommand s_VirtualMoveCommand;

	private static IDisposable s_EscManagerHandle;

	private static bool s_MovePreview;

	public static bool HasWaitingAgents => UnitWaitAgentList.Count > 0;

	public static void ClearWaitingAgents(bool delayed = false)
	{
		if (delayed)
		{
			UIUtility.DoTaskLater(0.1f, delegate
			{
				UnitWaitAgentList.Clear();
			});
		}
		else
		{
			UnitWaitAgentList.Clear();
		}
	}

	public static void CancelMoveCommand()
	{
		Game.Instance.GameCommandQueue.ClearMovePrediction();
	}

	public static void CancelMoveCommandLocal()
	{
		UnitHelper.ClearPrediction();
		s_VirtualMoveCommand = null;
		TryUnsubscribeEscManager();
	}

	public static void SetVirtualMoveCommand([NotNull] BaseUnitEntity unit, [NotNull] UnitCommandParams cmdParams)
	{
		UISounds.Instance.Sounds.Combat.CombatGridSetWaypointClick.Play();
		s_VirtualMoveCommand = new VirtualMoveCommand(cmdParams, unit);
		SubscribeEscManager();
	}

	public static void DirectInteract(BaseUnitEntity unit, InteractionPart interaction)
	{
		unit.Commands.Run(UnitDirectInteract.CreateCommandParams(interaction));
	}

	public static void TryApproachAndInteract(BaseUnitEntity unit, InteractionPart interaction)
	{
		if (unit == null || !interaction.HasEnoughActionPoints(unit))
		{
			return;
		}
		if (!Game.Instance.TurnController.TurnBasedModeActive)
		{
			PathfindingService.Instance.FindPathRT(unit.MovementAgent, interaction.Owner.Position, interaction.ApproachRadius, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else
				{
					UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, interaction.Owner.Position, interaction.ApproachRadius)
					{
						IsSynchronized = true,
						CanBeAccelerated = true
					};
					if (unit.IsInPlayerParty && !unit.IsInCombat)
					{
						if ((interaction.Owner.Position - unit.Position).magnitude > (float)BlueprintRoot.Instance.MinSprintDistance)
						{
							unitMoveToParams.MovementType = WalkSpeedType.Sprint;
						}
						else if ((interaction.Owner.Position - unit.Position).magnitude < (float)BlueprintRoot.Instance.MaxWalkDistance)
						{
							unitMoveToParams.MovementType = WalkSpeedType.Walk;
						}
						else
						{
							unitMoveToParams.MovementType = WalkSpeedType.Run;
						}
					}
					if (Game.Instance.CurrentMode == GameModeType.StarSystem)
					{
						float num = (unit.Blueprint as BlueprintStarship)?.SpeedOnStarSystemMap ?? 1f;
						unitMoveToParams.OverrideSpeed = num / StarSystemTimeController.TimeMultiplier;
					}
					unit.Commands.Run(unitMoveToParams);
					unit.Commands.AddToQueueFirst(new UnitInteractWithObjectParams(interaction)
					{
						IsSynchronized = true,
						CanBeAccelerated = true
					});
				}
			});
		}
		else if (interaction.IsEnoughCloseForInteractionFromDesiredPosition(unit))
		{
			TryRunVirtualMoveCommand();
			unit.Commands.AddToQueue(new UnitInteractWithObjectParams(interaction)
			{
				IsSynchronized = true,
				CanBeAccelerated = true
			});
		}
		else
		{
			MoveSelectedUnitsToPoint(interaction.Owner.Position);
		}
	}

	public static void TryUnitUseAbility(AbilityData abilityData, TargetWrapper target, bool shouldApproach = false)
	{
		TryRunVirtualMoveCommand();
		UnitCommandParams cmd = CreateUseAbilityCommandParams(abilityData, target);
		if (cmd != null)
		{
			PartUnitCommands commands = abilityData.Caster.GetCommandsOptional();
			if (commands != null)
			{
				if (shouldApproach)
				{
					PathfindingService.Instance.FindPathRT(((BaseUnitEntity)abilityData.Caster).MovementAgent, target.Entity.Position, abilityData.RangeCells, delegate(ForcedPath path)
					{
						if (path.error)
						{
							PFLog.Pathfinding.Error("An error path was returned. Ignoring");
						}
						else
						{
							UnitMoveToParams cmdParams = new UnitMoveToParams(path, target.Entity.Position, abilityData.RangeCells)
							{
								IsSynchronized = true,
								CanBeAccelerated = true
							};
							commands.AddToQueue(cmdParams);
							commands.AddToQueue(cmd);
						}
					});
				}
				else
				{
					commands.AddToQueue(cmd);
				}
				return;
			}
		}
		PFLog.Default.ErrorWithReport($"{abilityData.Caster} can't execute cast command");
	}

	public static void MoveSelectedUnitsToPoint(Vector3 worldPosition)
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			MoveSelectedUnitToPointTB(worldPosition);
		}
		else
		{
			MoveSelectedUnitsToPointRT(worldPosition, ClickGroundHandler.GetDefaultDirection(worldPosition), isControllerGamepad: false);
		}
	}

	private static void MoveSelectedUnitToPointTB(Vector3 worldPosition)
	{
		CustomGridNodeBase customGridNodeBase = UnitPathManager.Instance?.CurrentNode ?? worldPosition.GetNearestNodeXZUnwalkable();
		if (customGridNodeBase == null || !customGridNodeBase.Walkable)
		{
			Logger.Log("Cant move to coord {0}, node {1}", worldPosition, customGridNodeBase?.ToString() ?? "<null>");
			return;
		}
		if (!Game.Instance.TurnController.TurnBasedModeActive)
		{
			throw new InvalidOperationException("Expecting to be in TBM mode here");
		}
		if (Game.Instance.SelectionCharacter.SelectedUnits.Count != 1)
		{
			throw new InvalidOperationException(string.Format("Expecting only one selected unit, got #{0}: {1}", Game.Instance.SelectionCharacter.SelectedUnits.Count, string.Join(", ", Game.Instance.SelectionCharacter.SelectedUnits.Select((BaseUnitEntity v) => v.ToString()))));
		}
		BaseUnitEntity baseUnitEntity = Game.Instance.SelectionCharacter.SelectedUnits.Single();
		if (baseUnitEntity != Game.Instance.TurnController.CurrentUnit)
		{
			throw new InvalidOperationException($"Cant move unit {baseUnitEntity} on {Game.Instance.TurnController.CurrentUnit}'s turn");
		}
		if (!baseUnitEntity.Commands.Empty)
		{
			Logger.Log("Unit {0} has active command {1}, cant move.", baseUnitEntity, baseUnitEntity.Commands.Current);
			return;
		}
		if (!baseUnitEntity.IsStarship() && !WarhammerBlockManager.Instance.CanUnitStandOnNode(baseUnitEntity, customGridNodeBase))
		{
			Logger.Log("Unit {0} can't stand on the node {1}", baseUnitEntity, customGridNodeBase);
			return;
		}
		baseUnitEntity.TryCreateMoveCommandTB(new MoveCommandSettings
		{
			Destination = customGridNodeBase.Vector3Position
		}, showMovePrediction: true, out var status);
		switch (status)
		{
		case UnitHelper.MoveCommandStatus.SamePath:
			UISounds.Instance.Sounds.Combat.CombatGridConfirmActionClick.Play();
			TryRunVirtualMoveCommand();
			break;
		case UnitHelper.MoveCommandStatus.NotEnoughPoints:
			Logger.Log("Move command: Not enough points to move");
			break;
		case UnitHelper.MoveCommandStatus.NoReachableTile:
			Logger.Log("Move command: No reachable tiles available");
			break;
		case UnitHelper.MoveCommandStatus.NoForcedPath:
			Logger.Log("Move command: Failed to create Forced path");
			break;
		case UnitHelper.MoveCommandStatus.NoStartingCell:
			Logger.Log("Move command: No starting cell in Forced path available");
			break;
		case UnitHelper.MoveCommandStatus.CannotMove:
			Logger.Log("Move command: Cannot move");
			break;
		default:
			Logger.Warning("Unknown move command status occured: {0}", status);
			break;
		case UnitHelper.MoveCommandStatus.NewCommandCreated:
			break;
		}
	}

	public static void MoveSelectedUnitsToPointRT(Vector3 worldPosition, Vector3 direction, bool isControllerGamepad, bool preview = false, float formationSpaceFactor = 1f, List<BaseUnitEntity> selectedUnits = null, Action<BaseUnitEntity, MoveCommandSettings> commandRunner = null)
	{
		MoveSelectedUnitsToPointRT(Game.Instance.SelectionCharacter.SingleSelectedUnit.Value, worldPosition, direction, isControllerGamepad, preview, formationSpaceFactor, selectedUnits, commandRunner);
	}

	public static void MoveSelectedUnitsToPointRT(BaseUnitEntity mainUnit, Vector3 worldPosition, Vector3 direction, bool isControllerGamepad, bool preview = false, float formationSpaceFactor = 1f, List<BaseUnitEntity> selectedUnits = null, Action<BaseUnitEntity, MoveCommandSettings> commandRunner = null, List<BaseUnitEntity> allUnits = null)
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			throw new InvalidOperationException("Not expecting to be in TBM mode here");
		}
		s_MovePreview = preview;
		if (!preview)
		{
			UnitWaitAgentList.Clear();
		}
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem)
		{
			MoveSelectedUnitsToPointInSpace(worldPosition, direction, isControllerGamepad, preview, commandRunner);
			return;
		}
		bool flag = mainUnit?.View?.AgentOverride != null;
		IPartyFormation currentFormation = Game.Instance.Player.FormationManager.CurrentFormation;
		if (selectedUnits == null)
		{
			selectedUnits = Game.Instance.SelectionCharacter.SelectedUnits.Except((BaseUnitEntity u) => u.MovementAgent.IsTraverseInProgress).ToList();
		}
		if (allUnits == null)
		{
			allUnits = ((selectedUnits.Count == 1) ? selectedUnits : Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity c) => c.IsDirectlyControllable()).ToList());
		}
		float? num = null;
		if (selectedUnits.Count > 1)
		{
			num = selectedUnits.Select((BaseUnitEntity u) => u.Movable.ModifiedSpeedMps).Min();
			float num2 = 30.Feet().Meters / 2.5f;
			if (num < num2)
			{
				num = num2;
			}
		}
		if (selectedUnits.Count <= 0)
		{
			return;
		}
		Span<Vector3> resultPositions = stackalloc Vector3[allUnits.Count];
		PartyFormationHelper.FillFormationPositions(worldPosition, FormationAnchor.Front, direction, allUnits, selectedUnits, currentFormation, resultPositions, formationSpaceFactor);
		AbstractUnitEntity abstractUnitEntity = currentFormation.Tank ?? selectedUnits.FirstItem();
		bool flag2 = selectedUnits.Contains(abstractUnitEntity);
		float num3 = currentFormation.Length + 0.6f;
		float value = ((!num.HasValue) ? abstractUnitEntity.Movable.ModifiedSpeedMps : Math.Min(num.Value, abstractUnitEntity.Movable.ModifiedSpeedMps));
		for (int i = 0; i < allUnits.Count; i++)
		{
			if (isControllerGamepad && allUnits[i] == mainUnit && flag)
			{
				commandRunner?.Invoke(allUnits[i], new MoveCommandSettings
				{
					Destination = allUnits[i].Position,
					SpeedLimit = num
				});
			}
			else
			{
				if (!selectedUnits.HasItem(allUnits[i]))
				{
					continue;
				}
				BaseUnitEntity baseUnitEntity = allUnits[i];
				Vector3 mechanicsPosition = resultPositions[i];
				mechanicsPosition = SizePathfindingHelper.FromViewToMechanicsPosition(baseUnitEntity, mechanicsPosition);
				if (preview)
				{
					ShowDestination(allUnits[i], mechanicsPosition);
					continue;
				}
				float? num4 = null;
				if (Game.Instance.Player.FormationManager.GetPreserveFormation() && Game.Instance.Player.IsInCombat && flag2 && (abstractUnitEntity.Position - baseUnitEntity.Position).sqrMagnitude <= num3)
				{
					num4 = value;
				}
				(commandRunner ?? new Action<BaseUnitEntity, MoveCommandSettings>(RunMoveCommandRT))(baseUnitEntity, new MoveCommandSettings
				{
					Destination = mechanicsPosition,
					SpeedLimit = (num4 ?? num),
					FollowedUnit = mainUnit,
					IsControllerGamepad = isControllerGamepad
				});
			}
		}
		float num5 = 0f;
		for (int j = 0; j < allUnits.Count; j++)
		{
			if (selectedUnits.HasItem(allUnits[j]))
			{
				float magnitude = (worldPosition - resultPositions[j]).To2D().magnitude;
				if (magnitude > num5)
				{
					num5 = magnitude;
				}
			}
		}
		foreach (BaseUnitEntity selectedUnit in selectedUnits)
		{
			if (allUnits.HasItem(selectedUnit))
			{
				continue;
			}
			if (isControllerGamepad && selectedUnit == mainUnit && flag)
			{
				commandRunner?.Invoke(selectedUnit, new MoveCommandSettings
				{
					Destination = selectedUnit.Position,
					SpeedLimit = num
				});
				continue;
			}
			Vector3 vector = ((selectedUnits.Count == 1) ? worldPosition : GeometryUtils.ProjectToGround(worldPosition - direction.normalized * (num5 + 2f)));
			if (preview)
			{
				ShowDestination(selectedUnit, vector);
				continue;
			}
			(commandRunner ?? new Action<BaseUnitEntity, MoveCommandSettings>(RunMoveCommandRT))(selectedUnit, new MoveCommandSettings
			{
				Destination = vector,
				SpeedLimit = num
			});
		}
		if (preview)
		{
			ClickPointerManager.Instance?.ShowPreviewArrow(worldPosition, direction);
		}
		else
		{
			ClickPointerManager.Instance?.CancelPreview();
		}
		EventBus.RaiseEvent(delegate(IClickActionHandler h)
		{
			h.OnMoveRequested(worldPosition);
		});
	}

	private static void MoveSelectedUnitsToPointInSpace(Vector3 worldPosition, Vector3 direction, bool isControllerGamepad, bool preview = false, Action<BaseUnitEntity, MoveCommandSettings> commandRunner = null)
	{
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SingleSelectedUnit.Value;
		bool flag = value?.View?.AgentOverride != null;
		Span<Vector3> span = stackalloc Vector3[1];
		if (isControllerGamepad)
		{
			if (flag)
			{
				commandRunner?.Invoke(value, new MoveCommandSettings
				{
					Destination = value.Position
				});
			}
		}
		else
		{
			Vector3 mechanicsPosition = span[0];
			mechanicsPosition = SizePathfindingHelper.FromViewToMechanicsPosition(value, mechanicsPosition);
			if (preview)
			{
				ShowDestination(value, mechanicsPosition);
			}
			else
			{
				(commandRunner ?? new Action<BaseUnitEntity, MoveCommandSettings>(RunMoveCommandRT))(value, new MoveCommandSettings
				{
					Destination = mechanicsPosition
				});
			}
		}
		if (preview)
		{
			ClickPointerManager.Instance?.ShowPreviewArrow(worldPosition, direction);
		}
		else
		{
			ClickPointerManager.Instance?.CancelPreview();
		}
		EventBus.RaiseEvent(delegate(IClickActionHandler h)
		{
			h.OnMoveRequested(worldPosition);
		});
	}

	public static void ShowDestination(BaseUnitEntity unit, Vector3 point)
	{
		if (unit.GetSaddledUnit() != null || !unit.View.MovementAgent || UnitWaitAgentList.HasItem(unit))
		{
			return;
		}
		UnitWaitAgentList.Add(unit);
		PathfindingService.Instance.FindPathRT(unit.MovementAgent, point, 0.3f, delegate(ForcedPath p)
		{
			if (p.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				return;
			}
			using (PathDisposable<ForcedPath>.Get(p, unit))
			{
				UnitWaitAgentList.Remove(unit);
				if (p.vectorPath.Count > 0)
				{
					List<Vector3> vectorPath = p.vectorPath;
					Vector3 pathDestination = vectorPath[vectorPath.Count - 1];
					unit.View.OnMovementStarted(pathDestination, s_MovePreview);
				}
			}
		});
	}

	public static void ShowDestination(BaseUnitEntity unit, Path path)
	{
		if (unit.GetSaddledUnit() == null && path.vectorPath.Count > 0)
		{
			List<Vector3> vectorPath = path.vectorPath;
			Vector3 pathDestination = vectorPath[vectorPath.Count - 1];
			unit.View.OnMovementStarted(pathDestination, s_MovePreview);
		}
	}

	private static UnitCommandParams CreateUseAbilityCommandParams(AbilityData abilityData, TargetWrapper target)
	{
		PlayerUseAbilityParams result = new PlayerUseAbilityParams(abilityData, target)
		{
			IsSynchronized = true,
			CanBeAccelerated = true
		};
		if (abilityData.SourceItem != null)
		{
			EventBus.RaiseEvent(delegate(IClickActionHandler h)
			{
				h.OnItemUseRequested(abilityData, target);
			});
			return result;
		}
		EventBus.RaiseEvent(delegate(IClickActionHandler h)
		{
			h.OnCastRequested(abilityData, target);
		});
		return result;
	}

	private static void RunMoveCommand(BaseUnitEntity unit, UnitCommandParams cmdParams)
	{
		unit.Commands.Run(cmdParams);
		if (cmdParams.ForcedPath != null && cmdParams.ForcedPath.vectorPath.Count > 0)
		{
			UnitEntityView view = unit.View;
			List<Vector3> vectorPath = cmdParams.ForcedPath.vectorPath;
			view.TryShowPointer(vectorPath[vectorPath.Count - 1]);
		}
		else if (cmdParams.Target != null)
		{
			unit.View.TryShowPointer(cmdParams.Target.Point);
		}
		if (unit.Commands.Queue.OfType<UnitCommandParams>().FirstOrDefault() == cmdParams || Game.Instance.IsPaused)
		{
			ShowDestination(unit, cmdParams.Target.Point);
		}
	}

	private static void RunMoveCommandRT(BaseUnitEntity unit, MoveCommandSettings settings)
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			throw new InvalidOperationException("Should not be here in TBM");
		}
		if (settings.IsControllerGamepad && unit != settings.FollowedUnit)
		{
			UnitCommandParams cmdParams = UnitHelper.CreateUnitFollowCommandParamsRT(unit, settings);
			RunMoveCommand(unit, cmdParams);
			return;
		}
		unit.View.TryShowPointer(settings.Destination);
		PathfindingService.Instance.FindPathRT(unit.MovementAgent, settings.Destination, 0.3f, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else
			{
				UnitCommandParams unitCommandParams = UnitHelper.CreateMoveCommandParamsRT(unit, settings, path);
				if (unitCommandParams != null)
				{
					RunMoveCommand(unit, unitCommandParams);
				}
			}
		});
	}

	private static void TryRunVirtualMoveCommand()
	{
		if (s_VirtualMoveCommand != null)
		{
			TryUnsubscribeEscManager();
			UnitHelper.ClearPrediction(s_VirtualMoveCommand.Unit.ToBaseUnitEntity());
			AbstractUnitCommand cmd = s_VirtualMoveCommand.CmdHandle?.Cmd;
			UnitReference unit = s_VirtualMoveCommand.Unit;
			s_VirtualMoveCommand.RunCommand();
			s_VirtualMoveCommand = null;
			EventBus.RaiseEvent(delegate(IRunVirtualMoveCommandHandler h)
			{
				h.HandleRunVirtualMoveCommand(cmd, unit);
			});
		}
	}

	private static void SubscribeEscManager()
	{
		TryUnsubscribeEscManager();
		s_EscManagerHandle = EscHotkeyManager.Instance.Subscribe(CancelMoveCommand);
	}

	private static void TryUnsubscribeEscManager()
	{
		s_EscManagerHandle?.Dispose();
	}
}
