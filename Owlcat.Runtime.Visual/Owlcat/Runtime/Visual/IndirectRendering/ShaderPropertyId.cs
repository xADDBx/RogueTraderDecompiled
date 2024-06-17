using UnityEngine;

namespace Owlcat.Runtime.Visual.IndirectRendering;

public static class ShaderPropertyId
{
	public static readonly int _IndirectInstanceDataBuffer = Shader.PropertyToID("_IndirectInstanceDataBuffer");

	public static readonly int _LightProbesBuffer = Shader.PropertyToID("_LightProbesBuffer");

	public static readonly int _Surface = Shader.PropertyToID("_Surface");

	public static readonly int _DistortionEnabled = Shader.PropertyToID("_DistortionEnabled");

	public static readonly int _ArgsOffset = Shader.PropertyToID("_ArgsOffset");

	public static readonly int _ArgsBuffer = Shader.PropertyToID("_ArgsBuffer");

	public static readonly int _IsVisibleBuffer = Shader.PropertyToID("_IsVisibleBuffer");

	public static readonly int _MeshCount = Shader.PropertyToID("_MeshCount");

	public static readonly int _ArgsBufferSize = Shader.PropertyToID("_ArgsBufferSize");

	public static readonly int _MeshArgsBuffer = Shader.PropertyToID("_MeshArgsBuffer");

	public static readonly int _TotalInstanceCount = Shader.PropertyToID("_TotalInstanceCount");

	public static readonly int _CamViewProj = Shader.PropertyToID("_CamViewProj");

	public static readonly int _CamPosition = Shader.PropertyToID("_CamPosition");

	public static readonly int _InstanceDataBuffer = Shader.PropertyToID("_InstanceDataBuffer");

	public static readonly int _IsVisibleBufferSize = Shader.PropertyToID("_IsVisibleBufferSize");

	public static readonly int _MeshDataBuffer = Shader.PropertyToID("_MeshDataBuffer");

	public static readonly int _MeshArgsBufferSize = Shader.PropertyToID("_MeshArgsBufferSize");

	public static readonly int _PbdEnabledLocal = Shader.PropertyToID("_PbdEnabledLocal");

	public static readonly int unity_RenderingLayer = Shader.PropertyToID("unity_RenderingLayer");

	public static readonly int _BaseColorBlending = Shader.PropertyToID("_BaseColorBlending");

	public static readonly int _BaseColor = Shader.PropertyToID("_BaseColor");

	public static readonly int _GroundColorPower = Shader.PropertyToID("_GroundColorPower");

	public static readonly int _ZTest = Shader.PropertyToID("_ZTest");

	public static readonly int _SrcBlend = Shader.PropertyToID("_SrcBlend");

	public static readonly int _DstBlend = Shader.PropertyToID("_DstBlend");
}
