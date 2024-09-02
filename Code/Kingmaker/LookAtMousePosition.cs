using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public class LookAtMousePosition : MonoBehaviour
{
	private Ray m_RayToMouse;

	private RaycastHit m_RayHit;

	private Camera m_Camera;

	[SerializeField]
	private bool xAxisExclude;

	[SerializeField]
	private bool yAxisExclude;

	[SerializeField]
	private bool zAxisExclude;

	[SerializeField]
	[Tooltip("Smooth rotation between current view vector and mouse position")]
	private bool smooth;

	[SerializeField]
	[Tooltip("Time for rotation. Required only in smooth mode")]
	private float rotationSpeed = 0.1f;

	private Vector3 m_LookAtTargetPoint;

	private void Awake()
	{
		m_Camera = Game.GetCamera();
		if (m_Camera == null)
		{
			UberDebug.LogError("Camera is null");
			base.enabled = false;
		}
	}

	private void LateUpdate()
	{
		LookAtMouse();
		LockAxis();
	}

	private void LookAtMouse()
	{
		m_RayToMouse = m_Camera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(m_RayToMouse, out m_RayHit, 40f, 2359553))
		{
			m_LookAtTargetPoint = m_RayHit.point;
		}
		if (!smooth)
		{
			base.transform.LookAt(m_RayHit.point);
			return;
		}
		Quaternion rotation = base.transform.rotation;
		base.transform.LookAt(m_LookAtTargetPoint);
		Quaternion rotation2 = base.transform.rotation;
		base.transform.rotation = rotation;
		base.transform.rotation = Quaternion.Lerp(rotation, rotation2, rotationSpeed * Time.deltaTime);
	}

	private void LockAxis()
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.x = (xAxisExclude ? 0f : localEulerAngles.x);
		localEulerAngles.y = (yAxisExclude ? 0f : localEulerAngles.y);
		localEulerAngles.z = (zAxisExclude ? 0f : localEulerAngles.z);
		base.transform.localEulerAngles = localEulerAngles;
	}
}
