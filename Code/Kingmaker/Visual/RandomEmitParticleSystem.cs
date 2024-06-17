using System;
using UnityEngine;

namespace Kingmaker.Visual;

public class RandomEmitParticleSystem : MonoBehaviour
{
	public ParticleSystem[] ControlledParticleSystems;

	public float RandomX;

	public float RandomY;

	public float RandomZ;

	public float Delay;

	public float RandomDelay;

	public int MaxEmit;

	private TimeSpan m_NextEmitTime;

	private int m_NumberOfEmit;

	private Vector3 m_PreviousOffset = Vector3.zero;

	private TimeSpan Time
	{
		get
		{
			if (Game.Instance == null || Game.Instance.Player == null)
			{
				return TimeSpan.MinValue;
			}
			return Game.Instance.TimeController.GameTime;
		}
	}

	private void UpdateNextEmitTime()
	{
		TimeSpan ts = new TimeSpan(0, 0, 0, 0, Mathf.RoundToInt((Delay + UnityEngine.Random.value * RandomDelay) * 1000f));
		m_NextEmitTime = Time.Add(ts);
	}

	private void OnEnable()
	{
		MaxEmit = 0;
		UpdateNextEmitTime();
	}

	private void Update()
	{
		if ((MaxEmit == 0 || m_NumberOfEmit < MaxEmit) && Time > m_NextEmitTime)
		{
			Vector3 vector = new Vector3(UnityEngine.Random.Range(0f - RandomX, RandomX), UnityEngine.Random.Range(0f - RandomY, RandomY), UnityEngine.Random.Range(0f - RandomZ, RandomZ));
			ParticleSystem[] controlledParticleSystems = ControlledParticleSystems;
			foreach (ParticleSystem obj in controlledParticleSystems)
			{
				obj.transform.localPosition -= m_PreviousOffset;
				obj.transform.localPosition += vector;
				obj.Play();
				m_NumberOfEmit++;
			}
			m_PreviousOffset = vector;
			UpdateNextEmitTime();
		}
	}
}
