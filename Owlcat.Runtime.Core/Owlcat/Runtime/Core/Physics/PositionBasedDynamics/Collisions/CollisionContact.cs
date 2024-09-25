using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;

public struct CollisionContact
{
	public int ColliderId;

	public int ParticleId;

	public float3 Normal;

	public float3 Position;

	public float Restitution;

	public float Friction;
}
