using System;
using DG.Tweening;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

public class MoveAnimator : MonoBehaviour, IUIAnimator
{
	[Serializable]
	public class MovePart
	{
		public float AppearPosition;

		public float DisappearPosition;

		public float AnimationTime = 0.2f;

		public AnimationCurve Curve;
	}

	public bool Relatively;

	public bool MoveByX;

	[ConditionalShow("MoveByX")]
	public MovePart MovePartX;

	public bool MoveByY;

	[ConditionalShow("MoveByY")]
	public MovePart MovePartY;

	private RectTransform m_RectTransform;

	private bool m_IsInit;

	private RectTransform RectTransform
	{
		get
		{
			if (!m_RectTransform)
			{
				m_RectTransform = GetComponent<RectTransform>();
			}
			return m_RectTransform;
		}
	}

	public void Initialize()
	{
		if (!m_IsInit)
		{
			Vector2 anchoredPosition = RectTransform.anchoredPosition;
			if (Relatively)
			{
				MovePartX.AppearPosition += anchoredPosition.x;
				MovePartX.DisappearPosition += anchoredPosition.x;
				MovePartY.AppearPosition += anchoredPosition.y;
				MovePartY.DisappearPosition += anchoredPosition.y;
			}
			if (MoveByX)
			{
				anchoredPosition.x = MovePartX.DisappearPosition;
			}
			if (MoveByY)
			{
				anchoredPosition.y = MovePartX.DisappearPosition;
			}
			RectTransform.anchoredPosition = anchoredPosition;
			m_IsInit = true;
		}
	}

	public void AppearAnimation(UnityAction action = null)
	{
		Initialize();
		DOTween.Kill(RectTransform);
		if (MoveByX)
		{
			RectTransform.DOAnchorPosX(MovePartX.AppearPosition, MovePartX.AnimationTime).SetEase(MovePartX.Curve).SetUpdate(isIndependentUpdate: true);
		}
		if (MoveByY)
		{
			RectTransform.DOAnchorPosY(MovePartY.AppearPosition, MovePartY.AnimationTime).SetEase(MovePartY.Curve).SetUpdate(isIndependentUpdate: true);
		}
		action?.Invoke();
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		Initialize();
		DOTween.Kill(RectTransform);
		if (MoveByX)
		{
			RectTransform.DOAnchorPosX(MovePartX.DisappearPosition, MovePartX.AnimationTime).SetEase(MovePartX.Curve).SetUpdate(isIndependentUpdate: true);
		}
		if (MoveByY)
		{
			RectTransform.DOAnchorPosY(MovePartY.DisappearPosition, MovePartY.AnimationTime).SetEase(MovePartY.Curve).SetUpdate(isIndependentUpdate: true);
		}
		action?.Invoke();
	}

	public void PlayAnimation(bool value, UnityAction action = null)
	{
		if (value)
		{
			AppearAnimation(action);
		}
		else
		{
			DisappearAnimation(action);
		}
	}

	public void SetDisappearPosition(Vector2 position)
	{
		MovePartX.DisappearPosition = position.x;
		MovePartY.DisappearPosition = position.y;
	}

	public void SetAppearPosition(Vector2 position)
	{
		MovePartX.AppearPosition = position.x;
		MovePartY.AppearPosition = position.y;
	}
}
