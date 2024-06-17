using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class ColorPyramidBuffer
{
	public static int _Source = Shader.PropertyToID("_Source");

	public static int _SrcScaleBias = Shader.PropertyToID("_SrcScaleBias");

	public static int _SrcUvLimits = Shader.PropertyToID("_SrcUvLimits");

	public static int _SourceMip = Shader.PropertyToID("_SourceMip");

	public static int _ColorPyramidLodCount = Shader.PropertyToID("_ColorPyramidLodCount");
}
