using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Owlcat.Runtime.Visual.Waaagh;

public static class WaaaghCameraBuffers
{
	private const int kMaxUnusedFramesCount = 4;

	private static Dictionary<Camera, WaaaghCameraBuffer> s_All = new Dictionary<Camera, WaaaghCameraBuffer>();

	private static List<Camera> s_Cleaned = new List<Camera>();

	public static WaaaghCameraBuffer EnsureCamera(ref CameraData cameraData)
	{
		bool flag = cameraData.PostProcessEnabled && cameraData.Antialiasing == AntialiasingMode.TemporalAntialiasing;
		bool isSSREnabled = cameraData.IsSSREnabled;
		VFXCameraBufferTypes vFXCameraBufferTypes = VFXManager.IsCameraBufferNeeded(cameraData.Camera);
		bool flag2 = vFXCameraBufferTypes.HasFlag(VFXCameraBufferTypes.Depth);
		bool flag3 = vFXCameraBufferTypes.HasFlag(VFXCameraBufferTypes.Color);
		int num = 0;
		int num2 = 0;
		if (isSSREnabled || flag3 || flag)
		{
			num = 1;
		}
		if (flag2)
		{
			num2 = 1;
		}
		Vector2Int cameraRenderPixelSize = new Vector2Int(cameraData.CameraTargetDescriptor.width, cameraData.CameraTargetDescriptor.height);
		if (!s_All.TryGetValue(cameraData.Camera, out var value))
		{
			value = new WaaaghCameraBuffer(cameraData.Camera, cameraRenderPixelSize, num, num2, flag);
			s_All.Add(cameraData.Camera, value);
		}
		else if (value.HistoryColorFramesCount != num || value.HistoryDepthFramesCount != num2 || !value.CheckResolution(cameraRenderPixelSize))
		{
			value.Dispose();
			value = new WaaaghCameraBuffer(cameraData.Camera, cameraRenderPixelSize, num, num2, flag);
			s_All[cameraData.Camera] = value;
		}
		value.m_TaaEnabled = flag;
		return value;
	}

	public static void CleanUnused()
	{
		s_Cleaned.Clear();
		foreach (WaaaghCameraBuffer value in s_All.Values)
		{
			if (value.Camera == null || (!value.Camera.isActiveAndEnabled && value.Camera.cameraType != CameraType.Preview && value.Camera.cameraType != CameraType.SceneView))
			{
				value.UnusedFramesCount++;
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
		foreach (KeyValuePair<Camera, WaaaghCameraBuffer> item in s_All)
		{
			item.Value.Dispose();
		}
		s_All.Clear();
	}
}
