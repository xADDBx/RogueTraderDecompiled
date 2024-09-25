using System.Collections.Generic;
using Owlcat.Runtime.Visual.Lighting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Shadows;

public class ShadowmapEntry
{
	private static Stack<ShadowmapEntry> s_Cache = new Stack<ShadowmapEntry>();

	public LightType LightType;

	public Rect ScreenRect;

	public LightShadows Shadows;

	public Matrix4x4 LocalToWorldMatrix;

	public float ShadowNearPlane;

	public float Range;

	public int LightIndex;

	public Rect Viewport;

	public int[] MatrixIndices = new int[4];

	public ShadowSplitData[] Splits = new ShadowSplitData[4];

	public ShadowFlags ShadowFlags;

	public Vector4 ScaleOffset;

	public int ScreenSpaceMask;

	public float DepthBias;

	public float NormalBias;

	private ShadowmapEntry()
	{
	}

	private void SetLight(in VisibleLight light, in RenderingData renderingData)
	{
		LightType = light.lightType;
		ScreenRect = light.screenRect;
		Shadows = light.light.shadows;
		LocalToWorldMatrix = light.localToWorldMatrix;
		ShadowNearPlane = light.light.shadowNearPlane;
		Range = light.range;
		DepthBias = renderingData.ShadowData.DepthBias;
		NormalBias = renderingData.ShadowData.NormalBias;
		if (light.light != null)
		{
			light.light.TryGetComponent<OwlcatAdditionalLightData>(out var component);
			if (component != null && !component.UsePipelineSettings)
			{
				DepthBias = light.light.shadowBias;
				NormalBias = light.light.shadowNormalBias;
			}
		}
	}

	public static ShadowmapEntry Get(in VisibleLight light, in RenderingData renderingData)
	{
		ShadowmapEntry shadowmapEntry = null;
		if (s_Cache.Count > 0)
		{
			shadowmapEntry = s_Cache.Pop();
		}
		if (shadowmapEntry == null)
		{
			shadowmapEntry = new ShadowmapEntry();
		}
		shadowmapEntry.SetLight(in light, in renderingData);
		return shadowmapEntry;
	}

	public static void Release(ShadowmapEntry entry)
	{
		s_Cache.Push(entry);
	}
}
