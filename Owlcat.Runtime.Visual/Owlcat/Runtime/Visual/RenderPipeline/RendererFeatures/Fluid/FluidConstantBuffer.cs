using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.Fluid;

public static class FluidConstantBuffer
{
	public static int _AmbientWindNoiseMap = Shader.PropertyToID("_AmbientWindNoiseMap");

	public static int _AmbientWindNoiseMap_ST = Shader.PropertyToID("_AmbientWindNoiseMap_ST");

	public static int _AmbientWindParams = Shader.PropertyToID("_AmbientWindParams");

	public static int _FluidWorldBoundsMin = Shader.PropertyToID("_FluidWorldBoundsMin");

	public static int _FluidWorldBoundsSize = Shader.PropertyToID("_FluidWorldBoundsSize");

	public static int _FluidGlobalTime = Shader.PropertyToID("_FluidGlobalTime");

	public static int _Dt = Shader.PropertyToID("_Dt");

	public static int _Target = Shader.PropertyToID("_Target");

	public static int _Velocity = Shader.PropertyToID("_Velocity");

	public static int _Alpha = Shader.PropertyToID("_Alpha");

	public static int _Divergence = Shader.PropertyToID("_Divergence");

	public static int _Pressure = Shader.PropertyToID("_Pressure");

	public static int _Decay = Shader.PropertyToID("_Decay");

	public static int _WindVelocityRT = Shader.PropertyToID("_WindVelocityRT");

	public static int _WindVelocityRT_ST = Shader.PropertyToID("_WindVelocityRT_ST");

	public static int _ColorBuffer = Shader.PropertyToID("_ColorBuffer");

	public static int _ColorBuffer_ST = Shader.PropertyToID("_ColorBuffer_ST");

	public static int _FogTex0 = Shader.PropertyToID("_FogTex0");

	public static int _FogTex1 = Shader.PropertyToID("_FogTex1");

	public static int _FogTex0_ST = Shader.PropertyToID("_FogTex0_ST");

	public static int _FogTex1_ST = Shader.PropertyToID("_FogTex1_ST");

	public static int _FogIntensityScale = Shader.PropertyToID("_FogIntensityScale");

	public static int _FluidFogMask = Shader.PropertyToID("_FluidFogMask");

	public static int _FluidFogMask_ST = Shader.PropertyToID("_FluidFogMask_ST");

	public static int _VelocityMask = Shader.PropertyToID("_VelocityMask");

	public static int _FadeMask = Shader.PropertyToID("_FadeMask");

	public static int _FadeColor = Shader.PropertyToID("_FadeColor");

	public static int _Parameters = Shader.PropertyToID("_Parameters");

	public static int _DebugColorScaleOffset = Shader.PropertyToID("_DebugColorScaleOffset");
}
