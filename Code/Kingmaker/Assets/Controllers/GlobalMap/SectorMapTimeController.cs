using Kingmaker.Controllers;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Assets.Controllers.GlobalMap;

public class SectorMapTimeController : BaseGameTimeController, IControllerEnable, IController, IControllerDisable, ISectorMapWarpTravelHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, ISectorMapScanHandler
{
	private const float MaxPassageDurationInSec = 5f;

	private const float BaseTimeMultiplier = 28800f;

	private bool m_IsDisabled;

	private float TimeMultiplier => 28800f * AdditionalTimeMultiplier;

	private float WarpTravelTimeScale => TimeMultiplier * 100f / 5f;

	public void OnEnable()
	{
		m_IsDisabled = false;
		Game.Instance.TimeController.GameTimeScale = (Game.Instance.SectorMapController.IsScanning ? TimeMultiplier : 0f);
	}

	public void OnDisable()
	{
		Game.Instance.TimeController.GameTimeScale = 1f;
		m_IsDisabled = true;
	}

	public void HandleWarpTravelBeforeStart()
	{
		SaveState();
		SetState(GameTimeState.Normal);
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		Game.Instance.TimeController.GameTimeScale = WarpTravelTimeScale;
	}

	public void HandleWarpTravelStopped()
	{
		if (!m_IsDisabled)
		{
			Game.Instance.TimeController.GameTimeScale = 0f;
		}
		ResumeState();
	}

	public void HandleWarpTravelPaused()
	{
		if (!m_IsDisabled)
		{
			Game.Instance.TimeController.GameTimeScale = 0f;
		}
	}

	public void HandleWarpTravelResumed()
	{
		Game.Instance.TimeController.GameTimeScale = WarpTravelTimeScale;
	}

	public void HandleScanStarted(float range, float duration)
	{
		SaveState();
		SetState(GameTimeState.Normal);
		Game.Instance.TimeController.GameTimeScale = TimeMultiplier;
	}

	public void HandleSectorMapObjectScanned(SectorMapPassageView passageToStarSystem)
	{
	}

	public void HandleScanStopped()
	{
		ResumeState();
		Game.Instance.TimeController.GameTimeScale = 0f;
	}

	protected override void OnTimeStateChanged()
	{
	}
}
