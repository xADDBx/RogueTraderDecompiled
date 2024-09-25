using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.BarkBanters;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers;

public class BarkBanterController : IControllerTick, IController, IBarkBanterPlayedHandler, ISubscriber, IControllerStop, IPartyCombatHandler, IDialogInteractionHandler, ICutsceneHandler, ISubscriber<CutscenePlayerData>
{
	private BarkBanterPlayer m_BarkBanterPlayer;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (m_BarkBanterPlayer != null)
		{
			m_BarkBanterPlayer.Tick();
			if (m_BarkBanterPlayer.Finished)
			{
				m_BarkBanterPlayer = null;
			}
		}
	}

	void IControllerStop.OnStop()
	{
		m_BarkBanterPlayer?.InterruptBark();
		m_BarkBanterPlayer = null;
	}

	public void HandleBarkBanter(BlueprintBarkBanter barkBanter)
	{
		if (m_BarkBanterPlayer == null)
		{
			m_BarkBanterPlayer = barkBanter.CreatePlayer();
			Game.Instance.Player.PlayedBanters.Add(barkBanter);
			PFLog.Default.Log($"Play bark banter {barkBanter}");
		}
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat)
		{
			m_BarkBanterPlayer?.InterruptBark();
			m_BarkBanterPlayer = null;
		}
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		m_BarkBanterPlayer?.InterruptBark();
		m_BarkBanterPlayer = null;
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
	}

	public void HandleCutsceneStarted(bool queued)
	{
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		if (cutscenePlayerData?.Cutscene != null && cutscenePlayerData.Cutscene.LockControl)
		{
			m_BarkBanterPlayer?.InterruptBark();
			m_BarkBanterPlayer = null;
		}
	}

	public void HandleCutsceneRestarted()
	{
	}

	public void HandleCutscenePaused(CutscenePauseReason reason)
	{
	}

	public void HandleCutsceneResumed()
	{
	}

	public void HandleCutsceneStopped()
	{
	}
}
