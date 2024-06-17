using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid;

[Serializable]
public class FluidFogSettings
{
	public bool Enabled;

	[Space]
	public Texture2D FogTex0;

	public Vector4 FogTex0TilingScrollSpeed = new Vector4(1f, 1f, 0f, 0f);

	public Texture2D FogTex1;

	public Vector4 FogTex1TilingScrollSpeed = new Vector4(1f, 1f, 0f, 0f);

	[Space]
	public float FogUpdateFactor = 1f;

	public float FogFadeoutFactor = 0.999f;

	public static FluidFogSettings DefaultSettings => new FluidFogSettings
	{
		Enabled = false,
		FogTex0TilingScrollSpeed = new Vector4(1f, 1f, 0f, 0f),
		FogTex1TilingScrollSpeed = new Vector4(1f, 1f, 0f, 0f),
		FogUpdateFactor = 1f,
		FogFadeoutFactor = 0.999f
	};
}
