using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Pause;

public class PauseNotificationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IPauseHandler, ISubscriber, IAreaHandler, IFullScreenUIHandler
{
	public readonly ReactiveCommand<bool> ShowPause = new ReactiveCommand<bool>();

	public readonly ReactiveCommand<bool> ChangeAlphaPause = new ReactiveCommand<bool>();

	public bool IsPaused
	{
		get
		{
			if (Game.Instance.IsPaused)
			{
				return !Game.Instance.PauseController.IsPausedByPlayers;
			}
			return false;
		}
	}

	public PauseNotificationVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void Unpause()
	{
		Game.Instance.PauseBind();
	}

	public void OnPauseToggled()
	{
		ShowPause.Execute(IsPaused);
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		ChangeAlphaPause.Execute(IsPaused);
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		ChangeAlphaPause.Execute(!state && IsPaused);
	}
}
