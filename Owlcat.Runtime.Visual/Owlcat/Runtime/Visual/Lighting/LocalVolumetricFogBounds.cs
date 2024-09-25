using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.225\\Runtime\\Lighting\\LocalVolumetricFogGpuData.cs")]
public struct LocalVolumetricFogBounds
{
	public Vector3 right;

	public float extentX;

	public Vector3 up;

	public float extentY;

	public Vector3 center;

	public float extentZ;

	public float minZ;

	public float maxZ;

	public Vector2 pad0;
}
