using UnityEngine;

namespace Kingmaker.Visual;

[RequireComponent(typeof(Camera))]
public class CameraOverlayConnector : MonoBehaviour
{
	private Camera m_Camera;

	private void Awake()
	{
		m_Camera = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		if ((bool)m_Camera)
		{
			CameraStackManager.Instance.AddCamera(m_Camera, CameraStackManager.CameraStackType.Main);
		}
	}

	private void OnDisable()
	{
		if ((bool)m_Camera)
		{
			CameraStackManager.Instance.RemoveCamera(m_Camera, CameraStackManager.CameraStackType.Main);
		}
	}
}
