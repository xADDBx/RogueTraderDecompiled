using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.core@1.0.54\\Runtime\\Physics\\PositionBasedDynamics\\Collisions\\Broadphase\\BroadphaseType.cs")]
public enum BroadphaseType
{
	SimpleGrid,
	MultilevelGrid,
	OptimizedSpatialHashing
}
