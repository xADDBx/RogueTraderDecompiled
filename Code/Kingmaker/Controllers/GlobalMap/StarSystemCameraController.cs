using Kingmaker.Controllers.Rest;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Controllers.GlobalMap;

public class StarSystemCameraController : CameraController, IAreaHandler, ISubscriber, IAdditiveAreaSwitchHandler
{
	public StarSystemCameraController()
		: base(allowScroll: false, allowZoom: true, clamp: false, rotate: false)
	{
	}

	public override void OnEnable()
	{
		base.OnEnable();
		CameraRig.Instance.SavedPosition = null;
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		SetupCamera();
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
	}

	public void OnAdditiveAreaDidActivated()
	{
		SetupCamera();
	}

	private static void SetupCamera()
	{
		if (!(Game.Instance.CurrentlyLoadedArea.AreaStatGameMode != GameModeType.StarSystem))
		{
			CameraRig instance = CameraRig.Instance;
			if (!instance.CameraStarSystemAttachPoint)
			{
				UberDebug.LogError("No attach point for Solar System Camera");
				return;
			}
			instance.SetAttachPoint(instance.CameraStarSystemAttachPoint);
			instance.ScrollToImmediately(new Vector3(-4f, 0f, 8f));
		}
	}
}
