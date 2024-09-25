using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class DepthPyramidBuffer
{
	public static int _SrcOffsetAndLimit = Shader.PropertyToID("_SrcOffsetAndLimit");

	public static int _DstOffset = Shader.PropertyToID("_DstOffset");

	public static int _DepthPyramidMipRects = Shader.PropertyToID("_DepthPyramidMipRects");

	public static int _DepthPyramidLodCount = Shader.PropertyToID("_DepthPyramidLodCount");

	public static int _DepthPyramidSamplingRatio = Shader.PropertyToID("_DepthPyramidSamplingRatio");

	public static int _CameraDepthUAV = Shader.PropertyToID("_CameraDepthUAV");

	public static int _CameraDepthUAVSize = Shader.PropertyToID("_CameraDepthUAVSize");
}
