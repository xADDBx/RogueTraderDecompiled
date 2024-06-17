using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.UI.Sound;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

public class CharBAttentionMark : MonoBehaviour
{
	[SerializeField]
	[UsedImplicitly]
	private Image m_StaticMark;

	[SerializeField]
	[UsedImplicitly]
	private CanvasGroup m_AnimatedMark;

	private RectTransform m_AnimatedMarkRect;

	private bool m_IsInit;

	private DOTweenAnimation m_Animator;

	private void Init()
	{
		if (!m_IsInit)
		{
			m_AnimatedMarkRect = m_AnimatedMark.GetComponent<RectTransform>();
			m_StaticMark.gameObject.SetActive(value: false);
			m_Animator = m_StaticMark.GetComponent<DOTweenAnimation>();
			base.gameObject.SetActive(value: true);
			m_IsInit = true;
		}
	}

	public void Show(bool state)
	{
		Init();
		if (m_StaticMark.gameObject.activeSelf != state)
		{
			m_StaticMark.gameObject.SetActive(state);
			m_AnimatedMark.gameObject.SetActive(value: false);
			if (m_Animator != null && state)
			{
				m_Animator.DOPlay();
			}
		}
	}

	public void Blink()
	{
		Init();
		if (m_StaticMark.gameObject.activeSelf)
		{
			UISounds.Instance.Sounds.Systems.BlinkAttentionMark.Play();
			m_AnimatedMark.gameObject.SetActive(value: true);
			m_AnimatedMark.alpha = 1f;
			m_AnimatedMarkRect.localScale = new Vector3(0f, 0f, 1f);
			m_AnimatedMark.DOFade(0f, 0.4f).SetEase(Ease.OutSine).SetUpdate(isIndependentUpdate: true);
			m_AnimatedMarkRect.DOScale(new Vector3(5f, 5f, 1f), 0.4f).SetUpdate(isIndependentUpdate: true);
		}
	}
}
