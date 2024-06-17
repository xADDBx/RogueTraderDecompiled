using DG.Tweening;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Console;

public class ExplorationResourcePointConsoleView : ExplorationResourcePointBaseView
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private CanvasGroup m_HintCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_HintLabel;

	private Tweener m_ShowTween;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(base.HandleClick));
		AddDisposable(m_Button.OnFocusAsObservable().Subscribe(base.AnimateHover));
		AddDisposable(base.ViewModel.Name.Subscribe(SetHint));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_HintCanvasGroup.alpha = 0f;
		StopAllCoroutines();
		m_ShowTween?.Kill();
		m_ShowTween = null;
	}

	protected override void SetFocusImpl(bool value)
	{
		m_Button.SetFocus(value);
		m_HintCanvasGroup.alpha = 0f;
		StopAllCoroutines();
		DelayedInvoker.InvokeInTime(delegate
		{
			if (base.gameObject.activeInHierarchy)
			{
				m_ShowTween = m_HintCanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
				{
					UISounds.Instance.Sounds.Hint.HintShow.Play();
				}).SetUpdate(isIndependentUpdate: true)
					.SetAutoKill();
			}
		}, 0.3f);
	}

	private void SetHint(string hintText)
	{
		m_HintLabel.text = hintText;
	}
}
