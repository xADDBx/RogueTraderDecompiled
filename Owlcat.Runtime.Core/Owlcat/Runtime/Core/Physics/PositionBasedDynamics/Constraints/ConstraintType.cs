using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.core@1.0.52\\Runtime\\Physics\\PositionBasedDynamics\\Constraints\\ConstraintType.cs")]
public enum ConstraintType
{
	Distance,
	DistanceAngular,
	TriangleBend,
	ShapeMatching,
	Grass,
	StretchShear,
	BendTwist
}
