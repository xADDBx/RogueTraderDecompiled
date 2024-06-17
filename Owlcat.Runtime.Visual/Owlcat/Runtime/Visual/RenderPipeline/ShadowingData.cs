using Owlcat.Runtime.Visual.RenderPipeline.Shadows;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public struct ShadowingData
{
	public int ScreenSpaceShadowsTextureCount;

	public int ScreenSpaceShadowsTotalChannelsCount;

	public ShadowQuality ShadowQuality;

	public int AtlasSize;

	public float ShadowNearPlane;

	public int DirectionalLightCascadeResolution;

	public int PointLightResolution;

	public int SpotLightResolution;

	public Cascades DirectionalLightCascades;

	public float DepthBias;

	public float NormalBias;
}
