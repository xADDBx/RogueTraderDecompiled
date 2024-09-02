using System;
using Kingmaker.Blueprints.Root;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.VM.CustomVideoPlayer;

public class CustomUIVideoPlayerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public VideoClip Video;

	public Sprite PreviewArt;

	public string SoundStart;

	public string SoundStop;

	public readonly BoolReactiveProperty HasVideo = new BoolReactiveProperty();

	public readonly ReactiveCommand ChangeVideo = new ReactiveCommand();

	public readonly ReactiveCommand ResetVideoCommand = new ReactiveCommand();

	protected override void DisposeImplementation()
	{
	}

	public void SetVideo(VideoClip videoClip, Sprite previewArt, string soundStart, string soundStop)
	{
		Video = videoClip;
		PreviewArt = ((previewArt != null) ? previewArt : UIConfig.Instance.KeyArt);
		SoundStart = soundStart;
		SoundStop = soundStop;
		HasVideo.Value = Video != null;
		ChangeVideo.Execute();
	}

	public void ResetVideo()
	{
		ResetVideoCommand.Execute();
	}
}
