using System;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine.Video;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.Interchapter;

public class InterchapterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISubtitleBarkHandler, ISubscriber
{
	public readonly ReactiveProperty<string> Subtitle = new ReactiveProperty<string>(string.Empty);

	public InterchapterData Data { get; }

	public VideoClip VideoClip => Data.VideoClip;

	public string SoundStartEvent => Data.SoundStartEvent;

	public string SoundStopEvent => Data.SoundStopEvent;

	public InterchapterVM(InterchapterData data)
	{
		EventBus.Subscribe(this);
		Data = data;
	}

	public void Finish()
	{
		Data.Finish();
	}

	protected override void DisposeImplementation()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleOnShowBark(string text, float duration)
	{
		Subtitle.Value = text;
		DelayedInvoker.InvokeInTime(delegate
		{
			Subtitle.Value = string.Empty;
		}, duration);
	}

	public void HandleOnHideBark()
	{
		Subtitle.Value = string.Empty;
	}
}
