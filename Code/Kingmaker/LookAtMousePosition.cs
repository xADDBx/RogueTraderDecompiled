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
	}

	private void LookAtMouse()
	{
		m_RayToMouse = m_Camera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(m_RayToMouse, out m_RayHit, 40f, 2359553))
		{
			base.transform.LookAt(m_RayHit.point);
		}
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.x = (xAxisExclude ? 0f : localEulerAngles.x);
		localEulerAngles.y = (yAxisExclude ? 0f : localEulerAngles.y);
		localEulerAngles.z = (zAxisExclude ? 0f : localEulerAngles.z);
		base.transform.localEulerAngles = localEulerAngles;
	}
}
