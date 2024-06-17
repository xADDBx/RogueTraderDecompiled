using System;
using System.Collections.Generic;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Credits;
using Kingmaker.Code.UI.MVVM.VM.Dialog;
using Kingmaker.Code.UI.MVVM.VM.EtudeCounter;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.Code.UI.MVVM.VM.GameOver;
using Kingmaker.Code.UI.MVVM.VM.GroupChanger;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.Retrain;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.Subtitle;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Surface;

public class SurfaceStaticPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IFormationWindowUIHandler, ISubscriber, IVendorUIHandler, ISubscriber<IMechanicEntity>, IGameModeHandler, IMultiEntranceHandler, IVendorLogicStateChanged, IBeginSelectingVendorHandler, ICreditsWindowUIHandler, IProfitFactorHandler, IScreenUIHandler
{
	public readonly ServiceWindowsVM ServiceWindowsVM;

	public readonly LootContextVM LootContextVM;

	public readonly DialogContextVM DialogContextVM;

	public readonly GroupChangerContextVM GroupChangerContextVM;

	public readonly SurfaceHUDVM SurfaceHUDVM;

	public readonly SubtitleVM SubtitleVM;

	public readonly EtudeCounterVM EtudeCounterVM;

	public readonly CharGenContextVM CharGenContextVM;

	public readonly RespecContextVM RespecContextVM;

	public readonly UIVisibilityVM UIVisibilityVM;

	public readonly ReactiveProperty<TransitionVM> TransitionVM = new ReactiveProperty<TransitionVM>(null);

	public readonly ReactiveProperty<FormationVM> FormationVM = new ReactiveProperty<FormationVM>();

	public readonly ReactiveProperty<CreditsVM> CreditsVM = new ReactiveProperty<CreditsVM>();

	public readonly ReactiveProperty<VendorVM> VendorVM = new ReactiveProperty<VendorVM>();

	public readonly ReactiveProperty<GameOverVM> GameOverVM = new ReactiveProperty<GameOverVM>(null);

	public readonly ReactiveProperty<VendorSelectingWindowVM> VendorSelectingWindowVM = new ReactiveProperty<VendorSelectingWindowVM>();

	private Action<MechanicEntity> m_BeginTradingAction;

	public SurfaceStaticPartVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ServiceWindowsVM = new ServiceWindowsVM());
		AddDisposable(LootContextVM = new LootContextVM());
		AddDisposable(DialogContextVM = new DialogContextVM());
		AddDisposable(GroupChangerContextVM = new GroupChangerContextVM());
		AddDisposable(SurfaceHUDVM = new SurfaceHUDVM());
		AddDisposable(SubtitleVM = new SubtitleVM());
		AddDisposable(EtudeCounterVM = new EtudeCounterVM());
		AddDisposable(CharGenContextVM = new CharGenContextVM());
		AddDisposable(RespecContextVM = new RespecContextVM());
		AddDisposable(UIVisibilityVM = new UIVisibilityVM());
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleShowEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void HandleOpenFormation()
	{
		if (FormationVM.Value == null)
		{
			FormationVM disposable = (FormationVM.Value = new FormationVM(DisposeFormation));
			AddDisposable(disposable);
		}
	}

	public void HandleCloseFormation()
	{
		if (FormationVM.Value != null)
		{
			DisposeFormation();
		}
	}

	private void DisposeFormation()
	{
		FormationVM.Value?.Dispose();
		FormationVM.Value = null;
	}

	public void HandleOpenCredits()
	{
		CreditsVM disposable = (CreditsVM.Value = new CreditsVM(DisposeCredits, onlyBakers: true));
		AddDisposable(disposable);
		void DisposeCredits()
		{
			CreditsVM.Value?.Dispose();
			CreditsVM.Value = null;
		}
	}

	void IVendorLogicStateChanged.HandleBeginTrading()
	{
		m_BeginTradingAction?.Invoke(EventInvokerExtensions.MechanicEntity);
		m_BeginTradingAction = null;
	}

	void IVendorLogicStateChanged.HandleEndTrading()
	{
		DisposeVendor();
	}

	void IVendorLogicStateChanged.HandleVendorAboutToTrading()
	{
	}

	public void HandleTradeStarted()
	{
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null)
		{
			m_BeginTradingAction = delegate
			{
				VendorVM disposable = (VendorVM.Value = new VendorVM());
				AddDisposable(disposable);
			};
			VendorHelper.Vendor.BeginTrading(mechanicEntity);
		}
	}

	private void DisposeVendor()
	{
		VendorVM.Value?.Dispose();
		VendorVM.Value = null;
	}

	public void HandleMultiEntrance(BlueprintMultiEntrance multiEntrance)
	{
		bool flag = true;
		if ((bool)ContextData<AreaTransitionPartGameCommand.TransitionExecutorEntity>.Current)
		{
			EntityRef<BaseUnitEntity> entityRef = ContextData<AreaTransitionPartGameCommand.TransitionExecutorEntity>.Current.EntityRef;
			flag = entityRef.IsNull || (!entityRef.IsNull && ((BaseUnitEntity)entityRef).IsDirectlyControllable());
		}
		if (flag)
		{
			TransitionVM disposable = (TransitionVM.Value = new TransitionVM(multiEntrance, DisposeTransition));
			AddDisposable(disposable);
		}
	}

	private void DisposeTransition()
	{
		TransitionVM.Value?.Dispose();
		TransitionVM.Value = null;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!(gameMode != GameModeType.GameOver))
		{
			SurfaceHUDVM.InspectVM.HandleShowInspect(state: false);
			TooltipHelper.HideTooltip();
			GameOverVM disposable = (GameOverVM.Value = new GameOverVM());
			AddDisposable(disposable);
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (!(gameMode != GameModeType.GameOver))
		{
			GameOverVM.Value?.Dispose();
			GameOverVM.Value = null;
		}
	}

	public void HandleBeginSelectingVendor(List<MechanicEntity> vendors)
	{
		VendorSelectingWindowVM disposable = (VendorSelectingWindowVM.Value = new VendorSelectingWindowVM(vendors));
		AddDisposable(disposable);
	}

	public void HandleExitSelectingVendor()
	{
		VendorSelectingWindowVM.Value?.Dispose();
		VendorSelectingWindowVM.Value = null;
	}

	public void HandleProfitFactorModifierAdded(float max, ProfitFactorModifier modifier)
	{
		UIUtility.ShowProfitFactorModifiedNotification(max, modifier);
	}

	public void HandleProfitFactorModifierRemoved(float max, ProfitFactorModifier modifier)
	{
		UIUtility.ShowProfitFactorModifiedNotification(max, modifier);
	}

	void IScreenUIHandler.CloseScreen(IScreenUIHandler.ScreenType screen)
	{
		if (screen == IScreenUIHandler.ScreenType.VendorSelecting)
		{
			DisposeAndRemove(VendorSelectingWindowVM);
		}
	}
}
