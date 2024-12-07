using System;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.EscMenu;

public class EscMenuContextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEscMenuHandler, ISubscriber
{
	public readonly ReactiveProperty<EscMenuVM> EscMenu = new ReactiveProperty<EscMenuVM>();

	public EscMenuContextVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(RequestEscMenu));
	}

	protected override void DisposeImplementation()
	{
		DisposeEscMenu();
	}

	private void RequestEscMenu()
	{
		if (!Game.Instance.Player.Tutorial.HasShownData && !Game.Instance.RootUiContext.IsMainMenu && !LoadingProcess.Instance.IsLoadingScreenActive && !Game.Instance.RootUiContext.ServiceWindowNowIsOpening && (!Game.Instance.Player.WarpTravelState.IsInWarpTravel || !(Game.Instance.CurrentMode == GameModeType.GlobalMap)) && EscMenu.Value == null)
		{
			EscMenu.Value = new EscMenuVM(DisposeEscMenu);
		}
	}

	public void DisposeEscMenu()
	{
		EscMenu.Value?.Dispose();
		EscMenu.Value = null;
	}

	void IEscMenuHandler.HandleOpen()
	{
		RequestEscMenu();
	}

	public void HandleEscMenuOnShow()
	{
	}

	public void HandleEscMenuOnHide()
	{
	}
}
