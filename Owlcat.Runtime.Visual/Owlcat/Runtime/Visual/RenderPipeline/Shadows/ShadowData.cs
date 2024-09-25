using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Shadows;

[GenerateHLSL(PackingRules.Exact, false, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@1.1.225\\Runtime\\RenderPipeline\\Shadows\\ShadowData.cs")]
public struct ShadowData
{
	public Vector4 matrixIndices;

	public Vector4 atlasScaleOffset;

	public int shadowFlags;

	public int screenSpaceMask;

	public Vector2 unused;
}
