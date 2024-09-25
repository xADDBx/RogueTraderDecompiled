using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class LootFxParticles : MonoBehaviour
{
	private ParticleSystem m_Particles;

	private void Start()
	{
		m_Particles = GetComponentInChildren<ParticleSystem>();
	}

	private void Update()
	{
		ParticleSystem.EmissionModule emission = m_Particles.emission;
		emission.enabled = !Game.Instance.Player.IsInCombat;
	}
}
