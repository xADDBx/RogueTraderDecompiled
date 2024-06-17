using Unity.Burst;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowData
{
	public ShadowLightData LightData;

	public ShadowRenderData RenderData;

	public ShadowAtlasData DynamicAtlasData;

	public ShadowAtlasData StaticCacheAtlasData;

	public bool RenderDataValid;

	public int LastVisibleFrameId;

	public int LastRenderedFrameId;
}
