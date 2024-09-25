using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Epilog;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Epilog;

public class EpilogBaseView : ViewBase<EpilogVM>, IInitializable
{
	[Header("Window")]
	[SerializeField]
	private FadeAnimator m_WindowAnimator;

	[Header("Picture page")]
	[SerializeField]
	private CharBPortraitChanger m_Portrait;

	[Header("Cue")]
	[SerializeField]
	private EpilogCueBaseView m_Cue;

	[SerializeField]
	protected ScrollRectExtended m_CueScrollRect;

	[Header("Title")]
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[Header("Background")]
	[SerializeField]
	private Image m_BackgroundImage;

	[SerializeField]
	private VideoPlayerHelper m_BackgroundVideo;

	[SerializeField]
	private FadeAnimator m_BackgroundAnimator;

	private bool m_ContentRefreshing;

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			base.gameObject.SetActive(value: false);
			m_WindowAnimator.Initialize();
			m_BackgroundAnimator.Initialize();
			m_BackgroundVideo.Initialize();
			m_Cue.Initialize(BlueprintRoot.Instance.UIConfig.DialogColors);
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Show();
		AddDisposable(base.ViewModel.Portrait.Subscribe(delegate(Sprite p)
		{
			m_Portrait.SetNewPortrait(p);
		}));
		AddDisposable(base.ViewModel.Title.Subscribe(delegate
		{
			OnTitleChanged();
		}));
		AddDisposable(base.ViewModel.BackgroundClip.Subscribe(delegate
		{
			OnBackgroundChanged();
		}));
		AddDisposable(base.ViewModel.BackgroundSprite.Subscribe(delegate
		{
			OnBackgroundChanged();
		}));
		AddDisposable(base.ViewModel.Cues.ObserveAdd().Subscribe(delegate
		{
			OnCuesChanged();
		}));
		AddDisposable(base.ViewModel.Answers.Subscribe(delegate
		{
			OnAnswersChanged();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	protected virtual void OnAnswersChanged()
	{
	}

	private void OnTitleChanged()
	{
		SetTitle();
	}

	private void OnBackgroundChanged()
	{
		m_BackgroundAnimator.DisappearAnimation(delegate
		{
			SetBackground();
			m_BackgroundAnimator.AppearAnimation();
		});
	}

	private void OnCuesChanged()
	{
		CueVM cueVM = base.ViewModel.Cues.LastOrDefault();
		m_Cue.gameObject.SetActive(cueVM != null);
		if (cueVM != null)
		{
			m_Cue.Bind(cueVM);
		}
		m_CueScrollRect.ScrollToTop();
	}

	private void SetTitle()
	{
		m_Title.text = base.ViewModel.Title.Value;
	}

	private void SetBackground()
	{
		VideoClip value = base.ViewModel.BackgroundClip.Value;
		m_BackgroundVideo.gameObject.SetActive(value != null);
		if (m_BackgroundVideo.VideoClip != value)
		{
			m_BackgroundVideo.Stop();
			m_BackgroundVideo.SetClip(value, SoundStateType.Video, prepareVideo: false, base.ViewModel.SoundStart.Value, base.ViewModel.SoundStop.Value);
		}
		Sprite value2 = base.ViewModel.BackgroundSprite.Value;
		m_BackgroundImage.gameObject.SetActive(value2 != null);
		m_BackgroundImage.sprite = value2;
		if ((bool)value2 && m_BackgroundImage.TryGetComponent<AspectRatioFitter>(out var component))
		{
			component.aspectRatio = value2.rect.width / value2.rect.height;
		}
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_WindowAnimator.AppearAnimation();
		UISounds.Instance.Sounds.Dialogue.BookOpen.Play();
	}

	private void Hide()
	{
		m_WindowAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		UISounds.Instance.Sounds.Dialogue.BookClose.Play();
	}

	protected virtual void Confirm()
	{
		base.ViewModel.Answers.Value.FirstOrDefault()?.OnChooseAnswer();
	}
}
