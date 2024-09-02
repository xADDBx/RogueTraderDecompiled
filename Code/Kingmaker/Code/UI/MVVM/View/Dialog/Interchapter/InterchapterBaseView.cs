using DG.Tweening;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Interchapter;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Interchapter;

public class InterchapterBaseView : ViewBase<InterchapterVM>, IInitializable
{
	[Header("Video")]
	[SerializeField]
	private VideoPlayerHelper m_Video;

	[FormerlySerializedAs("m_VideoBackground")]
	[SerializeField]
	private CanvasGroup m_VideoGroup;

	[Header("Subtitle")]
	[SerializeField]
	private TextMeshProUGUI m_SubtitleText;

	[SerializeField]
	private CanvasGroup m_SubtitleGroup;

	private readonly ReactiveProperty<VideoState> m_State = new ReactiveProperty<VideoState>(VideoState.Inactive);

	private float m_InterruptTimer;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_State.Value = VideoState.Inactive;
		m_Video.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(delegate
		{
			InternalUpdate();
		}));
		AddDisposable(m_State.Subscribe(delegate(VideoState value)
		{
			base.ViewModel.Data.SetState(value);
		}));
		m_State.Value = VideoState.Preparing;
		m_VideoGroup.alpha = 0f;
		m_VideoGroup.DOFade(1f, 0.2f).SetUpdate(isIndependentUpdate: true);
		m_Video.Stop();
		m_Video.SetClip(base.ViewModel.VideoClip, SoundStateType.Video, prepareVideo: true, base.ViewModel.SoundStartEvent, base.ViewModel.SoundStopEvent);
		AddDisposable(base.ViewModel.Subtitle.Subscribe(delegate(string value)
		{
			m_SubtitleGroup.DOFade((value != string.Empty) ? 1 : 0, 0.2f).SetUpdate(isIndependentUpdate: true);
			m_SubtitleText.text = value;
		}));
	}

	private void InternalUpdate()
	{
		switch (m_State.Value)
		{
		case VideoState.Preparing:
			if (m_Video.IsPlaying)
			{
				m_State.Value = VideoState.Playing;
			}
			break;
		case VideoState.Playing:
			if (!m_Video.IsPlaying || m_Video.IsOvertime)
			{
				m_State.Value = VideoState.Finishing;
			}
			break;
		case VideoState.PlayingPressAnyKey:
			m_InterruptTimer -= Time.deltaTime;
			if (m_InterruptTimer <= 0f)
			{
				m_State.Value = VideoState.Playing;
			}
			if (!m_Video.IsPlaying)
			{
				m_State.Value = VideoState.Finishing;
			}
			break;
		case VideoState.Finishing:
			break;
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_Video.Stop();
		m_State.Value = VideoState.Inactive;
		base.gameObject.SetActive(value: false);
	}
}
