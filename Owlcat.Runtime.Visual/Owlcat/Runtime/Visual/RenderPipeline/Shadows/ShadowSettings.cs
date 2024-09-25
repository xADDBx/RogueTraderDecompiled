using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.Shadows;

[Serializable]
public class ShadowSettings
{
	public ShadowQuality ShadowQuality = ShadowQuality.All;

	[Range(1024f, 8192f)]
	public int AtlasSize = 4096;

	public float ShadowNearPlane = 2f;

	public float ShadowDistance = 50f;

	[Range(128f, 4096f)]
	public int DirectionalLightCascadeResolution = 1024;

	[Range(128f, 2048f)]
	public int PointLightResolution = 512;

	[Range(128f, 2048f)]
	public int SpotLightResolution = 512;

	public Cascades DirectionalLightCascades = new Cascades();

	[Range(0f, 10f)]
	public float DepthBias = 2.6f;

	[Range(0f, 3f)]
	public float NormalBias = 0.1f;
}
