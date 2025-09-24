using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUParticleSoA : GPUSoABase
{
	public static int _PbdParticlesPositionPairsBuffer = Shader.PropertyToID("_PbdParticlesPositionPairsBuffer");

	public static int _PbdParticlesMotionPairsBuffer = Shader.PropertyToID("_PbdParticlesMotionPairsBuffer");

	public static int _PbdParticlesExtendedDataBuffer = Shader.PropertyToID("_PbdParticlesExtendedDataBuffer");

	public ComputeBufferWrapper<ParticlePositionPair> PositionPairsBuffer;

	public ComputeBufferWrapper<ParticleMotionPair> MotionPairsBuffer;

	public ComputeBufferWrapper<ParticleExtendedData> ExtendedDataBuffer;

	public override string Name => "GPUParticleSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[3]
		{
			PositionPairsBuffer = new ComputeBufferWrapper<ParticlePositionPair>("_PbdParticlesPositionPairsBuffer", size),
			MotionPairsBuffer = new ComputeBufferWrapper<ParticleMotionPair>("_PbdParticlesMotionPairsBuffer", size),
			ExtendedDataBuffer = new ComputeBufferWrapper<ParticleExtendedData>("_PbdParticlesExtendedDataBuffer", size)
		};
	}
}
