using DG.Tweening;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Console;

public class ExplorationPointOfInterestConsoleView : ExplorationPointOfInterestBaseView
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
		AddDisposable(m_Button.OnFocusAsObservable().Subscribe(base.AnimateHover));
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

	protected override void SetHintImpl(string stateText)
	{
		string text = ((base.ViewModel.PointOfInterestBlueprintType.Value is BlueprintPointOfInterestGroundOperation) ? base.ViewModel.Name.Value : $"{stateText}\n<b><color=#{ColorUtility.ToHtmlStringRGB(m_HeaderColor)}>{base.ViewModel.Name}");
		m_HintLabel.text = text;
	}
}
