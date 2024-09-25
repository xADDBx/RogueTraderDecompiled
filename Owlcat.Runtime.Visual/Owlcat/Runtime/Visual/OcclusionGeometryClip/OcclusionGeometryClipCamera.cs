using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

public class OcclusionGeometryClipCamera : MonoBehaviour, ICameraInfoProvider
{
	private Camera m_CachedCamera;

	private Transform m_CachedTransform;

	public CameraInfo CameraInfo
	{
		get
		{
			CameraInfo result = default(CameraInfo);
			result.viewProjectionMatrix = m_CachedCamera.projectionMatrix * m_CachedCamera.worldToCameraMatrix;
			result.viewMatrix = m_CachedTransform.worldToLocalMatrix;
			result.viewMatrixInverse = m_CachedTransform.localToWorldMatrix;
			return result;
		}
	}

	private void OnEnable()
	{
		m_CachedCamera = GetComponent<Camera>();
		m_CachedTransform = GetComponent<Transform>();
		System.RegisterCamera(this);
	}

	private void OnDisable()
	{
		System.UnregisterCamera(this);
	}
}
