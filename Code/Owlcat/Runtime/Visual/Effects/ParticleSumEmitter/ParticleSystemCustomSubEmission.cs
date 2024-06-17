using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.ParticleSumEmitter;

public class ParticleSystemCustomSubEmission : MonoBehaviour
{
	[SerializeField]
	private GameObject m_SubEmitterParticleSystemsPrefab;

	private ParticleSystem m_ParticleSystem;

	private void Awake()
	{
		m_ParticleSystem = GetComponent<ParticleSystem>();
		ParticleSystem.CollisionModule collision = m_ParticleSystem.collision;
		collision.enabled = true;
		collision.sendCollisionMessages = true;
	}

	private void OnParticleCollision(GameObject other)
	{
		MonoSingleton<ParticleSystemCustomSubEmitterDelegate>.Instance.SubEmit(other, m_ParticleSystem, m_SubEmitterParticleSystemsPrefab);
	}
}
