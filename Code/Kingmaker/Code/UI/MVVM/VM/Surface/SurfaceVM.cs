using System;
using Kingmaker.Code.UI.MVVM.VM.QuestNotification;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Surface;

public class SurfaceVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler
{
	public readonly SurfaceStaticPartVM StaticPartVM;

	public readonly SurfaceDynamicPartVM DynamicPartVM;

	public readonly SurfaceCombatPartVM CombatPartVM;

	public readonly ReactiveProperty<bool> IsCombatInputModeActive = new ReactiveProperty<bool>();

	public bool IsInQuestNotification => QuestNotificatorVM.Instance?.IsShowUp.Value ?? false;

	public SurfaceVM()
	{
		AddDisposable(StaticPartVM = new SurfaceStaticPartVM());
		AddDisposable(DynamicPartVM = new SurfaceDynamicPartVM());
		AddDisposable(CombatPartVM = new SurfaceCombatPartVM());
		IsCombatInputModeActive.Value = TurnController.IsInTurnBasedCombat();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	void ITurnBasedModeHandler.HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		IsCombatInputModeActive.Value = isTurnBased;
		if (!isTurnBased && (BuildModeUtility.Data?.Loading?.WidgetStashCleanup).GetValueOrDefault())
		{
			WidgetFactoryStash.ResetStash();
		}
	}

	public void HandleTurnBasedModeResumed()
	{
		IsCombatInputModeActive.Value = true;
	}

	public void OpenJournal()
	{
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleOpenJournal();
		});
	}

	public void QuestNotificationForceClose()
	{
		QuestNotificatorVM.Instance.ForceClose();
	}
}
