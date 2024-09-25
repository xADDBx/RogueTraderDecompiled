using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class DeferredBuffer
{
	public static int _ResultUAV = Shader.PropertyToID("_ResultUAV");

	public static int _SpecCube0 = Shader.PropertyToID("_SpecCube0");

	public static int _SpecCube0_HDR = Shader.PropertyToID("_SpecCube0_HDR");

	public static int _SpecCube0_ProbePosition = Shader.PropertyToID("_SpecCube0_ProbePosition");

	public static int _SpecCube0_BoxMin = Shader.PropertyToID("_SpecCube0_BoxMin");

	public static int _SpecCube0_BoxMax = Shader.PropertyToID("_SpecCube0_BoxMax");

	public static int _UseBoxProjection = Shader.PropertyToID("_UseBoxProjection");
}
