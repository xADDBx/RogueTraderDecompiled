using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.core@9f31685b6d60\\Runtime\\Physics\\PositionBasedDynamics\\Collisions\\ColliderType.cs")]
public enum ColliderType
{
	Plane,
	Sphere,
	Capsule,
	Box
}
