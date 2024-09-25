using DG.Tweening;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Events;

namespace Kingmaker.UI.Common.Animations;

public class ColorAnimator : MonoBehaviour
{
	public _2dxFX_ColorChange ColorChangeFxObject;

	public float AnimationTime = 1.6f;

	public float SaturationAppearValue = 1f;

	public float BrightnessAppearValue = 1f;

	public float SaturationDisappearValue;

	public float BrightnessDisappearValue = 2f;

	private float m_ColorSaturation;

	private float m_ColorBrightness;

	private CanvasGroup m_CanvasGroup;

	private UnityAction m_Action;

	private bool? m_PermanentBlockRaycast;

	private bool m_AppearRequest;

	private bool m_DisappearRequest;

	private float ColorSaturation
	{
		get
		{
			return m_ColorSaturation;
		}
		set
		{
			m_ColorSaturation = Mathf.Clamp(value, 0f, 2f);
			ColorChangeFxObject._Saturation = m_ColorSaturation;
		}
	}

	private float ColorBrightness
	{
		get
		{
			return m_ColorBrightness;
		}
		set
		{
			m_ColorBrightness = Mathf.Clamp(value, 0f, 2f);
			ColorChangeFxObject._Saturation = m_ColorBrightness;
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
		if (ColorChangeFxObject == null)
		{
			UberDebug.LogError(base.gameObject?.ToString() + " doesn't have DestroyFx");
		}
	}

	public void AppearImmediately()
	{
		ColorSaturation = SaturationAppearValue;
		ColorBrightness = BrightnessAppearValue;
		CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
	}

	public void DisappearImmediately()
	{
		ColorSaturation = SaturationDisappearValue;
		ColorBrightness = BrightnessDisappearValue;
		CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
	}

	public Sequence AppearAnimation([CanBeNull] UnityAction action = null)
	{
		m_AppearRequest = true;
		Sequence sequence = DOTween.Sequence().Pause();
		if (m_DisappearRequest)
		{
			return sequence;
		}
		ColorBrightness = BrightnessDisappearValue;
		ColorSaturation = SaturationDisappearValue;
		sequence.Join(DOTween.To(() => ColorBrightness, delegate(float x)
		{
			ColorBrightness = x;
		}, BrightnessAppearValue, AnimationTime).SetEase(Ease.InOutQuad).SetUpdate(isIndependentUpdate: true)).Pause();
		sequence.Join(DOTween.To(() => ColorSaturation, delegate(float x)
		{
			ColorSaturation = x;
		}, SaturationAppearValue, AnimationTime).SetEase(Ease.InOutQuad).SetUpdate(isIndependentUpdate: true)
			.OnComplete(delegate
			{
				CanvasGroup.blocksRaycasts = !m_PermanentBlockRaycast.HasValue || m_PermanentBlockRaycast.Value;
				m_AppearRequest = false;
				if (m_DisappearRequest)
				{
					DisappearAnimation();
				}
				else
				{
					action?.Invoke();
				}
			})).Pause();
		return sequence;
	}

	public Sequence DisappearAnimation([CanBeNull] UnityAction action = null)
	{
		m_DisappearRequest = true;
		Sequence sequence = DOTween.Sequence();
		if (m_AppearRequest)
		{
			return sequence;
		}
		CanvasGroup.blocksRaycasts = m_PermanentBlockRaycast.HasValue && m_PermanentBlockRaycast.Value;
		ColorBrightness = BrightnessAppearValue;
		ColorSaturation = SaturationAppearValue;
		sequence.Join(DOTween.To(() => ColorBrightness, delegate(float x)
		{
			ColorBrightness = x;
		}, BrightnessDisappearValue, AnimationTime).SetUpdate(isIndependentUpdate: true));
		sequence.Join(DOTween.To(() => ColorSaturation, delegate(float x)
		{
			ColorSaturation = x;
		}, SaturationDisappearValue, AnimationTime).SetEase(Ease.InOutQuad).SetUpdate(isIndependentUpdate: true)
			.OnComplete(delegate
			{
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
