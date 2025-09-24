using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DepthPyramidPassData : PassDataBase
{
	public ComputeShader Shader;

	public ComputeShaderKernelDescriptor Kernel;

	public Material CopyDepthMaterial;

	public TextureHandle DepthPyramidUAV;

	public TextureHandle CameraDepthBuffer;

	public int2 DepthPyramidTextureSize;

	public int DepthPyramidLodCount;

	public Vector4[] DepthPyramidMipRects;

	public Vector4 DepthPyramidSamplingRatio;

	public int[] SrcOffset = new int[4];

	public int[] DstOffset = new int[4];

	public Rect Viewport;

	public LocalKeyword READ_FROM_CAMERA_DEPTH;
}
