using Kingmaker.Controllers.StarSystem;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Inspect;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.Selection;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Controllers.Clicks.Handlers;

public sealed class ClickUnitHandler : IClickEventHandler
{
	public PointerMode GetMode()
	{
		return PointerMode.Default;
	}

	public HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		return new HandlerPriorityResult(GetPriorityInternal(gameObject, worldPosition));
	}

	private float GetPriorityInternal(GameObject gameObject, Vector3 worldPosition)
	{
		if (!gameObject)
		{
			return 0f;
		}
		if (TurnController.IsInTurnBasedCombat() && Game.Instance.TurnController.IsPreparationTurn)
		{
			return 0f;
		}
		AbstractUnitEntityView componentNonAlloc = gameObject.GetComponentNonAlloc<AbstractUnitEntityView>();
		if (componentNonAlloc.Or(null)?.Data == null)
		{
			return 0f;
		}
		if (!componentNonAlloc.EntityData.IsVisibleForPlayer)
		{
			return 0f;
		}
		if ((bool)componentNonAlloc.EntityData.Features.IsUntargetable)
		{
			return 0f;
		}
		UnitPartPersonalEnemy optional = componentNonAlloc.EntityData.GetOptional<UnitPartPersonalEnemy>();
		if (optional != null && !optional.IsCurrentlyTargetable)
		{
			return 0f;
		}
		if (componentNonAlloc.EntityData.LifeState.IsDead)
		{
			if (Game.Instance.Player.IsInCombat)
			{
				return 0f;
			}
			if (!componentNonAlloc.EntityData.IsDeadAndHasLoot && (!Game.Instance.Player.UISettings.ShowInspect || Game.Instance.Player.IsInCombat))
			{
				return 0f;
			}
			return 0.9f;
		}
		if (!Game.Instance.Player.IsInCombat)
		{
			return 1f;
		}
		if (componentNonAlloc.EntityData.IsDirectlyControllable)
		{
			return 2f;
		}
		if (componentNonAlloc.EntityData.IsEnemy(Game.Instance.Player.MainCharacterEntity))
		{
			return 2f;
		}
		return 1f;
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		SelectionManagerBase selectionManager = UIAccess.SelectionManager;
		AbstractUnitEntityView targetUnit = gameObject.GetComponent<AbstractUnitEntityView>();
		if (targetUnit == null)
		{
			return false;
		}
		if (button == 1 && InspectUnitsHelper.IsInspectAllow(targetUnit.EntityData))
		{
			EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
			{
				h.HandleUnitRightClick(targetUnit.Data);
			});
			return false;
		}
		if (TurnController.IsInTurnBasedCombat())
		{
			AbstractUnitEntity entityData = targetUnit.EntityData;
			if (entityData != null && entityData.IsDeadOrUnconscious)
			{
				Game.Instance.ClickEventsController.GetHandler<ClickGroundHandler>().OnClick(gameObject, worldPosition, button, simulate, muteEvents);
			}
		}
		if (PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingEntity(targetUnit.EntityData);
		}))
		{
			return true;
		}
		if (IsUnitControllable(targetUnit))
		{
			if (!TurnController.IsInTurnBasedCombat() && !Game.Instance.IsControllerGamepad)
			{
				HandleClickControllableUnit(targetUnit.Data);
			}
			return true;
		}
		if (targetUnit.EntityData is BaseUnitEntity item && Game.Instance.Player.Party.Contains(item))
		{
			return false;
		}
		return HandleClickUnit(selectionManager.GetNearestSelectedUnit(targetUnit.transform.position), targetUnit);
	}

	public static bool HandleClickUnit(BaseUnitEntity selectedUnit, AbstractUnitEntityView targetUnit)
	{
		if (PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingEntity(targetUnit.EntityData);
		}))
		{
			return true;
		}
		if (selectedUnit == null)
		{
			return false;
		}
		if (!selectedUnit.IsInCombat && targetUnit.EntityData.IsDeadAndHasLoot)
		{
			Vector3 vector = ((targetUnit is UnitEntityView unitEntityView && unitEntityView.EntityData.Inventory.IsLootDropped) ? targetUnit.Data.Position : targetUnit.ViewTransform.position);
			Vector3 vector2 = selectedUnit.Position - vector;
			Vector3 targetPoint = vector + vector2 / vector2.magnitude;
			PathfindingService.Instance.FindPathRT(selectedUnit.MovementAgent, targetPoint, targetUnit.MovementAgent.ApproachRadius, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else if (selectedUnit.IsMovementLockedByGameModeOrCombat())
				{
					PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
				}
				else
				{
					UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, targetPoint, targetUnit.MovementAgent.ApproachRadius)
					{
						IsSynchronized = true,
						CanBeAccelerated = true
					};
					if (Game.Instance.CurrentMode == GameModeType.StarSystem)
					{
						float num3 = (selectedUnit.Blueprint as BlueprintStarship)?.SpeedOnStarSystemMap ?? 1f;
						unitMoveToParams.OverrideSpeed = num3 / StarSystemTimeController.TimeMultiplier;
					}
					selectedUnit.Commands.Run(unitMoveToParams);
					selectedUnit.Commands.AddToQueue(new UnitLootUnitParams(targetUnit.EntityData, targetPoint)
					{
						IsSynchronized = true,
						CanBeAccelerated = true
					});
				}
			});
			return true;
		}
		IUnitInteraction interaction = targetUnit.EntityData.SelectClickInteraction(selectedUnit);
		if (!targetUnit.EntityData.IsInCombat && !selectedUnit.IsInCombat && interaction != null)
		{
			if (interaction.MainPlayerPreferred)
			{
				BaseUnitEntity mainCharacterEntity = Game.Instance.Player.MainCharacterEntity;
				if (UIAccess.SelectionManager.IsSelected(mainCharacterEntity) && mainCharacterEntity.IsDirectlyControllable && !mainCharacterEntity.Stealth.Active)
				{
					float num = mainCharacterEntity.DistanceTo(targetUnit.EntityData);
					float num2 = selectedUnit.DistanceTo(targetUnit.EntityData);
					if (num < 15f || num - num2 < 5f)
					{
						selectedUnit = mainCharacterEntity;
					}
				}
			}
			PathfindingService.Instance.FindPathRT(selectedUnit.MovementAgent, targetUnit.ViewTransform.position, interaction.Distance, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else if (selectedUnit.IsMovementLockedByGameModeOrCombat())
				{
					PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
				}
				else
				{
					UnitMoveToParams cmdParams = new UnitMoveToParams(path, targetUnit.ViewTransform.position, interaction.Distance)
					{
						IsSynchronized = true,
						CanBeAccelerated = true
					};
					selectedUnit.Commands.Run(cmdParams);
					selectedUnit.Commands.AddToQueue(new UnitInteractWithUnitParams(targetUnit.EntityData)
					{
						IsSynchronized = true,
						CanBeAccelerated = true
					});
				}
			});
			return true;
		}
		if (IsSuicideAttacker(selectedUnit) && selectedUnit.IsEnemy(targetUnit.EntityData) && TurnController.IsInTurnBasedCombat())
		{
			RunMoveCommandTB(selectedUnit, new MoveCommandSettings
			{
				Destination = Game.Instance.ClickEventsController.WorldPosition
			});
		}
		return false;
	}

	public static void HandleClickControllableUnit(MechanicEntity mechanicEntity, bool isDoubleClick = false)
	{
		if (Game.Instance.CurrentMode == GameModeType.Cutscene || (mechanicEntity != null && !mechanicEntity.IsPlayerFaction) || !(mechanicEntity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		if (Game.Instance.SelectedAbilityHandler?.Ability != null)
		{
			Game.Instance.SelectedAbilityHandler.OnClick(baseUnitEntity.View.gameObject, baseUnitEntity.View.transform.position, -1);
			return;
		}
		Game.Instance.SelectionCharacter.SetSelected(baseUnitEntity);
		if (isDoubleClick)
		{
			if (!UIUtility.IsGlobalMap() && baseUnitEntity.IsViewActive)
			{
				CameraRig.Instance.ScrollTo(baseUnitEntity.Position);
				if ((bool)SettingsRoot.Controls.CameraFollowsUnit)
				{
					Game.Instance.CameraController?.Follower?.Follow(baseUnitEntity);
				}
			}
		}
		else if (baseUnitEntity.IsViewActive)
		{
			Game.Instance.CameraController?.Follower?.ScrollTo(baseUnitEntity);
		}
	}

	private static bool IsSuicideAttacker(AbstractUnitEntity unit)
	{
		return unit.Facts.HasComponent<SuicideAttacker>((EntityFact x) => x.Components.Find((EntityFactComponent y) => y.SourceBlueprintComponent is SuicideAttacker) != null);
	}

	private static void RunMoveCommandTB(BaseUnitEntity unit, MoveCommandSettings settings)
	{
		if (Game.Instance.TurnController.TurnBasedModeActive && unit == Game.Instance.TurnController.CurrentUnit)
		{
			UnitMoveToProperParams unitMoveToProperParams = unit.TryCreateMoveCommandTB(settings, showMovePrediction: false);
			if (unitMoveToProperParams != null)
			{
				unit?.Commands.Run(unitMoveToProperParams);
			}
		}
	}

	private bool IsUnitControllable(AbstractUnitEntityView unit)
	{
		return unit.EntityData.IsDirectlyControllable;
	}
}
