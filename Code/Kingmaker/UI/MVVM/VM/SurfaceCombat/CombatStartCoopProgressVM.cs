using System;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.SurfaceCombat;

public class CombatStartCoopProgressVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetEvents, ISubscriber
{
	public readonly BoolReactiveProperty IsActive = new BoolReactiveProperty(initialValue: false);

	public readonly IntReactiveProperty CurrentProgress = new IntReactiveProperty(0);

	public readonly IntReactiveProperty TotalProgress = new IntReactiveProperty(0);

	public CombatStartCoopProgressVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		if (UINetUtility.InLobbyAndPlayingOrLoading)
		{
			IsActive.Value = true;
			RefreshStartBattleProgress();
			AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
			{
				RefreshStartBattleProgress();
			}));
		}
	}

	protected override void DisposeImplementation()
	{
	}

	private void RefreshStartBattleProgress()
	{
		if (Game.Instance.TurnController.GetStartBattleProgress(out var current, out var target, out var _))
		{
			CurrentProgress.Value = current;
			TotalProgress.Value = target;
		}
	}

	void INetEvents.HandleTransferProgressChanged(bool value)
	{
	}

	void INetEvents.HandleNetStateChanged(LobbyNetManager.State state)
	{
		IsActive.Value = PhotonManager.Lobby.IsActive;
	}

	void INetEvents.HandleNetGameStateChanged(NetGame.State state)
	{
		IsActive.Value = PhotonManager.Lobby.IsActive;
	}

	void INetEvents.HandleNLoadingScreenClosed()
	{
	}
}
