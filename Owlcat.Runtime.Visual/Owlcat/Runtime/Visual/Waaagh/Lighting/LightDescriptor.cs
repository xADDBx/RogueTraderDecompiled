using Owlcat.Runtime.Visual.Lighting;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Lighting;

[BurstCompile]
public struct LightDescriptor
{
	public VisibleLight VisibleLight;

	public int LightID;

	public int LightUnsortedIndex;

	public float MinZ;

	public float MaxZ;

	public float MeanZ;

	public float ShadowStrength;

	public float InnerRadius;

	public bool IsBaked;

	public LightFalloffType LightFalloffType;

	public bool SnapSpecularToInnerRadius;

	public float InnerSpotAngle;

	public int ShadowDataIndex;

	public int ShadowmaskChannel;

	public LightShadows Shadows;

	public float ShadowNearPlane;

	public float ShadowDepthBias;

	public float ShadowNormalBias;

	public LightShadowmapResolution ShadowmapResolution;

	public ShadowmapUpdateMode ShadowmapUpdateMode;

	public bool ShadowmapAlwaysDrawDynamicShadowCasters;

	public bool ShadowmapUpdateOnLightMovement;

	public bool ShadowsCanBeCached;

	public uint LightLayerMask;

	public bool VolumetricLighting;

	public bool VolumetricShadows;

	public float VolumetricIntensity;

	public int LightCookieIndex;

	public LightCookieDescriptor lightCookieDescriptor;

	public ShadowUpdateFrequencyByDistance ShadowUpdateFrequencyByDistance;
}
