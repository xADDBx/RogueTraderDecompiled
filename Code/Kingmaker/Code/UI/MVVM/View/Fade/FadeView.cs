using DG.Tweening;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Fade;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Fade;

public class FadeView : ViewBase<FadeVM>
{
	[SerializeField]
	[UsedImplicitly]
	private Image m_FadeImage;

	[SerializeField]
	[UsedImplicitly]
	[Range(0f, 1f)]
	public float FadeTimer = 0.3f;

	[SerializeField]
	[UsedImplicitly]
	private CanvasGroup m_Vignette;

	[SerializeField]
	[UsedImplicitly]
	[Range(0f, 1f)]
	private float m_CutSceneTimer = 0.3f;

	[CanBeNull]
	private Tween m_Tween;

	public void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.LoadingScreen.Subscribe(ShowLoadingScreen));
		AddDisposable(base.ViewModel.CutsceneOverlay.Subscribe(DoCutScene));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void DoCutScene(bool state)
	{
		m_Vignette.gameObject.SetActive(state);
	}

	public void ShowLoadingScreen(bool show)
	{
		CancelTween();
		if (show)
		{
			m_Tween = m_FadeImage.DOFade(1f, FadeTimer).OnComplete(delegate
			{
				base.ViewModel.SetStateShown();
			}).SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			m_Tween = m_FadeImage.DOFade(0f, FadeTimer).OnComplete(delegate
			{
				base.ViewModel.SetStateHidden();
			}).SetUpdate(isIndependentUpdate: true);
		}
	}

	private void CancelTween()
	{
		m_Tween?.Kill();
		m_Tween = null;
	}
}
