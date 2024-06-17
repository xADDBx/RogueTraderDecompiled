using Kingmaker.Code.UI.MVVM;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.VM.NetRoles;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.View.NetRoles.Base;

public class NetRolesBaseView : ViewBase<NetRolesVM>, IGameModeHandler, ISubscriber, IInitializable
{
	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		SetVisibility(state: true);
	}

	protected override void DestroyViewImplementation()
	{
		SetVisibility(state: false);
	}

	private void SetVisibility(bool state)
	{
		if (state)
		{
			UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		}
		else
		{
			UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
		}
		EventBus.RaiseEvent(delegate(IModalWindowUIHandler h)
		{
			h.HandleModalWindowUiChanged(state, ModalWindowUIType.NetRoles);
		});
		base.gameObject.SetActive(state);
		Game.Instance.RequestPauseUi(state);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.Dialog)
		{
			base.ViewModel.OnClose();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
