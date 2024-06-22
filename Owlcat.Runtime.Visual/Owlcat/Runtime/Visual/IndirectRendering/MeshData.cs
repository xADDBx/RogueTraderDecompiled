using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.IndirectRendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@0.1.216\\Runtime\\IndirectRendering\\IndirectInstanceData.cs", packingRules = PackingRules.Exact, needAccessors = false)]
public struct MeshData
{
	public Vector3 aabbMin;

	public Vector3 aabbMax;

	public Vector2 unused;
}
