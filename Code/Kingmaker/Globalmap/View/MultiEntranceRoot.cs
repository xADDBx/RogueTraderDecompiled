using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.View;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.Globalmap.View;

public class MultiEntranceRoot : MonoBehaviour, IMultiEntranceHandler, ISubscriber, ITeleportHandler, IAreaHandler
{
	[SerializeField]
	private BlueprintMultiEntrance.Reference m_Entrance;

	private Vector3 m_OldCameraPos;

	private bool m_LightWasActive;

	private bool _needReposition;

	public BlueprintMultiEntrance Entrance => m_Entrance?.Get();

	public void HandleMultiEntrance(BlueprintMultiEntrance multiEntrance)
	{
		if (multiEntrance == Entrance)
		{
			CameraRig instance = CameraRig.Instance;
			m_OldCameraPos = instance.transform.position;
			instance.ScrollToImmediately(base.transform.position);
			m_LightWasActive = instance.MapLight.activeSelf;
			instance.MapLight.SetActive(value: false);
			instance.NoClamp = true;
			instance.Camera.transform.position = base.transform.position;
			instance.Camera.transform.rotation = base.transform.rotation;
			Game.Instance.MatchTimeOfDay();
			EventBus.RaiseEvent(delegate(IUIMultiEntranceHandler h)
			{
				h.HandleMultiEntranceUI(Entrance);
			});
			if (!UIUtility.IsGlobalMap() && FogOfWarArea.Active != null)
			{
				FogOfWarArea.Active.enabled = false;
			}
			EscHotkeyManager.Instance.Subscribe(BackToMap);
		}
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void BackToMap()
	{
		EventBus.RaiseEvent(delegate(IUIMultiEntranceHandler h)
		{
			h.HandleMultiEntranceUI(null);
		});
		EscHotkeyManager.Instance.Unsubscribe(BackToMap);
		CameraRig instance = CameraRig.Instance;
		instance.ScrollToImmediately(m_OldCameraPos);
		instance.MapLight.SetActive(m_LightWasActive);
		instance.NoClamp = false;
		if (FogOfWarArea.Active != null)
		{
			FogOfWarArea.Active.enabled = base.transform;
		}
		Game.Instance.MatchTimeOfDay();
	}

	public void HandlePartyTeleport(AreaEnterPoint enterPoint)
	{
		if (!LoadingProcess.Instance.IsLoadingInProcess)
		{
			m_OldCameraPos = CameraRig.Instance.transform.position;
			BackToMap();
		}
		else
		{
			_needReposition = true;
		}
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		_needReposition = false;
	}

	void IAreaHandler.OnAreaDidLoad()
	{
		if (_needReposition)
		{
			_needReposition = false;
			m_OldCameraPos = CameraRig.Instance.transform.position;
			BackToMap();
		}
	}
}
