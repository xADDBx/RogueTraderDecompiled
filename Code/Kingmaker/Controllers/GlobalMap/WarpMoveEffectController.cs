using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Globalmap;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Controllers.GlobalMap;

public class WarpMoveEffectController : IController, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, IAreaHandler
{
	private GameObject m_WarpMoveEffectObj;

	private WarpMoveEffect m_WarpMoveEffect;

	public void HandleWarpTravelBeforeStart()
	{
		EffectStart();
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		m_WarpMoveEffect.WarpTravelStarted();
	}

	public void HandleWarpTravelStopped()
	{
		EffectStop();
	}

	public void HandleWarpTravelPaused()
	{
		m_WarpMoveEffect.WarpTravelPaused();
	}

	public void HandleWarpTravelResumed()
	{
		m_WarpMoveEffect.WarpTravelResumed();
	}

	public void OnAreaBeginUnloading()
	{
		EffectStop();
	}

	public void OnAreaDidLoad()
	{
	}

	private void EffectStart()
	{
		if (BlueprintRoot.Instance.FxRoot.WarpMoveEffect != null)
		{
			Transform transform = CameraRig.Instance.transform;
			m_WarpMoveEffectObj = Object.Instantiate(BlueprintRoot.Instance.FxRoot.WarpMoveEffect.Load(), transform.position, transform.rotation, FxHelper.FxRoot);
			m_WarpMoveEffect = m_WarpMoveEffectObj.GetComponent<WarpMoveEffect>();
		}
		m_WarpMoveEffect.WarpTravelBeforeStart();
		if (m_WarpMoveEffectObj != null && BlueprintRoot.Instance.FxRoot.WarpSoundBeforeStart != null)
		{
			SoundEventsManager.PostEvent(BlueprintRoot.Instance.FxRoot.WarpSoundBeforeStart, m_WarpMoveEffectObj);
		}
		else
		{
			PFLog.TechArt.Warning("m_WarpMoveEffectObj or WarpSoundBeforeStart");
		}
	}

	private void EffectStop()
	{
		m_WarpMoveEffect.WarpTravelStopped();
		if (m_WarpMoveEffectObj != null && BlueprintRoot.Instance.FxRoot.WarpSoundEnd != null)
		{
			SoundEventsManager.PostEvent(BlueprintRoot.Instance.FxRoot.WarpSoundEnd, m_WarpMoveEffectObj);
		}
		else
		{
			PFLog.TechArt.Warning("m_WarpMoveEffectObj or WarpSoundEnd");
		}
	}
}
