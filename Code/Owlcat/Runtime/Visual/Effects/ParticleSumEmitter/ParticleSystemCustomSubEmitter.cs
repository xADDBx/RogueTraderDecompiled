using System.Collections.Generic;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.ParticleSumEmitter;

public class ParticleSystemCustomSubEmitter : MonoBehaviour
{
	private ParticleSystem[] m_ParticleSystems;

	private ParticleSystem.EmissionModule[] m_EmissionModules;

	private int[] m_Bursts;

	private ParticleSystem.MainModule[] m_MainModules;

	private float[] m_ParticleStartSpeeds;

	private ParticleSystem.VelocityOverLifetimeModule[] m_VelocityOverLifetimeModules;

	private Vector3[] m_MinVelocityShifts;

	private Vector3[] m_MaxVelocityShifts;

	private List<float>[] m_SpawnTimes;

	private ParticleSystem.Particle[][] m_ParticleBuffers;

	public void Initialize()
	{
		m_ParticleSystems = GetComponentsInChildren<ParticleSystem>();
		CreateSpawnTimes();
		CacheModules();
		CacheBursts();
		CacheStartSpeeds();
		CacheVelocityShifts();
		SwitchSomeModulesOff();
	}

	private void CreateSpawnTimes()
	{
		m_SpawnTimes = new List<float>[m_ParticleSystems.Length];
		m_ParticleBuffers = new ParticleSystem.Particle[m_ParticleSystems.Length][];
		for (int i = 0; i < m_SpawnTimes.Length; i++)
		{
			m_SpawnTimes[i] = new List<float>();
		}
	}

	private void CacheModules()
	{
		m_EmissionModules = new ParticleSystem.EmissionModule[m_ParticleSystems.Length];
		m_MainModules = new ParticleSystem.MainModule[m_ParticleSystems.Length];
		m_VelocityOverLifetimeModules = new ParticleSystem.VelocityOverLifetimeModule[m_ParticleSystems.Length];
		for (int i = 0; i < m_EmissionModules.Length; i++)
		{
			m_EmissionModules[i] = m_ParticleSystems[i].emission;
			m_MainModules[i] = m_ParticleSystems[i].main;
			m_VelocityOverLifetimeModules[i] = m_ParticleSystems[i].velocityOverLifetime;
		}
	}

	private void CacheBursts()
	{
		m_Bursts = new int[m_ParticleSystems.Length];
		for (int i = 0; i < m_ParticleSystems.Length; i++)
		{
			ParticleSystem.EmissionModule emissionModule = m_EmissionModules[i];
			if (emissionModule.burstCount == 0)
			{
				Debug.LogError($"[ParticleSystemCustomSubEmitter] Particle system {m_ParticleSystems[i].gameObject} doesn't have bursts");
				m_Bursts[i] = 1;
				continue;
			}
			if (emissionModule.burstCount > 1)
			{
				Debug.LogError($"[ParticleSystemCustomSubEmitter] Particle system {m_ParticleSystems[i].gameObject} has more than 1 bursts");
			}
			m_Bursts[i] = m_ParticleSystems[i].emission.GetBurst(0).maxCount;
		}
	}

	private void CacheStartSpeeds()
	{
		m_ParticleStartSpeeds = new float[m_ParticleSystems.Length];
		for (int i = 0; i < m_ParticleSystems.Length; i++)
		{
			ParticleSystem.MainModule mainModule = m_MainModules[i];
			m_ParticleStartSpeeds[i] = mainModule.startSpeedMultiplier;
		}
	}

	private void CacheVelocityShifts()
	{
		m_MinVelocityShifts = new Vector3[m_ParticleSystems.Length];
		m_MaxVelocityShifts = new Vector3[m_ParticleSystems.Length];
		for (int i = 0; i < m_ParticleSystems.Length; i++)
		{
			m_MinVelocityShifts[i].x = m_VelocityOverLifetimeModules[i].x.constantMin;
			m_MinVelocityShifts[i].y = m_VelocityOverLifetimeModules[i].y.constantMin;
			m_MinVelocityShifts[i].z = m_VelocityOverLifetimeModules[i].z.constantMin;
			m_MaxVelocityShifts[i].x = m_VelocityOverLifetimeModules[i].x.constantMax;
			m_MaxVelocityShifts[i].y = m_VelocityOverLifetimeModules[i].y.constantMax;
			m_MaxVelocityShifts[i].z = m_VelocityOverLifetimeModules[i].z.constantMax;
		}
	}

	private void SwitchSomeModulesOff()
	{
		for (int i = 0; i < m_ParticleSystems.Length; i++)
		{
			m_EmissionModules[i].enabled = false;
			m_VelocityOverLifetimeModules[i].enabled = false;
		}
	}

	public void ClearParticlesAfterCurrentTime(float time)
	{
		for (int i = 0; i < m_SpawnTimes.Length; i++)
		{
			if (m_SpawnTimes[i].Count > m_MainModules[i].maxParticles)
			{
				m_SpawnTimes[i].RemoveRange(0, m_SpawnTimes[i].Count - m_MainModules[i].maxParticles);
			}
			int num = 0;
			int num2 = m_SpawnTimes[i].Count;
			int num3;
			while (num + 1 < num2)
			{
				num3 = (num + num2) / 2;
				if (m_SpawnTimes[i][num3] < time)
				{
					num = num3;
				}
				else
				{
					num2 = num3;
				}
			}
			num3 = num2;
			m_SpawnTimes[i].RemoveRange(num3, m_SpawnTimes[i].Count - num3);
			if (m_ParticleBuffers[i] == null || m_ParticleBuffers[i].Length < num3)
			{
				m_ParticleBuffers[i] = new ParticleSystem.Particle[num3];
			}
			m_ParticleSystems[i].GetParticles(m_ParticleBuffers[i], num3);
			m_ParticleSystems[i].SetParticles(m_ParticleBuffers[i], num3);
		}
	}

	public void ClearAllParticles()
	{
		for (int i = 0; i < m_ParticleSystems.Length; i++)
		{
			m_ParticleSystems[i].Clear();
			m_SpawnTimes[i].Clear();
			m_ParticleBuffers[i] = null;
		}
	}

	public void Emit(List<ParticleCollisionEvent> particleCollisionEvents, int eventsCount, float currentTime)
	{
		ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
		for (int i = 0; i < eventsCount; i++)
		{
			ParticleCollisionEvent particleCollisionEvent = particleCollisionEvents[i];
			emitParams.position = particleCollisionEvent.intersection;
			Vector3 velocity = particleCollisionEvent.velocity;
			Vector3 normal = particleCollisionEvent.normal;
			Vector3 vector = velocity - Vector3.Project(velocity, normal);
			Vector3 normalized = normal.normalized;
			Vector3 normalized2 = vector.normalized;
			Vector3 right = Vector3.Cross(normalized2, normalized);
			for (int j = 0; j < m_ParticleSystems.Length; j++)
			{
				for (int k = 0; k < m_Bursts[j]; k++)
				{
					Vector3 source = RandomRangeVector3(m_MinVelocityShifts[j], m_MaxVelocityShifts[j]);
					source = AlignWithAxes(source, right, normalized, normalized2);
					emitParams.velocity = vector * m_ParticleStartSpeeds[j] + source;
					m_ParticleSystems[j].Emit(emitParams, 1);
					m_SpawnTimes[j].Add(currentTime);
				}
			}
		}
	}

	private Vector3 RandomRangeVector3(Vector3 min, Vector3 max)
	{
		return new Vector3(PFStatefulRandom.Visuals.Particles.Range(min.x, max.x), PFStatefulRandom.Visuals.Particles.Range(min.y, max.y), PFStatefulRandom.Visuals.Particles.Range(min.z, max.z));
	}

	private Vector3 AlignWithAxes(Vector3 source, Vector3 right, Vector3 up, Vector3 forward)
	{
		return right * source.x + up * source.y + forward * source.z;
	}
}
