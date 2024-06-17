using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;

public class ColliderRef
{
	public ColliderType Type;

	public bool IsGlobal;

	public Body Owner;

	public float4x4 World;

	public float4 Parameters0;

	public float Friction;

	public float Restitution;
}
