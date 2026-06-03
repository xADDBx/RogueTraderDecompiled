using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Code.View.UI;

public class BlinkWidget : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[Header("Settings")]
	[SerializeField]
	private float m_Duration = 0.22f;

	[SerializeField]
	private int m_Loops = 4;

	[SerializeField]
	private LoopType m_LoopType = LoopType.Yoyo;

	[SerializeField]
	private Ease m_Ease = Ease.InFlash;

	private Tweener m_BlinkTween;

	private void OnEnable()
	{
		m_CanvasGroup.alpha = 0f;
	}

	private void OnDisable()
	{
		m_BlinkTween?.Kill();
		m_CanvasGroup.alpha = 0f;
	}

	public void Blink()
	{
		m_BlinkTween?.Kill();
		m_CanvasGroup.alpha = 0f;
		m_BlinkTween = m_CanvasGroup.DOFade(1f, m_Duration);
		m_BlinkTween?.SetLoops(m_Loops, m_LoopType).SetEase(m_Ease).SetUpdate(isIndependentUpdate: true)
			.OnComplete(delegate
			{
				m_CanvasGroup.alpha = 0f;
			});
	}
}
