using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Pause;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Pause;

public class PauseNotificationBaseView : ViewBase<PauseNotificationVM>
{
	[SerializeField]
	private CanvasGroup m_PauseBlock;

	[SerializeField]
	private TextMeshProUGUI m_PauseText;

	private Tweener m_Animation;

	protected override void BindViewImplementation()
	{
		m_PauseText.text = UIStrings.Instance.CommonTexts.Pause;
		ChangeAlphaPause(base.ViewModel.IsPaused);
		AddDisposable(base.ViewModel.ShowPause.Subscribe(PlayPause));
		AddDisposable(base.ViewModel.ChangeAlphaPause.Subscribe(ChangeAlphaPause));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void PlayPause(bool state)
	{
		if (state)
		{
			UISounds.Instance.Sounds.Systems.PauseSound.Play();
		}
		m_PauseBlock.interactable = state;
		m_PauseBlock.blocksRaycasts = state;
		m_Animation?.Kill();
		m_Animation = m_PauseBlock.DOFade(state ? 1f : 0f, 0.2f).SetUpdate(isIndependentUpdate: true).SetDelay(state ? 0.2f : 0.0001f);
	}

	public void ChangeAlphaPause(bool state)
	{
		m_PauseBlock.interactable = state;
		m_PauseBlock.blocksRaycasts = state;
		m_PauseBlock.alpha = (state ? 1f : 0f);
	}
}
