using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker.Visual.FX;

internal sealed class VisualEffectReinitPeriodically : MonoBehaviour
{
	[SerializeField]
	private float m_TimeIntervalInSeconds = 10f;

	private Coroutine m_ReinitLoopCoroutine;

	private float m_ReinitLoopTimeInterval;

	[UsedImplicitly]
	private void OnEnable()
	{
		StartReinitLoop();
	}

	[UsedImplicitly]
	private void OnDisable()
	{
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
		yield return delay;
		while (visualEffect != null)
		{
			visualEffect.Reinit();
			yield return delay;
		}
	}
}
