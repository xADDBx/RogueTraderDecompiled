using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

public class DestroyAnimator : MonoBehaviour, IUIAnimator
{
	public _2dxFX_DestroyedFX DestroyFxObject;

	private float m_DestroyedFx;

	private CanvasGroup m_CanvasGroup;

	private UnityAction m_Action;

	private bool? m_PermanentBlockRaycast;

	private bool m_AppearRequest;

	private bool m_DisppearRequest;

	private float DestroyedFx
	{
		get
		{
			return m_DestroyedFx;
		}
		set
		{
			m_DestroyedFx = Mathf.Clamp(value, 0f, 1f);
			bool activeChange = m_DestroyedFx != 0f;
			DestroyFxObject.ActiveChange = activeChange;
			DestroyFxObject.SetDestroyed(m_DestroyedFx);
		}
	}

	private CanvasGroup CanvasGroup
	{
		get
		{
			if (m_CanvasGroup != null)
			{
				return m_CanvasGroup;
			}
			if (GetComponent<CanvasGroup>() == null)
			{
				base.gameObject.AddComponent<CanvasGroup>();
			}
			m_CanvasGroup = GetComponent<CanvasGroup>();
			return m_CanvasGroup;
		}
	}

	public void Initialize()
	{
		CanvasGroup.blocksRaycasts = false;
		m_PermanentBlockRaycast = null;
		if (DestroyFxObject == null)
		{
			PFLog.UI.Error(base.gameObject?.ToString() + " doesn't have DestroyFx");
		}
	}

	public void AppearImmediately()
	{
		base.gameObject.SetActive(value: true);
		DestroyedFx = 0f;
		CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
	}

	public void DisappearImmediately()
	{
		DestroyedFx = 1f;
		CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
		base.gameObject.SetActive(value: false);
	}

	public void AppearAnimation([CanBeNull] UnityAction action = null)
	{
		m_AppearRequest = true;
		if (m_DisppearRequest)
		{
			return;
		}
		base.gameObject.SetActive(value: true);
		DestroyedFx = 1f;
		DOTween.To(() => DestroyedFx, delegate(float x)
		{
			DestroyedFx = x;
		}, 0f, 1.6f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
			m_AppearRequest = false;
			if (m_DisppearRequest)
			{
				DisappearAnimation();
			}
			else
			{
				action?.Invoke();
			}
		});
	}

	public void DisappearAnimation([CanBeNull] UnityAction action = null)
	{
		m_DisppearRequest = true;
		if (m_AppearRequest)
		{
			return;
		}
		CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
		DestroyedFx = 0f;
		DOTween.To(() => DestroyedFx, delegate(float x)
		{
			DestroyedFx = x;
		}, 1f, 1.6f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			m_DisppearRequest = false;
			if (m_AppearRequest)
			{
				AppearAnimation();
			}
			else
			{
				action?.Invoke();
			}
		});
	}

	public void BlockRaycastPermanent(bool state)
	{
		m_PermanentBlockRaycast = state;
	}
}
