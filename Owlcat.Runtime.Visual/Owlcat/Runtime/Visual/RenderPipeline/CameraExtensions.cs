using Owlcat.Runtime.Visual.RenderPipeline.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public static class CameraExtensions
{
	public static OwlcatAdditionalCameraData GetUniversalAdditionalCameraData(this Camera camera)
	{
		GameObject gameObject = camera.gameObject;
		if (!gameObject.TryGetComponent<OwlcatAdditionalCameraData>(out var component))
		{
			return gameObject.AddComponent<OwlcatAdditionalCameraData>();
		}
		return component;
	}

	public static VolumeFrameworkUpdateMode GetVolumeFrameworkUpdateMode(this Camera camera)
	{
		return camera.GetUniversalAdditionalCameraData().VolumeFrameworkUpdateMode;
	}

	public static void SetVolumeFrameworkUpdateMode(this Camera camera, VolumeFrameworkUpdateMode mode)
	{
		OwlcatAdditionalCameraData universalAdditionalCameraData = camera.GetUniversalAdditionalCameraData();
		if (universalAdditionalCameraData.VolumeFrameworkUpdateMode != mode)
		{
			universalAdditionalCameraData.VolumeFrameworkUpdateMode = mode;
			if (!universalAdditionalCameraData.RequiresVolumeFrameworkUpdate)
			{
				camera.UpdateVolumeStack(universalAdditionalCameraData);
			}
		}
	}

	public static void UpdateVolumeStack(this Camera camera)
	{
		OwlcatAdditionalCameraData universalAdditionalCameraData = camera.GetUniversalAdditionalCameraData();
		camera.UpdateVolumeStack(universalAdditionalCameraData);
	}

	public static void UpdateVolumeStack(this Camera camera, OwlcatAdditionalCameraData cameraData)
	{
		if (!cameraData.RequiresVolumeFrameworkUpdate)
		{
			if (cameraData.VolumeStack == null)
			{
				cameraData.VolumeStack = VolumeManager.instance.CreateStack();
			}
			camera.GetVolumeLayerMaskAndTrigger(cameraData, out var layerMask, out var trigger);
			VolumeManager.instance.Update(cameraData.VolumeStack, trigger, layerMask);
		}
	}

	internal static void GetVolumeLayerMaskAndTrigger(this Camera camera, OwlcatAdditionalCameraData cameraData, out LayerMask layerMask, out Transform trigger)
	{
		layerMask = 1;
		trigger = camera.transform;
		if (cameraData != null)
		{
			layerMask = cameraData.VolumeLayerMask;
			trigger = ((cameraData.VolumeTrigger != null) ? cameraData.VolumeTrigger : trigger);
		}
		else if (camera.cameraType == CameraType.SceneView)
		{
			Camera main = Camera.main;
			OwlcatAdditionalCameraData component = null;
			if (main != null && main.TryGetComponent<OwlcatAdditionalCameraData>(out component))
			{
				layerMask = component.VolumeLayerMask;
			}
			trigger = ((component != null && component.VolumeTrigger != null) ? component.VolumeTrigger : trigger);
		}
	}
}
