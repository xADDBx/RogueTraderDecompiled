using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Lighting;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public struct RenderingData
{
	public CameraData CameraData;

	public TimeData TimeData;

	public LightData LightData;

	public ShadowData ShadowData;

	public PostProcessingData PostProcessingData;

	public RenderGraph RenderGraph;

	public CullingResults CullingResults;

	public bool SupportsDynamicBatching;

	public PerObjectData PerObjectData;

	public bool IrsHasOpaques;

	public bool IrsHasOpaqueDistortions;

	public bool IrsHasTransparents;

	public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> CaptureActions;

	public LightCookieManager lightCookieManager;
}
