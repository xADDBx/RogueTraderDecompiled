using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

public class CritterCombatBehavior : MonoBehaviour, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler
{
	public GameObject CritterFxDissolve;

	private GameObject m_FX;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		SetFxEnabled(isTurnBased);
	}

	public void HandleTurnBasedModeResumed()
	{
		SetFxEnabled(Game.Instance.TurnController.TurnBasedModeActive);
	}

	private void SetFxEnabled(bool dissolve)
	{
		if (dissolve)
		{
			if (!m_FX)
			{
				m_FX = FxHelper.SpawnFxOnGameObject(CritterFxDissolve, base.gameObject);
			}
		}
		else if ((bool)m_FX)
		{
			FxHelper.Destroy(m_FX);
			m_FX = null;
		}
	}
}
