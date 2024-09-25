using UnityEngine.Rendering;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.core@1.0.52\\Runtime\\Physics\\PositionBasedDynamics\\Forces\\ForceEmissionType.cs")]
public enum ForceEmissionType
{
	Point,
	Axis,
	Vortex,
	Directional
}
