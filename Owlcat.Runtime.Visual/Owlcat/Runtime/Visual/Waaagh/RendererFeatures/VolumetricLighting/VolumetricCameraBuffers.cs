using System.Collections.Generic;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;

public static class VolumetricCameraBuffers
{
	private const int kMaxUnusedFramesCount = 4;

	private static Dictionary<Camera, VolumetricCameraBuffer> s_All = new Dictionary<Camera, VolumetricCameraBuffer>();

	private static List<Camera> s_Cleaned = new List<Camera>();

	public static VolumetricCameraBuffer EnsureCamera(Camera camera, Vector2Int cameraRenderPixelSize, int3 textureSize, int historyFramesCount)
	{
		if (!s_All.TryGetValue(camera, out var value))
		{
			value = new VolumetricCameraBuffer(camera, cameraRenderPixelSize, textureSize, historyFramesCount);
			s_All.Add(camera, value);
		}
		else if (value.HistoryFramesCount != historyFramesCount || math.any(value.RtSize != textureSize))
		{
			value.Dispose();
			value = new VolumetricCameraBuffer(camera, cameraRenderPixelSize, textureSize, historyFramesCount);
			s_All[camera] = value;
		}
		return value;
	}

	public static void CleanUnused()
	{
		s_Cleaned.Clear();
		foreach (VolumetricCameraBuffer value in s_All.Values)
		{
			if (value.Camera == null || (!value.Camera.isActiveAndEnabled && value.Camera.cameraType != CameraType.Preview && value.Camera.cameraType != CameraType.SceneView))
			{
				if (value.LastFrameId != FrameId.FrameCount)
				{
					value.LastFrameId = FrameId.FrameCount;
					value.UnusedFramesCount++;
				}
				if (value.UnusedFramesCount > 4)
				{
					s_Cleaned.Add(value.Camera);
				}
			}
		}
		foreach (Camera item in s_Cleaned)
		{
			s_All[item].Dispose();
			s_All.Remove(item);
		}
		s_Cleaned.Clear();
	}

	internal static void Cleanup()
	{
		foreach (KeyValuePair<Camera, VolumetricCameraBuffer> item in s_All)
		{
			item.Value.Dispose();
		}
		s_All.Clear();
	}
}
