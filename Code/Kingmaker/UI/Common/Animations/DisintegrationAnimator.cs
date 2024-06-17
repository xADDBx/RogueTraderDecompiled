using DG.Tweening;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kingmaker.UI.Common.Animations;

public class DisintegrationAnimator : MonoBehaviour
{
	public _2dxFX_DesintegrationFX DisintegrationFxObject;

	public float AppearValue;

	public float DisappearValue = 1f;

	public float AnimationTime = 1.6f;

	private float m_DisintegrationFx;

	private Image m_Image;

	private CanvasGroup m_CanvasGroup;

	private UnityAction m_Action;

	private bool? m_PermanentBlockRaycast;

	private bool m_AppearRequest;

	private bool m_DisappearRequest;

	private float DisintegrationFx
	{
		get
		{
			return m_DisintegrationFx;
		}
		set
		{
			m_DisintegrationFx = Mathf.Clamp(value, 0f, 1f);
			DisintegrationFxObject.Desintegration = m_DisintegrationFx;
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

	private Image Image
	{
		get
		{
			if (m_Image != null)
			{
				return m_Image;
			}
			m_Image = GetComponent<Image>();
			return m_Image;
		}
	}

	public void Initialize()
	{
		EnableEffects(state: false);
		CanvasGroup.blocksRaycasts = false;
		m_PermanentBlockRaycast = null;
		if (DisintegrationFxObject == null)
		{
			UberDebug.LogError(base.gameObject?.ToString() + " doesn't have DestroyFx");
		}
	}

	private void EnableEffects(bool state)
	{
		DisintegrationFxObject.enabled = state;
		if (!state)
		{
			Image.material = null;
		}
	}

	public void AppearImmediately()
	{
		DisintegrationFx = AppearValue;
		CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
		EnableEffects(state: false);
	}

	public void DisappearImmediately()
	{
		DisintegrationFx = DisappearValue;
		CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
		EnableEffects(state: false);
	}

	public Sequence AppearAnimation([CanBeNull] UnityAction action = null)
	{
		m_AppearRequest = true;
		Sequence sequence = DOTween.Sequence().Pause();
		if (m_DisappearRequest)
		{
			return sequence;
		}
		DisintegrationFxObject.enabled = true;
		EnableEffects(state: true);
		DisintegrationFx = DisappearValue;
		sequence.Join(DOTween.To(() => DisintegrationFx, delegate(float x)
		{
			DisintegrationFx = x;
		}, AppearValue, AnimationTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
			EnableEffects(state: false);
			m_AppearRequest = false;
			if (m_DisappearRequest)
			{
				DisappearAnimation();
			}
			else
			{
				action?.Invoke();
			}
		}));
		return sequence;
	}

	public Sequence DisappearAnimation([CanBeNull] UnityAction action = null)
	{
		m_DisappearRequest = true;
		Sequence sequence = DOTween.Sequence().Pause();
		if (m_AppearRequest)
		{
			return sequence;
		}
		EnableEffects(state: true);
		CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
		DisintegrationFx = AppearValue;
		sequence.Join(DOTween.To(() => DisintegrationFx, delegate(float x)
		{
			DisintegrationFx = x;
		}, DisappearValue, AnimationTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			EnableEffects(state: false);
			m_DisappearRequest = false;
			if (m_AppearRequest)
			{
				AppearAnimation();
			}
			else
			{
				action?.Invoke();
			}
		}));
		return sequence;
	}

	public void BlockRaycastPermanent(bool state)
	{
		m_PermanentBlockRaycast = state;
	}
}
