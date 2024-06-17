using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Subtitle;

public class SubtitleVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISubtitleBarkHandler, ISubscriber
{
	public readonly ReactiveProperty<string> BarkText = new ReactiveProperty<string>(string.Empty);

	public SubtitleVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		BarkText.Value = string.Empty;
	}

	public void HandleOnShowBark(string text, float duration)
	{
		BarkText.Value = text;
		DelayedInvoker.InvokeInTime(delegate
		{
			BarkText.Value = string.Empty;
		}, duration);
	}

	public void HandleOnHideBark()
	{
		BarkText.Value = string.Empty;
	}
}
