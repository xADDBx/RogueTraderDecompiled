using UnityEngine;

namespace Kingmaker.Globalmap.View;

public class GlobalMapLightController : MonoBehaviour
{
	public Transform CameraRoot;

	public Transform MainLight;

	private void Update()
	{
		if (!CameraRoot || !MainLight)
		{
			PFLog.Default.Error("No camera root and/or directional light links.");
		}
		else
		{
			MainLight.LookAt(CameraRoot);
		}
	}
}
