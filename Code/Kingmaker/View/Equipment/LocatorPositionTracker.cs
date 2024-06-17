using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View.Equipment;

public class LocatorPositionTracker : MonoBehaviour
{
	private const string LocatorObjectName = "Locator_WeaponWarheadFX_00";

	private Transform m_Locator;

	private Vector3 m_PreviousLocatorPosition;

	private Vector3 m_CurrentLocatorPosition;

	private Vector3 Forward => MoveDirection();

	private Vector3 Up => m_Locator.forward;

	public Quaternion Rotation
	{
		get
		{
			if (!(Forward != Vector3.zero))
			{
				return base.transform.rotation;
			}
			return Quaternion.LookRotation(Forward, Up);
		}
	}

	private void Awake()
	{
		m_Locator = base.transform.FindRecursive("Locator_WeaponWarheadFX_00");
		m_Locator = m_Locator.Or(base.transform);
		m_CurrentLocatorPosition = m_Locator.position;
		m_PreviousLocatorPosition = m_CurrentLocatorPosition - m_Locator.forward;
	}

	private void LateUpdate()
	{
		m_PreviousLocatorPosition = m_CurrentLocatorPosition;
		m_CurrentLocatorPosition = m_Locator.position;
	}

	private Vector3 MoveDirection()
	{
		return m_CurrentLocatorPosition - m_PreviousLocatorPosition;
	}
}
