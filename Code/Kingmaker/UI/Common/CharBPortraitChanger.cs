using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using JetBrains.Annotations;
using Kingmaker.UI.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

public class CharBPortraitChanger : MonoBehaviour
{
	[Serializable]
	public class FXedPortrait
	{
		public Image Portrait;

		public CanvasGroup CanvasGroup;
	}

	[SerializeField]
	[UsedImplicitly]
	private FXedPortrait m_Portrait1;

	[SerializeField]
	[UsedImplicitly]
	private FXedPortrait m_Portrait2;

	[SerializeField]
	[UsedImplicitly]
	private float m_AnimationSpeed = 0.5f;

	private Sprite m_CurrentPortrait;

	private FXedPortrait m_UpperImage;

	private FXedPortrait m_LowerImage;

	private TweenerCore<float, float, FloatOptions> m_Tween;

	private bool m_IsInit;

	private void Init()
	{
		if (!m_IsInit)
		{
			m_UpperImage = m_Portrait1;
			m_LowerImage = m_Portrait2;
			m_UpperImage.Portrait.transform.SetAsLastSibling();
			m_IsInit = true;
		}
	}

	public void Dispose()
	{
		m_CurrentPortrait = null;
	}

	public void SetNewPortrait(Sprite sprite, bool playAnimation = true, bool playSound = false)
	{
		if (!(sprite == m_CurrentPortrait))
		{
			m_CurrentPortrait = sprite;
			if (!m_IsInit)
			{
				Init();
				m_UpperImage.Portrait.sprite = sprite;
				m_LowerImage.Portrait.sprite = sprite;
			}
			else if (playAnimation)
			{
				StartAnimation(playSound);
			}
			else
			{
				m_UpperImage.Portrait.sprite = sprite;
			}
		}
	}

	private void StartAnimation(bool playSound)
	{
		if (m_Tween != null)
		{
			m_Tween.Kill();
			if (m_UpperImage.CanvasGroup.alpha < 0.5f)
			{
				m_UpperImage.CanvasGroup.alpha = 0.5f;
			}
		}
		if (playSound)
		{
			UISounds.Instance.Sounds.Chargen.ChargenPortraitChange.Play();
		}
		m_LowerImage.Portrait.sprite = m_CurrentPortrait;
		m_LowerImage.CanvasGroup.alpha = 1f;
		m_Tween = DOTween.To(() => m_UpperImage.CanvasGroup.alpha, delegate(float x)
		{
			m_UpperImage.CanvasGroup.alpha = x;
		}, 0f, m_AnimationSpeed).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_UpperImage = ((m_UpperImage == m_Portrait1) ? m_Portrait2 : m_Portrait1);
			m_LowerImage = ((m_LowerImage == m_Portrait1) ? m_Portrait2 : m_Portrait1);
			m_UpperImage.Portrait.transform.SetAsLastSibling();
			m_Tween = null;
		});
	}
}
