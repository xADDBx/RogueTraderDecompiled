using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Utilities;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0.1.216\\Runtime\\Utilities\\GeometryUtils.cs")]
[BurstCompile]
public struct OrientedBBox
{
	public Vector3 right;

	public float extentX;

	public Vector3 up;

	public float extentY;

	public Vector3 center;

	public float extentZ;

	public Vector3 forward => Vector3.Cross(up, right);

	public OrientedBBox(Matrix4x4 trs)
	{
		Vector3 vector = trs.GetColumn(0);
		Vector3 vector2 = trs.GetColumn(1);
		Vector3 vector3 = trs.GetColumn(2);
		center = trs.GetColumn(3);
		right = vector * (1f / vector.magnitude);
		up = vector2 * (1f / vector2.magnitude);
		extentX = 0.5f * vector.magnitude;
		extentY = 0.5f * vector2.magnitude;
		extentZ = 0.5f * vector3.magnitude;
	}
}
