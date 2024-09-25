using System.Linq;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Networking;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class StarSystemMapClickObjectHandler : IClickEventHandler
{
	public static StarSystemObjectView DestinationSso;

	public PointerMode GetMode()
	{
		return PointerMode.Default;
	}

	public virtual HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		return new HandlerPriorityResult(GetPriorityInternal(gameObject, worldPosition));
	}

	private float GetPriorityInternal(GameObject gameObject, Vector3 worldPosition)
	{
		if (!gameObject)
		{
			return 0f;
		}
		if (!IsAnomaly(gameObject))
		{
			if (!GetStarSystemObject(gameObject))
			{
				return 1f;
			}
			return 2f;
		}
		return 3f;
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		float num = Mathf.Clamp(worldPosition.z, -50f, 40f);
		float num2 = (num + 50f) / 90f;
		float x = Mathf.Clamp(worldPosition.x, 0f - (80f - num2 * 17f), 80f - num2 * 17f);
		Vector3 clampedWorldPosition = new Vector3(x, worldPosition.y, num);
		if (PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingPosition(clampedWorldPosition);
		}))
		{
			return false;
		}
		if (!UINetUtility.IsControlMainCharacter())
		{
			return false;
		}
		StarSystemObjectView starSystemObject = GetStarSystemObject(gameObject);
		StarSystemObjectEntity starSystemObjectEntity = ((starSystemObject != null) ? starSystemObject.Data : null);
		Game.Instance.GameCommandQueue.InteractWithStarSystemObjectGameCommand(starSystemObjectEntity, clampedWorldPosition);
		return true;
	}

	public static void MovePlayerShip(StarSystemObjectView starSystemObject)
	{
		DestinationSso = starSystemObject;
		StarSystemMapMoveController.MovePlayerShip(ShipPathHelper.LandingPoint(DestinationSso));
	}

	private static StarSystemObjectView GetStarSystemObject(GameObject gameObject)
	{
		return gameObject.GetComponentNonAlloc<StarSystemObjectView>();
	}

	private static bool IsAnomaly(GameObject gameObject)
	{
		return gameObject.GetComponentNonAlloc<AnomalyView>() != null;
	}

	public static void MovePlayerShipAndInteract(AnomalyEntityData anomaly)
	{
		if (anomaly.InteractTime == BlueprintAnomaly.AnomalyInteractTime.ByClick)
		{
			StarSystemMapMoveController.StopPlayerShip();
			anomaly.Interact();
			DestinationSso = null;
		}
		else
		{
			StarSystemMapMoveController.MovePlayerShip(anomaly.Position);
			Game.Instance.StarSystemMapController.LandOnAnomaly(anomaly);
		}
	}

	public static void HandlePlayerShipMove(StarSystemObjectView sso)
	{
		if (!(sso == null))
		{
			AnomalyEntityData anomalyEntityData = Game.Instance.State.StarSystemObjects.OfType<AnomalyEntityData>().FirstOrDefault((AnomalyEntityData anom) => anom.View.BlockedObject == sso);
			if (anomalyEntityData != null && anomalyEntityData.CanInteract())
			{
				DestinationSso = sso;
				MovePlayerShipAndInteract(anomalyEntityData);
			}
			else
			{
				MovePlayerShip(sso);
			}
		}
	}
}
