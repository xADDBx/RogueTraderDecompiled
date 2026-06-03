using System;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Augmentations;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Menu;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows;

public class ServiceWindowsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INewServiceWindowUIHandler, ISubscriber, IGameModeHandler, ILevelUpCompleteUIHandler, ISubscriber<IBaseUnitEntity>, IEncyclopediaHandler, ILevelUpInitiateUIHandler, IShipCustomizationForceUIHandler, IAreaHandler, IAdditiveAreaSwitchHandler, ITurnBasedModeHandler, ITurnBasedModeStartHandler, ILootInteractionHandler, IVendorUIHandler, ISubscriber<IMechanicEntity>, IMultiEntranceHandler, IFormationWindowUIHandler, ICharInfoAbilitiesChooseModeHandler
{
	public readonly ReactiveProperty<ServiceWindowsMenuVM> ServiceWindowsMenuVM = new ReactiveProperty<ServiceWindowsMenuVM>();

	public readonly ReactiveProperty<InventoryVM> InventoryVM = new ReactiveProperty<InventoryVM>();

	public readonly ReactiveProperty<CharacterInfoVM> CharacterInfoVM = new ReactiveProperty<CharacterInfoVM>();

	public readonly ReactiveProperty<JournalVM> JournalVM = new ReactiveProperty<JournalVM>();

	public readonly ReactiveProperty<EncyclopediaVM> EncyclopediaVM = new ReactiveProperty<EncyclopediaVM>();

	public readonly ReactiveProperty<LocalMapVM> LocalMapVM = new ReactiveProperty<LocalMapVM>();

	public readonly ReactiveProperty<ShipCustomizationVM> ShipCustomizationVM = new ReactiveProperty<ShipCustomizationVM>();

	public readonly ReactiveProperty<ColonyManagementVM> ColonyManagementVM = new ReactiveProperty<ColonyManagementVM>();

	public readonly ReactiveProperty<CargoManagementVM> CargoManagementVM = new ReactiveProperty<CargoManagementVM>();

	public readonly ReactiveProperty<AugmentationsVM> AugmentationsVM = new ReactiveProperty<AugmentationsVM>();

	private readonly ReactiveCommand<ServiceWindowsType> m_OnWindowShown = new ReactiveCommand<ServiceWindowsType>();

	private readonly ReactiveCommand m_OnWindowHidden = new ReactiveCommand();

	public ServiceWindowsType CurrentWindow;

	public readonly BoolReactiveProperty ForceHideBackground = new BoolReactiveProperty();

	private CharInfoPageType m_CharInfoPageType;

	private ShipCustomizationTab m_ShipCustomizationTabType;

	private bool m_CharInfoAbilitiesChooseMode;

	public readonly ReactiveCommand<ServiceWindowsType> OnOpen = new ReactiveCommand<ServiceWindowsType>();

	private readonly CompositeDisposable m_SelectUnit = new CompositeDisposable();

	private INode m_OpenEncyclopediaPage;

	public bool CharInfoAbilitiesChooseMode => m_CharInfoAbilitiesChooseMode;

	public CharInfoPageType CharInfoPageType => m_CharInfoPageType;

	private bool IsInSpace
	{
		get
		{
			if (!(Game.Instance.CurrentMode == GameModeType.SpaceCombat) && !(Game.Instance.CurrentMode == GameModeType.StarSystem))
			{
				return Game.Instance.CurrentMode == GameModeType.GlobalMap;
			}
			return true;
		}
	}

	private bool FormationIsOpened => FormationVM.Instance != null;

	private bool ServiceWindowNowIsOpening
	{
		get
		{
			return RootUIContext.Instance.ServiceWindowNowIsOpening;
		}
		set
		{
			RootUIContext.Instance.ServiceWindowNowIsOpening = value;
		}
	}

	public ServiceWindowsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		BindKeys();
		ServiceWindowNowIsOpening = false;
	}

	protected override void DisposeImplementation()
	{
		ServiceWindowNowIsOpening = false;
		HideMenu();
		HideWindow(CurrentWindow);
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, GetFullScreenUIType(CurrentWindow));
		});
	}

	private void BindKeys()
	{
		AddDisposable(Game.Instance.Keyboard.Bind("OpenInventory", HandleOpenInventory));
		AddDisposable(Game.Instance.Keyboard.Bind("OpenAugmentations", HandleOpenAugmentations));
		AddDisposable(Game.Instance.Keyboard.Bind("OpenCharacterScreen", HandleOpenCharacterInfo));
		AddDisposable(Game.Instance.Keyboard.Bind("OpenJournal", delegate
		{
			EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
			{
				h.HandleOpenJournal();
			});
		}));
		if (!UIUtility.IsShipArea())
		{
			AddDisposable(Game.Instance.Keyboard.Bind("OpenMap", HandleOpenLocalMap));
		}
		AddDisposable(Game.Instance.Keyboard.Bind("OpenEncyclopedia", delegate
		{
			HandleOpenEncyclopedia();
		}));
		AddDisposable(Game.Instance.Keyboard.Bind("OpenCargoManagement", HandleOpenCargoManagement));
		if (Game.Instance.Player.CanAccessStarshipInventory)
		{
			AddDisposable(Game.Instance.Keyboard.Bind("OpenShipCustomization", delegate
			{
				HandleOpenShipCustomization();
			}));
		}
		AddDisposable(Game.Instance.Keyboard.Bind("OpenColonyManagement", delegate
		{
			if (Game.Instance.Player.CanAccessStarshipInventory && !Game.Instance.Player.ColoniesState.ForbidColonization)
			{
				HandleOpenColonyManagement();
			}
		}));
		if (IsInSpace)
		{
			return;
		}
		AddDisposable(Game.Instance.Keyboard.Bind("OpenFormation", delegate
		{
			if (!RootUIContext.Instance.IsBlockedFullScreenUIType() && !IsInSpace && !RootUIContext.Instance.ServiceWindowNowIsOpening)
			{
				if (FormationIsOpened)
				{
					EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
					{
						h.HandleCloseFormation();
					});
				}
				else
				{
					EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
					{
						h.HandleOpenFormation();
					});
				}
			}
		}));
	}

	public void HandleCloseAll()
	{
		HideWindow(CurrentWindow);
		ServiceWindowsMenuVM.Value?.SelectWindow(ServiceWindowsType.None);
		HideMenu();
	}

	public void HandleOpenWindowOfType(ServiceWindowsType type, Action onClosed = null)
	{
		HandleOpenWindow(type, onClosed);
	}

	public void HandleOpenInventory()
	{
		HandleOpenWindow(ServiceWindowsType.Inventory);
	}

	public void HandleOpenAugmentations()
	{
		HandleOpenWindow(ServiceWindowsType.Augmentations);
	}

	public void HandleOpenEncyclopedia(INode page = null)
	{
		m_OpenEncyclopediaPage = page;
		HandleOpenWindow(ServiceWindowsType.Encyclopedia);
	}

	public void HandleOpenCharacterInfo()
	{
		m_CharInfoPageType = CharInfoPageType.Summary;
		HandleOpenWindow(ServiceWindowsType.CharacterInfo);
	}

	public void HandleOpenCharacterInfoPage(CharInfoPageType pageType, BaseUnitEntity unitEntity)
	{
		m_CharInfoPageType = pageType;
		if (CharacterInfoVM.Value != null)
		{
			InternalSelectUnit(unitEntity);
			CharacterInfoVM.Value.SetCurrentPage(pageType);
			return;
		}
		m_SelectUnit.Add(m_OnWindowShown.Subscribe(delegate
		{
			CharacterInfoVM.Value?.ClearProgressionIfNeeded(unitEntity);
			InternalSelectUnit(unitEntity);
			m_SelectUnit.Clear();
		}));
		HandleOpenWindow(ServiceWindowsType.CharacterInfo);
	}

	private void InternalSelectUnit(BaseUnitEntity unitEntity)
	{
		if (unitEntity != null)
		{
			Game.Instance.SelectionCharacter.SetSelected(unitEntity, force: true, forceFullScreenState: true);
		}
	}

	public void HandleOpenShipCustomizationPage(ShipCustomizationTab pageType)
	{
		m_ShipCustomizationTabType = pageType;
		if (ShipCustomizationVM.Value != null)
		{
			ShipCustomizationVM.Value.SetCurrentTab(pageType);
		}
		else
		{
			HandleOpenWindow(ServiceWindowsType.ShipCustomization);
		}
	}

	public void HandleOpenJournal()
	{
		HandleOpenWindow(ServiceWindowsType.Journal);
	}

	public void HandleOpenLocalMap()
	{
		HandleOpenWindow(ServiceWindowsType.LocalMap);
	}

	public void HandleOpenColonyManagement()
	{
		HandleOpenWindow(ServiceWindowsType.ColonyManagement);
	}

	public void HandleOpenCargoManagement()
	{
		HandleOpenWindow(ServiceWindowsType.CargoManagement);
	}

	public void HandleOpenShipCustomization(bool force = false)
	{
		if (Game.Instance.Player.CanAccessStarshipInventory || force)
		{
			m_ShipCustomizationTabType = (force ? ShipCustomizationTab.Skills : ShipCustomizationTab.Upgrade);
			HandleOpenWindow(ServiceWindowsType.ShipCustomization);
		}
	}

	private void HandleOpenWindow(ServiceWindowsType type, Action onClosed = null)
	{
		UIVisibilityState.ShowAllUI();
		EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
		{
			h.HandleCloseFormation();
		});
		bool flag = type == ServiceWindowsType.Inventory || type == ServiceWindowsType.CargoManagement || type == ServiceWindowsType.CharacterInfo || type == ServiceWindowsType.ShipCustomization;
		if (((bool)Game.Instance.Player.ServiceWindowsBlocked && flag) || (type == ServiceWindowsType.Augmentations && !StoreManager.CheckIfDlcPurchasedAndInstalled(DlcNameEnum.DLC3TheInfiniteMuseion)) || ((bool)Game.Instance.Player.InventoryWindowBlocked && type == ServiceWindowsType.Inventory) || ((bool)Game.Instance.Player.CharacterInfoWindowBlocked && type == ServiceWindowsType.CharacterInfo) || ((bool)Game.Instance.Player.AugmentationsWindowBlocked && type == ServiceWindowsType.Augmentations) || ServiceWindowNowIsOpening || RootUIContext.Instance.IsVendorShow)
		{
			return;
		}
		if (RootUIContext.Instance.IsBlockedFullScreenUIType() && !CanShowAugments())
		{
			if (!CanShowEncyclopedia())
			{
				return;
			}
			FullScreenUIType fullScreenUIType = Game.Instance.RootUiContext.FullScreenUIType;
			if (fullScreenUIType == FullScreenUIType.Chargen || fullScreenUIType == FullScreenUIType.TransitionMap)
			{
				return;
			}
		}
		ServiceWindowNowIsOpening = true;
		HandleOpenWindowDelayed(type, onClosed);
		bool CanShowAugments()
		{
			if (type == ServiceWindowsType.Augmentations && RootUIContext.Instance.CommonVM.EscMenuContextVM.EscMenu.Value == null)
			{
				if (!(Game.Instance.CurrentMode == GameModeType.Dialog) && !(Game.Instance.CurrentMode == GameModeType.Default))
				{
					return Game.Instance.CurrentMode == GameModeType.Cutscene;
				}
				return true;
			}
			return false;
		}
		bool CanShowEncyclopedia()
		{
			if (type == ServiceWindowsType.Encyclopedia && RootUIContext.Instance.CommonVM.EscMenuContextVM.EscMenu.Value == null)
			{
				if (!(Game.Instance.CurrentMode == GameModeType.Dialog) && !(Game.Instance.CurrentMode == GameModeType.Default))
				{
					return Game.Instance.CurrentMode == GameModeType.Pause;
				}
				return true;
			}
			return false;
		}
	}

	private void HandleOpenWindowDelayed(ServiceWindowsType type, Action onClosed = null)
	{
		if (RootUIContext.Instance.IsExplorationWindow && type != ServiceWindowsType.Encyclopedia)
		{
			ServiceWindowNowIsOpening = false;
			return;
		}
		if (ServiceWindowsMenuVM.Value == null)
		{
			if (type == CurrentWindow || type == ServiceWindowsType.None)
			{
				ServiceWindowNowIsOpening = false;
				return;
			}
			ForceHideBackground.Value = type == ServiceWindowsType.ShipCustomization;
			ShowMenu();
		}
		OnOpen?.Execute(type);
		ServiceWindowsMenuVM.Value?.SelectWindow(type, onClosed);
	}

	private void OnSelectWindow(ServiceWindowsType type)
	{
		if (CurrentWindow == ServiceWindowsType.CharacterInfo)
		{
			CharacterInfoVM value = CharacterInfoVM.Value;
			if (value != null && !value.CanCloseWindow)
			{
				UIUtility.ShowMessageBox(UIStrings.Instance.CharacterSheet.LevelupDialogCloseProgression, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
				{
					if (button == DialogMessageBoxBase.BoxButton.Yes)
					{
						DoSelectWindow(type);
					}
				});
				ServiceWindowNowIsOpening = false;
				return;
			}
		}
		if (CurrentWindow == ServiceWindowsType.Augmentations)
		{
			AugmentationsVM.Value?.RemoveNotInstalled();
		}
		DoSelectWindow(type);
	}

	private void DoSelectWindow(ServiceWindowsType type)
	{
		HideWindow(CurrentWindow);
		if (type == CurrentWindow || type == ServiceWindowsType.None)
		{
			HideMenu();
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, GetFullScreenUIType(CurrentWindow));
			});
			CurrentWindow = ServiceWindowsType.None;
			ServiceWindowNowIsOpening = false;
		}
		else
		{
			CurrentWindow = type;
			ShowWindow(type);
		}
	}

	private void ShowWindow(ServiceWindowsType type)
	{
		if ((BuildModeUtility.Data?.Loading?.WidgetStashCleanup).GetValueOrDefault())
		{
			WidgetFactoryStash.ResetStash();
		}
		switch (type)
		{
		case ServiceWindowsType.Inventory:
		{
			InventoryVM disposable5 = (InventoryVM.Value = new InventoryVM());
			AddDisposable(disposable5);
			break;
		}
		case ServiceWindowsType.CharacterInfo:
		{
			CharacterInfoVM disposable9 = (CharacterInfoVM.Value = new CharacterInfoVM(m_CharInfoPageType));
			AddDisposable(disposable9);
			AddDisposable(CharacterInfoVM.Value.PageType.Subscribe(delegate(CharInfoPageType value)
			{
				m_CharInfoPageType = value;
			}));
			break;
		}
		case ServiceWindowsType.Journal:
		{
			JournalVM disposable8 = (JournalVM.Value = new JournalVM());
			AddDisposable(disposable8);
			break;
		}
		case ServiceWindowsType.LocalMap:
		{
			LocalMapVM disposable7 = (LocalMapVM.Value = new LocalMapVM());
			AddDisposable(disposable7);
			break;
		}
		case ServiceWindowsType.Encyclopedia:
		{
			EncyclopediaVM disposable6 = (EncyclopediaVM.Value = new EncyclopediaVM(m_OpenEncyclopediaPage));
			AddDisposable(disposable6);
			break;
		}
		case ServiceWindowsType.ShipCustomization:
			if (!RootUIContext.Instance.HasDialog)
			{
				ShipCustomizationVM disposable4 = (ShipCustomizationVM.Value = new ShipCustomizationVM(null, m_ShipCustomizationTabType));
				AddDisposable(disposable4);
			}
			break;
		case ServiceWindowsType.ColonyManagement:
		{
			ColonyManagementVM disposable3 = (ColonyManagementVM.Value = new ColonyManagementVM());
			AddDisposable(disposable3);
			break;
		}
		case ServiceWindowsType.CargoManagement:
		{
			CargoManagementVM disposable2 = (CargoManagementVM.Value = new CargoManagementVM());
			AddDisposable(disposable2);
			break;
		}
		case ServiceWindowsType.Augmentations:
		{
			AugmentationsVM disposable = (AugmentationsVM.Value = new AugmentationsVM());
			AddDisposable(disposable);
			break;
		}
		}
		ServiceWindowNowIsOpening = false;
		if (Game.Instance.ClickEventsController != null && Game.Instance.ClickEventsController.Mode != 0 && !IsInSpace)
		{
			Game.Instance.ClickEventsController.ClearPointerMode();
		}
		m_OnWindowShown.Execute(type);
	}

	private void HideWindow(ServiceWindowsType type)
	{
		switch (type)
		{
		case ServiceWindowsType.Inventory:
			DisposeAndRemove(InventoryVM);
			break;
		case ServiceWindowsType.CharacterInfo:
			DisposeAndRemove(CharacterInfoVM);
			break;
		case ServiceWindowsType.Journal:
			DisposeAndRemove(JournalVM);
			break;
		case ServiceWindowsType.LocalMap:
			DisposeAndRemove(LocalMapVM);
			break;
		case ServiceWindowsType.Encyclopedia:
			DisposeAndRemove(EncyclopediaVM);
			break;
		case ServiceWindowsType.ColonyManagement:
			DisposeAndRemove(ColonyManagementVM);
			break;
		case ServiceWindowsType.ShipCustomization:
			DisposeAndRemove(ShipCustomizationVM);
			break;
		case ServiceWindowsType.CargoManagement:
			DisposeAndRemove(CargoManagementVM);
			break;
		case ServiceWindowsType.Augmentations:
			DisposeAndRemove(AugmentationsVM);
			break;
		}
		m_OnWindowHidden.Execute();
	}

	private void ShowMenu()
	{
		ServiceWindowsMenuVM disposable = (ServiceWindowsMenuVM.Value = new ServiceWindowsMenuVM(OnSelectWindow));
		AddDisposable(disposable);
	}

	private void HideMenu()
	{
		ServiceWindowsMenuVM.Value?.Dispose();
		ServiceWindowsMenuVM.Value = null;
		if ((BuildModeUtility.Data?.Loading?.WidgetStashCleanup).GetValueOrDefault())
		{
			Game.Instance.CoroutinesController.InvokeInTicks(WidgetFactoryStash.ResetStash, 1);
		}
	}

	public void HandleLevelUpComplete(bool isChargen)
	{
		HandleCloseAll();
	}

	public void HandleChargenStart(Action enterNewGameAction)
	{
		HandleCloseAll();
	}

	public void HandleEncyclopediaPage(string pageKey)
	{
		if (!string.IsNullOrEmpty(pageKey))
		{
			BlueprintEncyclopediaPage page = ChapterList.GetPage(pageKey);
			if (page != null)
			{
				HandleEncyclopediaPage(page);
			}
		}
	}

	public void HandleEncyclopediaPage(INode page)
	{
		if (!(page is GlossaryLetterIndexPage))
		{
			Game.Instance.Player.UISettings.CurrentEncyclopediaPage = page as IPage;
		}
		if (EncyclopediaVM.Value == null)
		{
			HandleOpenWindow(ServiceWindowsType.Encyclopedia);
		}
		else
		{
			EncyclopediaVM.Value.HandleEncyclopediaPage(page);
		}
	}

	private static FullScreenUIType GetFullScreenUIType(ServiceWindowsType type)
	{
		return type switch
		{
			ServiceWindowsType.None => FullScreenUIType.Unknown, 
			ServiceWindowsType.Inventory => FullScreenUIType.Inventory, 
			ServiceWindowsType.CharacterInfo => FullScreenUIType.CharacterScreen, 
			ServiceWindowsType.Journal => FullScreenUIType.Journal, 
			ServiceWindowsType.Encyclopedia => FullScreenUIType.Encyclopedia, 
			ServiceWindowsType.ShipCustomization => FullScreenUIType.ShipCustomization, 
			ServiceWindowsType.LocalMap => FullScreenUIType.LocalMap, 
			ServiceWindowsType.ColonyManagement => FullScreenUIType.ColonyManagement, 
			ServiceWindowsType.CargoManagement => FullScreenUIType.CargoManagement, 
			ServiceWindowsType.Augmentations => FullScreenUIType.Augmentations, 
			_ => FullScreenUIType.Unknown, 
		};
	}

	public void HandleLevelUpStart(BaseUnitEntity unit, Action onCommit = null, Action onStop = null, LevelUpState.CharBuildMode mode = LevelUpState.CharBuildMode.LevelUp)
	{
		HandleOpenCharacterInfoPage(CharInfoPageType.LevelProgression, unit);
	}

	public void HandleForceOpenShipCustomization()
	{
		HandleOpenShipCustomization(force: true);
	}

	public void HandleForceCloseAllComponentsMenu()
	{
		HandleCloseAll();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.CutsceneGlobalMap)
		{
			HandleCloseAll();
		}
		else if (gameMode == GameModeType.Dialog && CurrentWindow != ServiceWindowsType.ColonyManagement)
		{
			HandleCloseAll();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		HandleCloseAll();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		HandleCloseAll();
	}

	public void OnAdditiveAreaDidActivated()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			HandleCloseAll();
		}
	}

	void ITurnBasedModeStartHandler.HandleTurnBasedModeStarted()
	{
		HandleCloseAll();
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
	}

	public void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null)
	{
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		HandleCloseAll();
	}

	public void HandleTradeStarted()
	{
		HandleCloseAll();
	}

	public void HandleMultiEntrance(BlueprintMultiEntrance multiEntrance)
	{
		HandleCloseAll();
	}

	public void HandleOpenFormation()
	{
		DoSelectWindow(ServiceWindowsType.None);
	}

	public void HandleCloseFormation()
	{
	}

	public void HandleChooseMode(bool active)
	{
		m_CharInfoAbilitiesChooseMode = active;
	}
}
