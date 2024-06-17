using System;
using DG.Tweening;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

[RequireComponent(typeof(CanvasGroup))]
public class FadeAnimator : MonoBehaviour, IUIAnimator
{
	private CanvasGroup m_CanvasGroup;

	private bool? m_PermanentBlockRaycast;

	private UnityAction m_AppearAction;

	private UnityAction m_DisappearAction;

	private Tweener m_AppearTween;

	[SerializeField]
	[UsedImplicitly]
	private float m_AppearTime = 0.2f;

	[SerializeField]
	[UsedImplicitly]
	private Ease m_AppearAnimCurve = Ease.Linear;

	private Tweener m_DisappearTween;

	[SerializeField]
	[UsedImplicitly]
	private float m_DisappearTime = 0.2f;

	[SerializeField]
	[UsedImplicitly]
	private Ease m_DisappearAnimCurve = Ease.Linear;

	[SerializeField]
	[UsedImplicitly]
	private bool m_GameObjectAlwaysActive;

	private bool m_isInit;

	public CanvasGroup CanvasGroup => m_CanvasGroup = (m_CanvasGroup ? m_CanvasGroup : ((base.gameObject != null) ? this.EnsureComponent<CanvasGroup>() : null));

	public float AppearAnimationTime => m_AppearTime;

	public float DisappearAnimationTime => m_DisappearTime;

	public event Action OnAppearEvent;

	public event Action OnDisappearEvent;

	public void Initialize()
	{
		if (m_GameObjectAlwaysActive)
		{
			base.gameObject.SetActive(value: true);
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = 0f;
			CanvasGroup.blocksRaycasts = false;
		}
		m_PermanentBlockRaycast = null;
		m_isInit = true;
	}

	public void TryCreateTweens()
	{
		if (m_AppearTween != null && m_DisappearTween != null)
		{
			return;
		}
		m_AppearTween = CanvasGroup.Or(null)?.DOFade(1f, m_AppearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_AppearAnimCurve)
			.SetUpdate(isIndependentUpdate: true)
			.OnComplete(delegate
			{
				if (CanvasGroup != null)
				{
					CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
					CanvasGroup.alpha = 1f;
				}
				m_AppearAction?.Invoke();
			});
		m_DisappearTween = CanvasGroup.Or(null)?.DOFade(0f, m_DisappearTime).SetAutoKill(autoKillOnCompletion: false).SetEase(m_DisappearAnimCurve)
			.SetUpdate(isIndependentUpdate: true)
			.OnComplete(delegate
			{
				if (CanvasGroup != null)
				{
					CanvasGroup.alpha = 0f;
				}
				if (m_DisappearAction != null)
				{
					m_DisappearAction();
				}
				else if (!m_GameObjectAlwaysActive)
				{
					base.gameObject.SetActive(value: false);
				}
			});
		m_AppearTween.ChangeStartValue(0f);
	}

	public void AppearAnimation([CanBeNull] UnityAction action = null)
	{
		if (!m_isInit)
		{
			Initialize();
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
		}
		if (!m_GameObjectAlwaysActive)
		{
			if (!base.gameObject.activeInHierarchy && m_CanvasGroup != null)
			{
				m_CanvasGroup.alpha = 0.01f;
			}
			base.gameObject.SetActive(value: true);
		}
		if (!base.gameObject.activeInHierarchy)
		{
			if (m_CanvasGroup != null)
			{
				m_CanvasGroup.alpha = 1f;
			}
			return;
		}
		TryCreateTweens();
		if (m_AppearTween == null || m_DisappearTween == null)
		{
			m_isInit = false;
			Initialize();
		}
		m_AppearAction = action;
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
		}
		if (m_DisappearTween.IsPlaying())
		{
			m_DisappearTween.Pause();
		}
		if (CanvasGroup != null)
		{
			this.OnAppearEvent?.Invoke();
			m_AppearTween.ChangeStartValue(CanvasGroup.alpha);
			m_AppearTween.Play();
		}
	}

	public void DisappearAnimation([CanBeNull] UnityAction action = null)
	{
		if (!m_isInit)
		{
			Initialize();
		}
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			if (m_CanvasGroup != null)
			{
				m_CanvasGroup.alpha = 0.001f;
			}
			if (!m_GameObjectAlwaysActive)
			{
				base.gameObject.SetActive(value: false);
			}
			return;
		}
		TryCreateTweens();
		m_DisappearAction = action;
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
		}
		if (m_AppearTween.IsPlaying())
		{
			m_AppearTween.Pause();
		}
		if (CanvasGroup != null)
		{
			this.OnDisappearEvent?.Invoke();
			m_DisappearTween.ChangeStartValue(CanvasGroup.alpha);
			m_DisappearTween.Play();
		}
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

	public void BlockRaycastPermanent(bool state)
	{
		m_PermanentBlockRaycast = state;
	}

	public void OnDisable()
	{
		bool num = m_AppearTween != null && m_AppearTween.IsPlaying();
		bool flag = m_DisappearTween != null && m_DisappearTween.IsPlaying();
		if (num)
		{
			m_AppearTween.Complete(withCallbacks: true);
		}
		else if (flag)
		{
			m_DisappearTween.Complete(withCallbacks: true);
		}
		m_PermanentBlockRaycast = null;
		m_AppearTween?.Kill();
		m_DisappearTween?.Kill();
		m_AppearTween = null;
		m_DisappearTween = null;
		m_isInit = false;
	}

	public void SetAlwaysActive(bool state)
	{
		m_GameObjectAlwaysActive = state;
	}
}
