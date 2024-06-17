using Kingmaker.Blueprints.Camera;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Rest;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.View;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Controllers.GlobalMap;

public class SectorMapCameraController : CameraController, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, IAreaHandler, IControllerStop, IController, IAdditiveAreaSwitchHandler
{
	private Coroutine m_ScrollToCoroutine;

	private CameraFlyAnimationParams CameraSpeed => UIConfig.Instance.GlobalMapWarpTravelCameraSpeed;

	private Vector3 NewPosition => Game.Instance.SectorMapTravelController.To.Position;

	private Vector3 OldPosition => Game.Instance.SectorMapTravelController.From.Position;

	public SectorMapCameraController()
		: base(allowScroll: true, allowZoom: true, clamp: true, rotate: false)
	{
	}

	public void HandleWarpTravelBeforeStart()
	{
		CameraRig.Instance.LockCamera();
	}

	public override void Tick()
	{
		CameraRig instance = CameraRig.Instance;
		if (!(instance == null) && !instance.FixCamera)
		{
			Follower.TryFollow();
			instance.TickScroll();
			instance.CameraZoom.TickZoom();
			instance.TickShake();
		}
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		if (IsPointOnScreen(OldPosition) && IsPointOnScreen(NewPosition))
		{
			return;
		}
		if (CameraSpeed == null)
		{
			CameraRig.Instance.ScrollTo(NewPosition);
			return;
		}
		float maxSpeed = (CameraSpeed.AutoSpeed ? float.MaxValue : CameraSpeed.MaxSpeed);
		float speed = (CameraSpeed.AutoSpeed ? 0f : CameraSpeed.Speed);
		CameraRig.Instance.ScrollTo(OldPosition);
		DelayedInvoker.InvokeInTime(delegate
		{
			CameraRig.Instance.ScrollToTimed(NewPosition, CameraSpeed.MaxTime, maxSpeed, speed, CameraSpeed.AnimationCurve, useUnscaledTime: true);
		}, 1f);
	}

	public void HandleWarpTravelStopped()
	{
		CameraRig.Instance.UnLockCamera();
	}

	public void HandleWarpTravelPaused()
	{
	}

	public void HandleWarpTravelResumed()
	{
	}

	public override void OnEnable()
	{
		base.OnEnable();
		CameraRig.Instance.SavedPosition = null;
	}

	private bool IsPointOnScreen(Vector3 point)
	{
		return UIUtilityGetRect.CheckObjectInRect(point, 35f, 35f);
	}

	public void OnAreaBeginUnloading()
	{
		CameraRig.Instance.UnLockCamera();
	}

	void IControllerStop.OnStop()
	{
		CameraRig.Instance.UnLockCamera();
	}

	public void OnAreaDidLoad()
	{
		SetupCamera();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		CameraRig.Instance.UnLockCamera();
	}

	public void OnAdditiveAreaDidActivated()
	{
		SetupCamera();
	}

	private void SetupCamera()
	{
		if (!(Game.Instance.CurrentMode != GameModeType.GlobalMap))
		{
			CameraRig instance = CameraRig.Instance;
			instance.SetAttachPoint(instance.CameraGlobalMapAttachPoint);
			if (Game.Instance.SectorMapTravelController.IsTravelling)
			{
				instance.ScrollToImmediately(NewPosition);
				instance.LockCamera();
			}
			else
			{
				instance.ScrollToImmediately(Game.Instance.SectorMapController.GetCurrentStarSystem().Data.Position);
			}
		}
	}
}
