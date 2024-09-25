using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class SnapToTransform : MonoBehaviour
{
	[NonSerialized]
	private Transform m_TrackedTranform;

	[NonSerialized]
	private Vector3 m_Shift;

	private void Update()
	{
		if (m_TrackedTranform == null)
		{
			base.enabled = false;
		}
		else
		{
			base.transform.position = m_TrackedTranform.position + m_Shift;
		}
	}

	public void SetTrackedTransform([NotNull] Transform t)
	{
		m_TrackedTranform = t;
		m_Shift = base.transform.position - t.position;
		base.enabled = true;
	}

	private void OnDisable()
	{
		m_TrackedTranform = null;
	}
}
