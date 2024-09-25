using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Debugging;

public class GPUDebugSoA
{
	public ComputeBuffer DrawParticlesArgsBuffer;

	public ComputeBuffer DebugParticleIndicesBuffer;

	public ComputeBuffer DrawDistanceConstraintsArgsBuffer;

	public ComputeBuffer DebugDistanceConstraintsIndicesBuffer;

	public ComputeBuffer DrawNormalsArgsBuffer;

	public ComputeBuffer DrawCollidersGridArgsBuffer;

	public ComputeBuffer DrawCollidersAabbArgsBuffer;

	public ComputeBuffer DrawForceVolumeAabbArgsBuffer;

	public ComputeBuffer DrawForceVolumeGridArgsBuffer;

	public ComputeBuffer DrawBodiesAabbArgsBuffer;

	public void Dispose()
	{
		DrawParticlesArgsBuffer?.Dispose();
		DebugParticleIndicesBuffer?.Dispose();
		DrawDistanceConstraintsArgsBuffer?.Dispose();
		DebugDistanceConstraintsIndicesBuffer?.Dispose();
		DrawNormalsArgsBuffer?.Dispose();
		DrawCollidersGridArgsBuffer?.Dispose();
		DrawCollidersAabbArgsBuffer?.Dispose();
		DrawForceVolumeAabbArgsBuffer?.Dispose();
		DrawForceVolumeGridArgsBuffer?.Dispose();
		DrawBodiesAabbArgsBuffer?.Dispose();
	}
}
