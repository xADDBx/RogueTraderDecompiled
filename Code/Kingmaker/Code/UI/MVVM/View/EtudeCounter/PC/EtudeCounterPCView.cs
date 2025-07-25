using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.EtudeCounter.PC;

public class EtudeCounterPCView : EtudeCounterView
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_SubLabelButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ToggleExtraText();
		}));
	}

	public void SetEtudeCounterVisible(bool value)
	{
		base.gameObject.SetActive(value);
	}

	protected override void OnShow()
	{
		base.OnShow();
		base.gameObject.SetActive(value: true);
		m_ContainterTransform.DOComplete();
		m_ContainterTransform.DOKill();
		m_FadeAnimator.AppearAnimation();
		m_ContainterTransform.DOAnchorPosY(0f, 0.5f).From(isRelative: true).SetDelay(1f)
			.SetEase(Ease.InExpo);
		m_ContainterTransform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).From(isRelative: true).SetEase(Ease.InExpo);
		TweenTimeoutReset();
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_ContainterTransform.DOComplete();
		m_ContainterTransform.DOKill();
		m_FadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		TweenTimeoutReset();
	}
}
