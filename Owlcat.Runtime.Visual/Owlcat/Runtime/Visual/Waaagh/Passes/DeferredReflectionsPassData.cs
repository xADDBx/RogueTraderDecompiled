using Owlcat.Runtime.Visual.Waaagh.BilateralUpsample;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DeferredReflectionsPassData : PassDataBase
{
	public TextureHandle CameraColorRT;

	public TextureHandle CameraDeferredReflectionsRT;

	public TextureHandle CameraDepthRT;

	public TextureHandle CameraDepthCopytRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraTranslucencyRT;

	public TextureHandle SsrRT;

	public TextureHandle SsrUpsampledRT;

	public int2 UpsampledSize;

	public NativeArray<VisibleReflectionProbe> VisibleReflectionProbes;

	public Material DeferredReflectionsMaterial;

	public bool SsrEnabled;

	public bool SsrNeedUpsamplePass;

	public ComputeShader BilateralUpsampleCS;

	public ComputeShaderKernelDescriptor BilateralUpSampleColorKernel;

	internal ShaderVariablesBilateralUpsample ShaderVariablesBilateralUpsample;

	public ColorSpace ActiveColorSpace;
}
