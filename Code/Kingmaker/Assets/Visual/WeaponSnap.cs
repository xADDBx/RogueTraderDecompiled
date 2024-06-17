using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Assets.Visual;

public class WeaponSnap : MonoBehaviour
{
	public Transform SnapTo;

	[NonSerialized]
	[CanBeNull]
	private Transform m_Transform;

	public void LateUpdate()
	{
		if (!(SnapTo == null))
		{
			if (m_Transform == null)
			{
				m_Transform = base.transform;
			}
			m_Transform.position = SnapTo.position;
			m_Transform.rotation = SnapTo.rotation;
		}
	}
}
