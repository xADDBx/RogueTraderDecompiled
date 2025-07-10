using System;
using System.Collections;
using Kingmaker.Blueprints.Root;
using Kingmaker.Sound.Base;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual.SectorMap;

public class ScanPulseController : MonoBehaviour
{
	private ParticleSystem[] m_PulseParticleSystems;

	private float m_ScanSpeed;

	private TimeSpan m_ScanStartTime;

	private static TimeSpan CurrentTime => Game.Instance.TimeController.GameTime;

	public float PulseRadius => m_ScanSpeed * (float)(CurrentTime - m_ScanStartTime).Seconds;

	private void Awake()
	{
		m_PulseParticleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
	}

	public void Pulse(float range, float duration)
	{
		base.gameObject.transform.localScale = Vector3.zero;
		range *= 2f;
		ParticleSystem[] pulseParticleSystems = m_PulseParticleSystems;
		for (int i = 0; i < pulseParticleSystems.Length; i++)
		{
			pulseParticleSystems[i].startLifetime = duration;
		}
		m_ScanSpeed = range / duration;
		m_ScanStartTime = CurrentTime;
		StartCoroutine(IncreaseSize(range, duration));
		m_PulseParticleSystems.FirstItem()?.Play(withChildren: true);
		if (BlueprintRoot.Instance.FxRoot.ScanSoundStart != null)
		{
			SoundEventsManager.PostEvent(BlueprintRoot.Instance.FxRoot.ScanSoundStart, base.gameObject);
		}
		else
		{
			PFLog.TechArt.Warning("ScanSoundStart is missing");
		}
	}

	private IEnumerator IncreaseSize(float range, float duration)
	{
		Vector3 initialScale = base.transform.localScale;
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float t = Mathf.PingPong(time, duration) / duration;
			base.gameObject.transform.localScale = Vector3.Lerp(initialScale, new Vector3(range, range, range), t);
			yield return null;
		}
	}
}
