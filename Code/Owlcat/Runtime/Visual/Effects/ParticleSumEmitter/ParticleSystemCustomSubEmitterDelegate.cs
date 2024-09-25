using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints.Area;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.ParticleSumEmitter;

public class ParticleSystemCustomSubEmitterDelegate : MonoSingleton<ParticleSystemCustomSubEmitterDelegate>, IAreaActivationHandler, ISubscriber
{
	private Dictionary<int, ParticleSystemCustomSubEmitter> m_SubEmittersDictionary = new Dictionary<int, ParticleSystemCustomSubEmitter>();

	private BlueprintArea m_LastLoadedArea;

	private List<ParticleCollisionEvent> m_ParticleCollisionEventsList = new List<ParticleCollisionEvent>();

	public void SubEmit(GameObject collisionObject, ParticleSystem source, GameObject subEmitter)
	{
		int collisionEvents = source.GetCollisionEvents(collisionObject, m_ParticleCollisionEventsList);
		float currentTime = (float)Game.Instance.TimeController.RealTime.TotalSeconds;
		GetSubEmitter(subEmitter).Emit(m_ParticleCollisionEventsList, collisionEvents, currentTime);
	}

	private void Awake()
	{
		m_LastLoadedArea = Game.Instance.CurrentlyLoadedArea;
		ClearAllParticles();
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnAreaActivated()
	{
		if (Game.Instance.CurrentlyLoadedArea == m_LastLoadedArea)
		{
			ClearParticlesAfterCurrentTime();
		}
		else
		{
			ClearAllParticles();
		}
	}

	public void ClearAllParticles()
	{
		foreach (ParticleSystemCustomSubEmitter value in m_SubEmittersDictionary.Values)
		{
			value.ClearAllParticles();
		}
	}

	public void ClearParticlesAfterCurrentTime()
	{
		float time = (float)Game.Instance.TimeController.RealTime.TotalSeconds;
		foreach (ParticleSystemCustomSubEmitter value in m_SubEmittersDictionary.Values)
		{
			value.ClearParticlesAfterCurrentTime(time);
		}
	}

	private ParticleSystemCustomSubEmitter GetSubEmitter(GameObject subEmitter)
	{
		int instanceID = subEmitter.GetInstanceID();
		if (m_SubEmittersDictionary.ContainsKey(instanceID))
		{
			return m_SubEmittersDictionary[instanceID];
		}
		GameObject gameObject = Object.Instantiate(subEmitter, Vector3.zero, quaternion.identity, base.transform);
		ParticleSystemCustomSubEmitter particleSystemCustomSubEmitter = gameObject.GetComponent<ParticleSystemCustomSubEmitter>();
		if (particleSystemCustomSubEmitter == null)
		{
			particleSystemCustomSubEmitter = gameObject.AddComponent<ParticleSystemCustomSubEmitter>();
		}
		particleSystemCustomSubEmitter.Initialize();
		m_SubEmittersDictionary[instanceID] = particleSystemCustomSubEmitter;
		return particleSystemCustomSubEmitter;
	}
}
