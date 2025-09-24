using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class ObjectMotionVectorsPassData : PassDataBase
{
	public TextureHandle MotionVectorsRT;

	public TextureHandle CameraDepthRT;

	public DrawingSettings DrawingSettings;

	public CullingResults CullingResults;

	public FilteringSettings FilteringSettings;
}
