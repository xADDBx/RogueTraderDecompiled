using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public struct RenderingData
{
	public CullingResults CullResults;

	public CameraData CameraData;

	public LightingData LightData;

	public ShadowingData ShadowData;

	public PostProcessingData PostProcessingData;

	public PerObjectData PerObjectData;

	public bool SupportsDynamicBatching;

	public ColorSpace ColorSpace;

	public RenderPath RenderPath;
}
