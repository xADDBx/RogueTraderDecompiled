using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.UI.Legacy.MainMenuUI;

public class MainMenuCamera : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private MainMenuCameraAnchors m_CameraAnchors;

	[SerializeField]
	[UsedImplicitly]
	private MainMenuCameraController m_CameraController;

	[SerializeField]
	[UsedImplicitly]
	private Camera m_Camera;

	private float m_CurrentCameraAspect;

	public void Start()
	{
		m_CameraController.SetCameraPosition(m_CameraAnchors.MainPosition);
	}

	private void Update()
	{
		if (!(m_Camera == null) && m_Camera.aspect != m_CurrentCameraAspect)
		{
			CalculateCameraPositions();
		}
	}

	public void CalculateCameraPositions()
	{
		PFLog.MainMenuLight.Log("MainMenuBoard.CalculateCameraPositions()");
		m_CameraAnchors.CalculateMainPosition(m_Camera);
		m_CurrentCameraAspect = m_Camera.aspect;
		m_Camera.transform.SetPositionAndRotation(m_CameraAnchors.MainPosition.position, m_CameraAnchors.MainPosition.rotation);
	}
}
