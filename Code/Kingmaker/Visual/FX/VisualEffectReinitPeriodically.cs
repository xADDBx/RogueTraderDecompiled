using System.Collections;
using JetBrains.Annotations;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker.Visual.FX;

internal sealed class VisualEffectReinitPeriodically : MonoBehaviour, ICloseLoadingScreenHandler, ISubscriber
{
	[SerializeField]
	private float m_TimeIntervalInSeconds = 10f;

	[AkEventReference]
	[SerializeField]
	private string m_StartSound;

	[SerializeField]
	private bool m_WaitForSplashEnd;

	private Coroutine m_ReinitLoopCoroutine;

	private float m_ReinitLoopTimeInterval;

	[UsedImplicitly]
	private void OnEnable()
	{
		if (m_WaitForSplashEnd)
		{
			EventBus.Subscribe(this);
		}
		else
		{
			StartReinitLoop();
		}
	}

	public void HandleCloseLoadingScreen()
	{
		StartReinitLoop();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if (m_WaitForSplashEnd)
		{
			EventBus.Unsubscribe(this);
		}
		StopReinitLoop();
	}

	[UsedImplicitly]
	private void OnValidate()
	{
		if (m_ReinitLoopCoroutine != null && !Mathf.Approximately(m_ReinitLoopTimeInterval, m_TimeIntervalInSeconds))
		{
			StopReinitLoop();
			StartReinitLoop();
		}
	}

	private void StartReinitLoop()
	{
		if (!(m_TimeIntervalInSeconds <= 0f) && TryGetComponent<VisualEffect>(out var component))
		{
			m_ReinitLoopTimeInterval = m_TimeIntervalInSeconds;
			m_ReinitLoopCoroutine = StartCoroutine(ReinitLoop(component, m_TimeIntervalInSeconds));
		}
	}

	private void StopReinitLoop()
	{
		if (m_ReinitLoopCoroutine != null)
		{
			StopCoroutine(m_ReinitLoopCoroutine);
			m_ReinitLoopCoroutine = null;
			m_ReinitLoopTimeInterval = 0f;
		}
	}

	private IEnumerator ReinitLoop(VisualEffect visualEffect, float timeInterval)
	{
		WaitForSeconds delay = new WaitForSeconds(timeInterval);
		while (visualEffect != null)
		{
			PostSoundEvent();
			yield return delay;
			visualEffect.Reinit();
		}
	}

	private void PostSoundEvent()
	{
		if (!string.IsNullOrEmpty(m_StartSound))
		{
			SoundEventsManager.PostEvent(m_StartSound, base.gameObject);
		}
	}
}
