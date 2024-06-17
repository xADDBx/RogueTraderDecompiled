using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.UI.Common.Animations;

public class CompassAnimator : MonoBehaviour
{
	[Serializable]
	private class CompassAnimationElement
	{
		[SerializeField]
		private RectTransform m_Transform;

		[SerializeField]
		private int m_RotationFramesCount;

		[SerializeField]
		private int m_DelayFramesCount;

		private int m_FrameId;

		public void OnEnableImpl()
		{
			m_FrameId = 0;
			m_Transform.localRotation = Quaternion.identity;
		}

		public void UpdateImpl()
		{
			m_FrameId = (m_FrameId + 1) % (m_RotationFramesCount + m_DelayFramesCount);
			if (m_FrameId < m_RotationFramesCount)
			{
				float y = 360f * (float)m_FrameId / (float)m_RotationFramesCount;
				m_Transform.localRotation = Quaternion.Euler(0f, y, 0f);
			}
		}
	}

	[SerializeField]
	private List<CompassAnimationElement> m_RotationElements = new List<CompassAnimationElement>();

	private void OnEnable()
	{
		foreach (CompassAnimationElement rotationElement in m_RotationElements)
		{
			rotationElement.OnEnableImpl();
		}
	}

	private void Update()
	{
		foreach (CompassAnimationElement rotationElement in m_RotationElements)
		{
			rotationElement.UpdateImpl();
		}
	}
}
