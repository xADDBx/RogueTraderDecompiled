using Owlcat.Runtime.Visual.Waaagh.Shadows;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh;

public struct ShadowData
{
	public ShadowUpdateDistances ShadowUpdateDistances;

	public bool StaticShadowsCacheEnabled;

	internal ShadowManager ShadowManager;

	public ShadowQuality ShadowQuality;

	public Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution AtlasSize;

	public Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution CacheAtlasSize;

	public float ShadowNearPlane;

	public Owlcat.Runtime.Visual.Waaagh.Shadows.ShadowResolution DirectionalLightCascadeResolution;

	public ShadowResolutionSettings PointLightResolution;

	public ShadowResolutionSettings SpotLightResolution;

	public Cascades DirectionalLightCascades;

	public float DepthBias;

	public float NormalBias;

	public float ReceiverNormalBias;
}
