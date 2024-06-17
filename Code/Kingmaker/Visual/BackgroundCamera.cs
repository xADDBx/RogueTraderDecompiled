using UnityEngine;

namespace Kingmaker.Visual;

[RequireComponent(typeof(Camera))]
public class BackgroundCamera : MonoBehaviour
{
	public float proportionality;

	private Vector3 m_StartPos;

	private Camera m_Camera;

	private void Awake()
	{
		m_Camera = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		if ((bool)m_Camera)
		{
			CameraStackManager.Instance.AddCamera(m_Camera, CameraStackManager.CameraStackType.Background);
		}
		m_StartPos = base.transform.position;
	}

	private void LateUpdate()
	{
		Camera main = CameraStackManager.Instance.GetMain();
		if ((bool)main)
		{
			base.transform.SetPositionAndRotation(m_StartPos + main.transform.position * proportionality, main.transform.rotation);
			m_Camera.fieldOfView = main.fieldOfView;
		}
	}

	private void OnDisable()
	{
		if ((bool)m_Camera)
		{
			CameraStackManager.Instance.RemoveCamera(m_Camera, CameraStackManager.CameraStackType.Background);
		}
	}
}
