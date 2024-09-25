using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public static class CameraExtensions
{
	public static WaaaghAdditionalCameraData GetWaaaghAdditionalCameraData(this Camera camera)
	{
		GameObject gameObject = camera.gameObject;
		if (!gameObject.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
		{
			return gameObject.AddComponent<WaaaghAdditionalCameraData>();
		}
		return component;
	}

	public static VolumeFrameworkUpdateMode GetVolumeFrameworkUpdateMode(this Camera camera)
	{
		return camera.GetWaaaghAdditionalCameraData().VolumeFrameworkUpdateMode;
	}

	public static void SetVolumeFrameworkUpdateMode(this Camera camera, VolumeFrameworkUpdateMode mode)
	{
		WaaaghAdditionalCameraData waaaghAdditionalCameraData = camera.GetWaaaghAdditionalCameraData();
		if (waaaghAdditionalCameraData.VolumeFrameworkUpdateMode != mode)
		{
			waaaghAdditionalCameraData.VolumeFrameworkUpdateMode = mode;
			if (!waaaghAdditionalCameraData.RequiresVolumeFrameworkUpdate)
			{
				camera.UpdateVolumeStack(waaaghAdditionalCameraData);
			}
		}
	}

	public static void UpdateVolumeStack(this Camera camera)
	{
		WaaaghAdditionalCameraData waaaghAdditionalCameraData = camera.GetWaaaghAdditionalCameraData();
		camera.UpdateVolumeStack(waaaghAdditionalCameraData);
	}

	public static void UpdateVolumeStack(this Camera camera, WaaaghAdditionalCameraData cameraData)
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

	internal static void GetVolumeLayerMaskAndTrigger(this Camera camera, WaaaghAdditionalCameraData cameraData, out LayerMask layerMask, out Transform trigger)
	{
		if (cameraData != null)
		{
			layerMask = cameraData.VolumeLayerMask;
			trigger = ((cameraData.VolumeTrigger != null) ? cameraData.VolumeTrigger : camera.transform);
		}
		else
		{
			layerMask = 1;
			trigger = camera.transform;
		}
	}
}
