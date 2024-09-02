using System;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.CustomVideoPlayer;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.Base;

public class CustomUIVideoPlayerBaseView : ViewBase<CustomUIVideoPlayerVM>
{
	[Header("Main Content")]
	[SerializeField]
	protected VideoPlayerHelper m_Video;

	[SerializeField]
	private Image m_VidePreview;

	[Header("Play Pause Button")]
	[SerializeField]
	protected OwlcatMultiButton m_PlayPauseBigButton;

	[Header("Progress Bar")]
	[SerializeField]
	private RectTransform m_VideoProgressParent;

	[SerializeField]
	private RectTransform m_VideoProgressTransform;

	[SerializeField]
	private TextMeshProUGUI m_VideoProgressTime;

	[Header("Show Interface Settings")]
	[SerializeField]
	private float m_HideInterfaceDelay = 1f;

	[SerializeField]
	private FadeAnimator m_PlayPauseBigButtonFadeAnimator;

	[SerializeField]
	private FadeAnimator m_VideoProgressFadeAnimator;

	private Tweener m_VideoProgressTweener;

	private IDisposable m_VideoTimePositionDisposable;

	protected bool VideoIsStarted;

	protected readonly BoolReactiveProperty VideoIsPlaying = new BoolReactiveProperty();

	private bool m_InterfaceIsShowed;

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			m_Video.Or(null)?.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.ChangeVideo, delegate
		{
			if (!(m_Video.Or(null)?.VideoClip == base.ViewModel.Video))
			{
				m_Video.Or(null)?.Stop();
				m_Video.Or(null)?.SetClip(base.ViewModel.Video, SoundStateType.VideoDLCManager, prepareVideo: true, base.ViewModel.SoundStart, base.ViewModel.SoundStop);
				ResetVideo();
			}
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.ResetVideoCommand, delegate
		{
			ResetVideo();
		}));
		AddDisposable(VideoIsPlaying.Subscribe(delegate(bool value)
		{
			m_PlayPauseBigButton.Or(null)?.SetActiveLayer(value ? 1 : 0);
		}));
		if (m_Video.Or(null)?.VideoPlayer != null)
		{
			m_Video.VideoPlayer.loopPointReached += ResetVideo;
		}
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.LateUpdateAsObservable(), delegate
		{
			UpdateState();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_VideoProgressTweener?.Kill();
		m_VideoProgressTweener = null;
		if (m_Video.Or(null)?.VideoPlayer != null)
		{
			m_Video.VideoPlayer.loopPointReached -= ResetVideo;
		}
	}

	private void ResetVideo(VideoPlayer vp = null)
	{
		m_Video.Or(null)?.Stop();
		m_VideoProgressTweener?.Kill();
		m_VideoProgressTweener = null;
		m_VidePreview.sprite = base.ViewModel.PreviewArt;
		m_VidePreview.Or(null)?.gameObject.SetActive(value: true);
		m_VideoProgressTransform.sizeDelta = new Vector2(0f, m_VideoProgressTransform.rect.height);
		VideoIsStarted = false;
		VideoIsPlaying.Value = false;
		ShowHideInterface(state: true);
	}

	private void UpdateState()
	{
		if (!(base.ViewModel.Video == null))
		{
			double length = base.ViewModel.Video.length;
			double time = m_Video.VideoPlayer.time;
			m_VideoProgressTime.text = FormatTime(time) + "/" + FormatTime(length);
			SetLoadingProgress((float)time / (float)length);
		}
	}

	private void SetLoadingProgress(float virtualProgress)
	{
		virtualProgress = Mathf.Clamp01(virtualProgress);
		float progressWidth = m_VideoProgressParent.rect.width;
		float startValue = ((m_VideoProgressTransform.rect.width > 0f && progressWidth > 0f) ? (m_VideoProgressTransform.rect.width / progressWidth) : 0f);
		m_VideoProgressTweener?.Kill();
		m_VideoProgressTweener = DOTween.To(delegate
		{
			m_VideoProgressTransform.sizeDelta = new Vector2(virtualProgress * progressWidth, m_VideoProgressTransform.rect.height);
		}, startValue, virtualProgress, 0.5f).SetEase(Ease.Linear);
	}

	private string FormatTime(double timeInSeconds)
	{
		int num = Mathf.FloorToInt((float)timeInSeconds / 60f);
		int num2 = Mathf.FloorToInt((float)timeInSeconds % 60f);
		return $"{num}:{num2:00}";
	}

	public void StartVideo()
	{
		m_VidePreview.Or(null)?.gameObject.SetActive(value: false);
		m_Video.Or(null)?.Play();
		VideoIsStarted = true;
		VideoIsPlaying.Value = true;
	}

	public void PlayPauseVideo()
	{
		if (!VideoIsPlaying.Value)
		{
			StartVideo();
		}
		else
		{
			ResetVideo();
		}
	}

	protected void ShowHideInterface(bool state)
	{
		m_InterfaceIsShowed = state;
		if (state)
		{
			m_PlayPauseBigButtonFadeAnimator.Or(null)?.AppearAnimation();
			m_VideoProgressFadeAnimator.Or(null)?.AppearAnimation();
		}
		else
		{
			if (!VideoIsStarted || !VideoIsPlaying.Value)
			{
				return;
			}
			DelayedInvoker.InvokeInTime(delegate
			{
				if (VideoIsStarted && VideoIsPlaying.Value && !m_InterfaceIsShowed)
				{
					m_PlayPauseBigButtonFadeAnimator.Or(null)?.DisappearAnimation();
					m_VideoProgressFadeAnimator.Or(null)?.DisappearAnimation();
				}
			}, m_HideInterfaceDelay);
		}
	}
}
