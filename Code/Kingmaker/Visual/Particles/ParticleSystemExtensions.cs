using UnityEngine;

namespace Kingmaker.Visual.Particles;

public static class ParticleSystemExtensions
{
	private static ParticleSystem.Particle[] s_Particles = new ParticleSystem.Particle[2000];

	public static int GetParticlesReusable(this ParticleSystem particleSystem, out ParticleSystem.Particle[] particles)
	{
		if (s_Particles.Length < particleSystem.main.maxParticles)
		{
			s_Particles = new ParticleSystem.Particle[(int)((float)particleSystem.main.maxParticles * 1.5f)];
		}
		int particles2 = particleSystem.GetParticles(s_Particles);
		particles = s_Particles;
		return particles2;
	}
}
