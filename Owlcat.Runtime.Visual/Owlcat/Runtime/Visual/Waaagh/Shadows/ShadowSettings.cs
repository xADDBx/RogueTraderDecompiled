using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[Serializable]
public class ShadowSettings
{
	public ShadowQuality ShadowQuality = ShadowQuality.All;

	public ShadowUpdateDistances ShadowUpdateDistances = new ShadowUpdateDistances();

	public ShadowResolution AtlasSize = ShadowResolution._4096;

	public bool StaticShadowsCacheEnabled;

	public ShadowResolution CacheAtlasSize = ShadowResolution._2048;

	public float ShadowNearPlane = 2f;

	[SerializeField]
	[FormerlySerializedAs("ShadowDistance")]
	private float m_ShadowDistance = 50f;

	[NonSerialized]
	public OverridableValue<float> ShadowDistance;

	public ShadowResolution DirectionalLightCascadeResolution = ShadowResolution._1024;

	public Cascades DirectionalLightCascades = new Cascades();

	public ShadowResolutionSettings PointLightResolution = new ShadowResolutionSettings
	{
		DefaultTier = ShadowResolutionTier.High,
		Low = ShadowResolution._128,
		Medium = ShadowResolution._256,
		High = ShadowResolution._512,
		Ultra = ShadowResolution._1024
	};

	public ShadowResolutionSettings SpotLightResolution = new ShadowResolutionSettings
	{
		DefaultTier = ShadowResolutionTier.High,
		Low = ShadowResolution._128,
		Medium = ShadowResolution._256,
		High = ShadowResolution._512,
		Ultra = ShadowResolution._1024
	};

	[Range(0f, 10f)]
	public float DepthBias = 2.6f;

	[Range(0f, 3f)]
	public float NormalBias = 0.1f;

	[HideInInspector]
	[Range(0f, 2f)]
	public float ReceiverNormalBias;

	public ShadowSettings()
	{
		ShadowDistance = new OverridableValue<float>(() => m_ShadowDistance);
	}
}
