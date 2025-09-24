using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;

public struct ParticleMotionPair
{
	public float3 Predicted;

	public float3 Velocity;
}
