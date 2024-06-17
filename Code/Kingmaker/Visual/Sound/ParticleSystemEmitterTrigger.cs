using Kingmaker.ElementsSystem;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemEmitterTrigger : MonoBehaviour
{
	private ParticleSystem m_System;

	private float m_LastUpdateTime;

	private ParticleSystem.Burst[] m_Bursts;

	private GameObject m_AudioObject;

	[SerializeField]
	private ActionsReference m_OnEmitActions;

	[AkEventReference]
	[SerializeField]
	private string m_EventOnEmit;

	private void OnEnable()
	{
		if (m_System == null)
		{
			m_System = GetComponent<ParticleSystem>();
			m_Bursts = new ParticleSystem.Burst[m_System.emission.burstCount];
			m_System.emission.GetBursts(m_Bursts);
		}
		if (m_AudioObject == null)
		{
			m_AudioObject = GetComponentInParent<AudioObject>()?.gameObject ?? GetComponentInParent<PooledFx>()?.gameObject ?? base.gameObject;
		}
	}

	private void Update()
	{
		ParticleSystem.Burst[] bursts = m_Bursts;
		for (int i = 0; i < bursts.Length; i++)
		{
			ParticleSystem.Burst burst = bursts[i];
			float num = burst.time;
			int num2 = 0;
			while (m_System.time >= num && num2 < burst.cycleCount)
			{
				if (m_LastUpdateTime < num)
				{
					m_OnEmitActions.Get()?.Actions.Run();
					if (!string.IsNullOrEmpty(m_EventOnEmit))
					{
						SoundEventsManager.PostEvent(m_EventOnEmit, m_AudioObject);
					}
					break;
				}
				num += burst.repeatInterval;
				num2++;
			}
			if (m_System.main.loop && burst.cycleCount == 1 && burst.time == 0f && m_System.time < m_LastUpdateTime)
			{
				m_OnEmitActions.Get()?.Actions.Run();
				if (!string.IsNullOrEmpty(m_EventOnEmit))
				{
					SoundEventsManager.PostEvent(m_EventOnEmit, m_AudioObject);
				}
				break;
			}
		}
		m_LastUpdateTime = m_System.time;
	}
}
