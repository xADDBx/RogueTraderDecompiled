using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUParticleSoA : GPUSoABase
{
	public static int _PbdParticlesBasePositionBuffer = Shader.PropertyToID("_PbdParticlesBasePositionBuffer");

	public static int _PbdParticlesPositionBuffer = Shader.PropertyToID("_PbdParticlesPositionBuffer");

	public static int _PbdParticlesPredictedBuffer = Shader.PropertyToID("_PbdParticlesPredictedBuffer");

	public static int _PbdParticlesVelocityBuffer = Shader.PropertyToID("_PbdParticlesVelocityBuffer");

	public static int _PbdParticlesMassBuffer = Shader.PropertyToID("_PbdParticlesMassBuffer");

	public static int _PbdParticlesRadiusBuffer = Shader.PropertyToID("_PbdParticlesRadiusBuffer");

	public static int _PbdParticlesFlagsBuffer = Shader.PropertyToID("_PbdParticlesFlagsBuffer");

	public ComputeBufferWrapper<float3> BasePositionBuffer;

	public ComputeBufferWrapper<float3> PositionBuffer;

	public ComputeBufferWrapper<float3> PredictedBuffer;

	public ComputeBufferWrapper<float3> VelocityBuffer;

	public ComputeBufferWrapper<float> MassBuffer;

	public ComputeBufferWrapper<float> RadiusBuffer;

	public ComputeBufferWrapper<uint> FlagsBuffer;

	public override string Name => "GPUParticleSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[7]
		{
			BasePositionBuffer = new ComputeBufferWrapper<float3>("_PbdParticlesBasePositionBuffer", size),
			PositionBuffer = new ComputeBufferWrapper<float3>("_PbdParticlesPositionBuffer", size),
			PredictedBuffer = new ComputeBufferWrapper<float3>("_PbdParticlesPredictedBuffer", size),
			VelocityBuffer = new ComputeBufferWrapper<float3>("_PbdParticlesVelocityBuffer", size),
			MassBuffer = new ComputeBufferWrapper<float>("_PbdParticlesMassBuffer", size),
			RadiusBuffer = new ComputeBufferWrapper<float>("_PbdParticlesRadiusBuffer", size),
			FlagsBuffer = new ComputeBufferWrapper<uint>("_PbdParticlesFlagsBuffer", size)
		};
	}
}
