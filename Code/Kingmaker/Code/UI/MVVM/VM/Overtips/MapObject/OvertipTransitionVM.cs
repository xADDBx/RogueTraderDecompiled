using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.StarSystem;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;

public class OvertipTransitionVM : BaseOvertipMapObjectVM
{
	public readonly ReactiveProperty<string> Title = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> HasCharactersMovingToHere = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsVisibleForPlayer = new ReactiveProperty<bool>(initialValue: false);

	public readonly bool EnableInCombat;

	protected override bool UpdateEnabled => MapObjectEntity?.IsInGame ?? false;

	protected override Vector3 GetEntityPosition()
	{
		return MapObjectEntity.Position;
	}

	public OvertipTransitionVM(MapObjectEntity mapObjectData)
		: base(mapObjectData)
	{
		EnableInCombat = mapObjectData.View.GetComponent<AreaTransition>()?.Settings.EnableInCombat ?? true;
		Title.Value = mapObjectData.View.GetComponent<AreaTransition>()?.Settings?.AreaEnterPoint?.TooltipDescription;
	}

	protected override void OnUpdateHandler()
	{
		IsVisibleForPlayer.Value = MapObjectEntity?.IsInGame ?? false;
		HasCharactersMovingToHere.Value = IsCharactersMove();
		base.OnUpdateHandler();
	}

	private bool IsCharactersMove()
	{
		if (!UpdateEnabled)
		{
			return false;
		}
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			UnitAreaTransition current = partyAndPet.Commands.GetCurrent<UnitAreaTransition>();
			if (current != null && current.TransitionPart.Owner == MapObjectEntity)
			{
				return true;
			}
		}
		return false;
	}

	public void OnClick()
	{
		if (LoadingProcess.Instance.IsLoadingInProcess)
		{
			return;
		}
		if (Game.Instance.CurrentMode != GameModeType.StarSystem)
		{
			StartAreaTransition();
		}
		else
		{
			if (!(Game.Instance.CurrentMode == GameModeType.StarSystem))
			{
				return;
			}
			StarSystemObjectView starSystemObjectLandOn = Game.Instance.StarSystemMapController.StarSystemShip.StarSystemObjectLandOn;
			MapObjectView view = MapObjectEntity.View;
			if (starSystemObjectLandOn == view)
			{
				EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
				{
					h.HandleCall(delegate
					{
						Game.Instance.GetController<StarSystemTimeController>()?.SetState(GameTimeState.Normal);
						Game.Instance.Player.FixPartyAfterChange();
						StartAreaTransition();
					}, delegate
					{
					}, Game.Instance.LoadedAreaState.Settings.CapitalPartyMode);
				});
			}
			else
			{
				StartSpaceAreaTransition();
			}
		}
	}

	private void StartAreaTransition()
	{
		if (!Game.Instance.Player.IsInCombat && !(Game.Instance.CurrentMode == GameModeType.Dialog) && MapObjectEntity.GetOptional<AreaTransitionPart>() != null && UINetUtility.IsControlMainCharacterWithWarning())
		{
			Kingmaker.GameCommands.AreaTransitionHelper.StartAreaTransition(MapObjectEntity);
		}
	}

	private void StartSpaceAreaTransition()
	{
		if (Game.Instance.Player.IsInCombat || Game.Instance.CurrentMode == GameModeType.Dialog)
		{
			return;
		}
		AreaTransitionPart optional = MapObjectEntity.GetOptional<AreaTransitionPart>();
		if (optional == null)
		{
			return;
		}
		UIAccess.SelectionManager.SelectAll();
		Vector3 deepNavmeshPoint = ObstacleAnalyzer.GetDeepNavmeshPoint(optional.View.ViewTransform.position);
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			Game.Instance.GameCommandQueue.MoveShip(MapObjectEntity, MoveShipGameCommand.VisitType.MovePlayerShip);
			return;
		}
		UnitCommandsRunner.MoveSelectedUnitsToPointRT(deepNavmeshPoint, ClickGroundHandler.GetDefaultDirection(optional.View.ViewTransform.position), Game.Instance.IsControllerGamepad, preview: false, BlueprintRoot.Instance.Formations.MinSpaceFactor, null, delegate(BaseUnitEntity unit, MoveCommandSettings s)
		{
			RunSpaceUnitTransitionCommand(unit, s.Destination);
		});
	}

	private static void RunSpaceUnitTransitionCommand(BaseUnitEntity unit, Vector3 position)
	{
		PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, position, 2f, 1, delegate(ForcedPath path)
		{
			if (path.error)
			{
				PFLog.Pathfinding.Error("An error path was returned. Ignoring");
			}
			else if (unit.IsMovementLockedByGameModeOrCombat())
			{
				PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
			}
			else
			{
				UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, position, 2f)
				{
					IsSynchronized = true
				};
				if (unit.Blueprint is BlueprintStarship { SpeedOnStarSystemMap: var speedOnStarSystemMap })
				{
					unitMoveToParams.OverrideSpeed = speedOnStarSystemMap / StarSystemTimeController.TimeMultiplier;
				}
				unit.Commands.Run(unitMoveToParams);
			}
		});
	}

	public void HighlightChanged()
	{
		MapObjectIsHighlited.Value = MapObjectEntity?.View.AreaHighlighted ?? false;
	}
}
