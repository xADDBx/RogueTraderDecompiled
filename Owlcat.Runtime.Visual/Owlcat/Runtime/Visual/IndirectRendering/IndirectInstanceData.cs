using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.IndirectRendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.232\\Runtime\\IndirectRendering\\IndirectInstanceData.cs", packingRules = PackingRules.Exact, needAccessors = false)]
public struct IndirectInstanceData
{
	public Matrix4x4 objectToWorld;

	public Matrix4x4 worldToObject;

	public uint meshID;

	public Vector3 tintColor;

	public Vector4 shadowmask;

	public uint hidden;

	public uint physicsDataIndex;

	public Vector2 unused;
}
