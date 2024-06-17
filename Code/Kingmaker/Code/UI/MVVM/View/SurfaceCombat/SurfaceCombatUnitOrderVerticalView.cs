using System;
using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public abstract class SurfaceCombatUnitOrderVerticalView : SurfaceCombatUnitOrderView
{
	[SerializeField]
	private DisintegrationAnimator[] m_DisintegrationAnimator;

	protected override void DoInitialize()
	{
	}

	protected override Sequence GetHideAnimationInternal(Action completeAction)
	{
		Sequence sequence = DOTween.Sequence().Pause();
		if (base.ViewModel != null && base.ViewModel.IsCurrent.Value)
		{
			Tweener t = base.RectTransform.DOScale(base.RectTransform.localScale * 1.6f, m_AnimationTime).Pause().OnComplete(delegate
			{
				base.RectTransform.localScale = new Vector3(1f, 1f, 1f);
			});
			sequence.Join(t);
			Vector2 endValue = new Vector2(base.RectTransform.anchoredPosition.x, base.RectTransform.anchoredPosition.y - base.RectTransform.rect.height * 1.5f);
			Tweener t2 = base.RectTransform.DOAnchorPos(endValue, m_AnimationTime).Pause();
			sequence.Join(t2);
			Tweener t3 = base.CanvasGroup.DOFade(0f, m_AnimationTime).Pause().OnComplete(delegate
			{
				completeAction?.Invoke();
			});
			sequence.Append(t3);
		}
		else
		{
			DisintegrationAnimator[] disintegrationAnimator = m_DisintegrationAnimator;
			foreach (DisintegrationAnimator disintegrationAnimator2 in disintegrationAnimator)
			{
				sequence.Join(disintegrationAnimator2.DisappearAnimation().Pause());
			}
			Tweener t4 = base.CanvasGroup.DOFade(0f, m_AnimationTime).Pause().OnPlay(delegate
			{
			})
				.OnComplete(delegate
				{
					completeAction?.Invoke();
				});
			sequence.Join(t4);
		}
		return sequence;
	}

	protected override Sequence GetMoveAnimationInternal(Action completeAction, Vector2 targetPosition)
	{
		Sequence sequence = DOTween.Sequence().Pause().OnComplete(delegate
		{
			completeAction?.Invoke();
		})
			.SetUpdate(isIndependentUpdate: true);
		Tweener t = base.RectTransform.DOAnchorPos(targetPosition, m_AnimationTime).Pause().SetUpdate(isIndependentUpdate: true);
		sequence.Join(t);
		return sequence;
	}

	protected override Sequence GetShowAnimationInternal(Action completeAction, Vector2 targetPosition)
	{
		Sequence sequence = DOTween.Sequence().Pause().OnComplete(delegate
		{
			completeAction?.Invoke();
		});
		base.CanvasGroup.alpha = 0.01f;
		base.RectTransform.anchoredPosition = new Vector2(base.RectTransform.anchoredPosition.x, base.RectTransform.anchoredPosition.y + base.RectTransform.rect.height);
		Tweener t = base.RectTransform.DOAnchorPos(targetPosition, m_AnimationTime / 2f).Pause();
		DisintegrationAnimator[] disintegrationAnimator = m_DisintegrationAnimator;
		foreach (DisintegrationAnimator disintegrationAnimator2 in disintegrationAnimator)
		{
			sequence.Join(disintegrationAnimator2.AppearAnimation().Pause());
		}
		Tweener t2 = base.CanvasGroup.DOFade(1f, m_AnimationTime / 2f).Pause();
		sequence.Join(t2).Join(t);
		return sequence;
	}
}
