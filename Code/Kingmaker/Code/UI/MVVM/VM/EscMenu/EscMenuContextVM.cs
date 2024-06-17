using System;
using Kingmaker.EntitySystem.Persistence;
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

	private void RequestEscMenu()
	{
		if (!Game.Instance.Player.Tutorial.HasShownData && !Game.Instance.RootUiContext.IsMainMenu && !LoadingProcess.Instance.IsLoadingScreenActive && EscMenu.Value == null)
		{
			EscMenu.Value = new EscMenuVM(DisposeEscMenu);
		}
	}

	public void DisposeEscMenu()
	{
		EscMenu.Value?.Dispose();
		EscMenu.Value = null;
	}

	protected override void DisposeImplementation()
	{
		DisposeEscMenu();
	}

	void IEscMenuHandler.HandleOpen()
	{
		RequestEscMenu();
	}
}
