using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound.Base;
using Kingmaker.UI.InputSystems;
using UnityEngine;

namespace Kingmaker.Controllers.GlobalMap;

public class StarSystemMapMoveController : IControllerTick, IController
{
	private static bool m_StartedMovement;

	public static List<Vector3> Path = new List<Vector3>();

	public static List<Vector3> RemainingPath = new List<Vector3>();

	public static bool MovePlayerShip(Vector3 targetPos)
	{
		StarSystemShip starSystemShip = Game.Instance.StarSystemMapController.StarSystemShip;
		if (starSystemShip == null)
		{
			return false;
		}
		if (m_StartedMovement)
		{
			StopPlayerShipInternal();
		}
		Path = ShipPathHelper.FindPath(starSystemShip.Position, targetPos);
		starSystemShip.Agent.ForcePath(ForcedPath.Construct(Path));
		RemainingPath = starSystemShip.Agent.GetRemainingPath();
		m_StartedMovement = true;
		starSystemShip.StarSystemObjectLandOn = null;
		EscHotkeyManager.Instance.Subscribe(Game.Instance.GameCommandQueue.StopStarSystemStarShip);
		EventBus.RaiseEvent(delegate(IStarSystemShipMovementHandler h)
		{
			h.HandleStarSystemShipMovementStarted();
		});
		if (starSystemShip != null && BlueprintRoot.Instance.FxRoot.StarshipMoveSoundStart != null)
		{
			SoundEventsManager.PostEvent(BlueprintRoot.Instance.FxRoot.StarshipMoveSoundStart, starSystemShip.gameObject);
		}
		else
		{
			PFLog.TechArt.Warning("starship or sound not exist");
		}
		return true;
	}

	public static void StopPlayerShip()
	{
		if (m_StartedMovement || StarSystemMapClickObjectHandler.DestinationSso is AnomalyView)
		{
			Game.Instance.StarSystemMapController.StarSystemShip.Agent.Stop();
			StopPlayerShipInternal();
		}
	}

	public static bool CheckLanding()
	{
		StarSystemShip starSystemShip = Game.Instance.StarSystemMapController.StarSystemShip;
		starSystemShip.CheckLanding();
		if (starSystemShip.StarSystemObjectLandOn is AnomalyView anomalyView && anomalyView == StarSystemMapClickObjectHandler.DestinationSso)
		{
			anomalyView.InteractWithAnomaly();
			return true;
		}
		StarSystemObjectView currentPlanet = starSystemShip.StarSystemObjectLandOn;
		StarSystemObjectView destinationSso = StarSystemMapClickObjectHandler.DestinationSso;
		if (currentPlanet == destinationSso && currentPlanet != null)
		{
			EventBus.RaiseEvent(delegate(IStarShipLandingHandler h)
			{
				h.HandleStarShipLanded(currentPlanet);
			});
			return true;
		}
		return false;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		float gameDeltaTime = Game.Instance.TimeController.GameDeltaTime;
		StarSystemShip starSystemShip = Game.Instance.StarSystemMapController.StarSystemShip;
		if (m_StartedMovement && !starSystemShip.Agent.WantsToMove)
		{
			CheckLanding();
			StopPlayerShipInternal();
		}
		else if (m_StartedMovement)
		{
			starSystemShip.Agent.TickMovement(gameDeltaTime);
			starSystemShip.ApplyRotation();
			RemainingPath = starSystemShip.Agent.GetRemainingPath();
			starSystemShip.DrawPath(RemainingPath);
			Game.Instance.Player.LastPositionOnPreviousVisitedArea = starSystemShip.Position;
		}
	}

	private static void StopPlayerShipInternal()
	{
		EventBus.RaiseEvent(delegate(IStarSystemShipMovementHandler h)
		{
			h.HandleStarSystemShipMovementEnded();
		});
		Game.Instance.StarSystemMapController.StarSystemShip.UndrawPath();
		EscHotkeyManager.Instance.Unsubscribe(Game.Instance.GameCommandQueue.StopStarSystemStarShip);
		m_StartedMovement = false;
		RemainingPath.Clear();
		Path.Clear();
		Game.Instance.Player.LastPositionOnPreviousVisitedArea = Game.Instance.StarSystemMapController.StarSystemShip.Position;
		StarSystemShip starSystemShip = Game.Instance.StarSystemMapController.StarSystemShip;
		if (starSystemShip != null && BlueprintRoot.Instance.FxRoot.StarshipMoveSoundEnd != null)
		{
			SoundEventsManager.PostEvent(BlueprintRoot.Instance.FxRoot.StarshipMoveSoundEnd, starSystemShip.gameObject);
		}
		else
		{
			PFLog.TechArt.Warning("starship or sound not exist");
		}
	}
}
