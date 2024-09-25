using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FogOfWar;

public static class FogOfWarConstantBuffer
{
	public static int _ColorMask = Shader.PropertyToID("_ColorMask");

	public static int _ClearColor = Shader.PropertyToID("_ClearColor");

	public static int VIEW_PROJ = Shader.PropertyToID("VIEW_PROJ");

	public static int _Vertices = Shader.PropertyToID("_Vertices");

	public static int _Radius = Shader.PropertyToID("_Radius");

	public static int _LightPosition = Shader.PropertyToID("_LightPosition");

	public static int _Falloff = Shader.PropertyToID("_Falloff");

	public static int _FogOfWarRadius = Shader.PropertyToID("_FogOfWarRadius");

	public static int _FogOfWarBorderWidth = Shader.PropertyToID("_FogOfWarBorderWidth");

	public static int _FogOfWarCustomRevealerMask = Shader.PropertyToID("_FogOfWarCustomRevealerMask");

	public static int _Parameter = Shader.PropertyToID("_Parameter");

	public static int _FogOfWarShadowMap = Shader.PropertyToID("_FogOfWarShadowMap");

	public static int _StaticMask = Shader.PropertyToID("_StaticMask");

	public static int _FogOfWarMask = Shader.PropertyToID("_FogOfWarMask");

	public static int _FogOfWarMask_ST = Shader.PropertyToID("_FogOfWarMask_ST");

	public static int _FogOfWarColor = Shader.PropertyToID("_FogOfWarColor");

	public static int _FogOfWarGlobalFlag = Shader.PropertyToID("_FogOfWarGlobalFlag");

	public static int _FogOfWarMaskSize = Shader.PropertyToID("_FogOfWarMaskSize");

	public static int _BorderEnabled = Shader.PropertyToID("_BorderEnabled");

	public static int _WorldSize = Shader.PropertyToID("_WorldSize");

	public static int _BorderWidth = Shader.PropertyToID("_BorderWidth");

	public static int _BorderOffset = Shader.PropertyToID("_BorderOffset");
}
