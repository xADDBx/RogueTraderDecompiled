using System;
using DG.Tweening;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

public class SizeAnimator : MonoBehaviour, IUIAnimator
{
	[Serializable]
	public class MovePart
	{
		public float AppearSize;

		public float DisappearSize;
	}

	public bool SizeByX;

	[ConditionalShow("SizeByX")]
	public MovePart PartX;

	public bool SizeByY;

	[ConditionalShow("SizeByY")]
	public MovePart PartY;

	private RectTransform m_RectTransform;

	private bool m_isInit;

	private Tweener m_AppearTween;

	[SerializeField]
	private float m_AppearTime = 0.2f;

	[SerializeField]
	private Ease m_AppearAnimCurve = Ease.Linear;

	private UnityAction m_AppearAction;

	private Tweener m_DisappearTween;

	[SerializeField]
	private float m_DisappearTime = 0.2f;

	[SerializeField]
	private Ease m_DisappearAnimCurve = Ease.Linear;

	private UnityAction m_DisappearAction;

	private Action m_OnUpdateAction;

	private RectTransform RectTransform
	{
		get
		{
			RectTransform obj = m_RectTransform ?? GetComponent<RectTransform>();
			RectTransform result = obj;
			m_RectTransform = obj;
			return result;
		}
	}

	public event Action OnAppearEvent;

	public event Action OnDisappearEvent;

	public void Initialize()
	{
		if (!m_isInit && !(RectTransform == null))
		{
			Vector2 size = RectTransform.rect.size;
			if (SizeByX)
			{
				size.x = PartX.DisappearSize;
			}
			if (SizeByY)
			{
				size.y = PartX.DisappearSize;
			}
			RectTransform.sizeDelta = size;
			m_isInit = true;
		}
	}

	public void TryCreateTweens()
	{
		if (m_AppearTween != null && m_DisappearTween != null)
		{
			return;
		}
		Vector2 endValue = new Vector2(RectTransform.sizeDelta.x, RectTransform.sizeDelta.y);
		Vector2 endValue2 = new Vector2(RectTransform.sizeDelta.x, RectTransform.sizeDelta.y);
		if (SizeByX)
		{
			endValue.x = PartX.AppearSize;
			endValue2.x = PartX.DisappearSize;
		}
		if (SizeByY)
		{
			endValue.y = PartY.AppearSize;
			endValue2.y = PartY.DisappearSize;
		}
		m_AppearTween = RectTransform.DOSizeDelta(endValue, m_AppearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_AppearAnimCurve)
			.SetUpdate(isIndependentUpdate: true)
			.OnUpdate(delegate
			{
				m_OnUpdateAction?.Invoke();
			})
			.OnComplete(delegate
			{
				m_AppearAction?.Invoke();
			});
		m_DisappearTween = RectTransform.DOSizeDelta(endValue2, m_DisappearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_DisappearAnimCurve)
			.SetUpdate(isIndependentUpdate: true)
			.OnUpdate(delegate
			{
				m_OnUpdateAction?.Invoke();
			})
			.OnComplete(delegate
			{
				if (m_DisappearAction != null)
				{
					m_DisappearAction();
				}
			});
		m_AppearTween.ChangeStartValue(0f);
	}

	public void AppearAnimation(UnityAction action = null)
	{
		if (!m_isInit)
		{
			Initialize();
		}
		if (base.gameObject.activeInHierarchy)
		{
			TryCreateTweens();
			if (m_AppearTween == null || m_DisappearTween == null)
			{
				m_isInit = false;
				Initialize();
			}
			m_AppearAction = action;
			if (m_DisappearTween.IsPlaying())
			{
				m_DisappearTween.Pause();
			}
			this.OnAppearEvent?.Invoke();
			m_AppearTween.ChangeStartValue(m_RectTransform.sizeDelta);
			m_AppearTween.Play();
		}
	}

	public void DisappearAnimation(UnityAction action = null)
	{
		if (!m_isInit)
		{
			Initialize();
		}
		if (base.gameObject.activeInHierarchy)
		{
			TryCreateTweens();
			m_DisappearAction = action;
			if (m_AppearTween.IsPlaying())
			{
				m_AppearTween.Pause();
			}
			this.OnDisappearEvent?.Invoke();
			m_DisappearTween.ChangeStartValue(RectTransform.sizeDelta);
			m_DisappearTween.Play();
		}
	}

	public void SetAppearSize()
	{
		Vector2 sizeDelta = new Vector2(RectTransform.sizeDelta.x, RectTransform.sizeDelta.y);
		if (SizeByX)
		{
			sizeDelta.x = PartX.AppearSize;
		}
		if (SizeByY)
		{
			sizeDelta.y = PartY.AppearSize;
		}
		RectTransform.sizeDelta = sizeDelta;
	}

	public void SetDisappearSize()
	{
		Vector2 sizeDelta = new Vector2(RectTransform.sizeDelta.x, RectTransform.sizeDelta.y);
		if (SizeByX)
		{
			sizeDelta.x = PartX.DisappearSize;
		}
		if (SizeByY)
		{
			sizeDelta.y = PartY.DisappearSize;
		}
		RectTransform.sizeDelta = sizeDelta;
	}

	public void SetOnUpdateAction(Action onUpdateAction)
	{
		m_OnUpdateAction = onUpdateAction;
	}
}
