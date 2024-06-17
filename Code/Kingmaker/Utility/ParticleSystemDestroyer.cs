using System.Collections;
using System.Linq;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Utility;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemDestroyer : MonoBehaviour
{
	public float MinDuration = 8f;

	public float MaxDuration = 10f;

	private float m_MaxLifetime;

	private bool m_EarlyStop;

	private float GetLifetime(ParticleSystem system)
	{
		float startLifetimeMultiplier = system.main.startLifetimeMultiplier;
		startLifetimeMultiplier = Mathf.Max(system.main.startLifetime.constantMax, startLifetimeMultiplier);
		if (system.main.startLifetime.curve != null)
		{
			startLifetimeMultiplier = Mathf.Max(system.main.startLifetime.curve.keys.Max((Keyframe k) => k.value), startLifetimeMultiplier);
		}
		if (system.main.startLifetime.curveMax != null)
		{
			startLifetimeMultiplier = Mathf.Max(system.main.startLifetime.curveMax.keys.Max((Keyframe k) => k.value), startLifetimeMultiplier);
		}
		return startLifetimeMultiplier;
	}

	private IEnumerator Start()
	{
		ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = systems;
		foreach (ParticleSystem system in array)
		{
			m_MaxLifetime = Mathf.Max(GetLifetime(system), m_MaxLifetime);
		}
		float stopTime = Time.time + PFStatefulRandom.Visuals.Particles.Range(MinDuration, MaxDuration);
		while (Time.time < stopTime || m_EarlyStop)
		{
			yield return null;
		}
		PFLog.Default.Log("stopping " + base.name);
		array = systems;
		for (int i = 0; i < array.Length; i++)
		{
			ParticleSystem.EmissionModule emission = array[i].emission;
			emission.enabled = false;
		}
		BroadcastMessage("Extinguish", SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(m_MaxLifetime);
		Object.Destroy(base.gameObject);
	}

	public void Stop()
	{
		m_EarlyStop = true;
	}
}
