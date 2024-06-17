using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
public struct ShadowCacheCopyRequest
{
	public float4 DynamicAtlasScaleOffset;

	public float4 StaticAtlasScaleOffset;
}
