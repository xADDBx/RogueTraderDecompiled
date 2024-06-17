using System;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Formation;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
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
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.GameCommands.Colonization;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
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

public class ServiceWindowsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INewServiceWindowUIHandler, ISubscriber, IGameModeHandler, ILevelUpCompleteUIHandler, ISubscriber<IBaseUnitEntity>, IEncyclopediaHandler, ILevelUpInitiateUIHandler, IShipCustomizationForceUIHandler, IAreaHandler, IAdditiveAreaSwitchHandler, ITurnBasedModeHandler, ITurnBasedModeStartHandler, ILootInteractionHandler, IVendorUIHandler, ISubscriber<IMechanicEntity>, IMultiEntranceHandler, ICommandServiceWindowUIHandler, IFormationWindowUIHandler, ICharInfoAbilitiesChooseModeHandler
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

	private bool m_ServiceWindowNowIsOpening;

	public bool CharInfoAbilitiesChooseMode => m_CharInfoAbilitiesChooseMode;

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

	public ServiceWindowsVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		BindKeys();
		m_ServiceWindowNowIsOpening = false;
	}

	protected override void DisposeImplementation()
	{
		m_ServiceWindowNowIsOpening = false;
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
		if (UIUtility.IsShipArea())
		{
			AddDisposable(Game.Instance.Keyboard.Bind("OpenColonyManagement", delegate
			{
				if (Game.Instance.Player.CanAccessStarshipInventory && !Game.Instance.Player.ColoniesState.ForbidColonization)
				{
					Game.Instance.GameCommandQueue.ColonyManagementUIOpen();
				}
			}));
		}
		if (IsInSpace)
		{
			return;
		}
		AddDisposable(Game.Instance.Keyboard.Bind("OpenFormation", delegate
		{
			if (!RootUIContext.Instance.IsBlockedFullScreenUIType() && !IsInSpace)
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

	public void HandleOpenWindowOfType(ServiceWindowsType type)
	{
		HandleOpenWindow(type);
	}

	void ICommandServiceWindowUIHandler.HandleOpenWindowOfType(ServiceWindowsType type)
	{
		HandleOpenWindow(type, onlyAnotherWindow: false);
	}

	public void HandleOpenInventory()
	{
		HandleOpenWindow(ServiceWindowsType.Inventory);
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

	public void HandleOpenShipCustomization()
	{
		m_ShipCustomizationTabType = ShipCustomizationTab.Upgrade;
		HandleOpenWindow(ServiceWindowsType.ShipCustomization);
	}

	private void HandleOpenWindow(ServiceWindowsType type)
	{
		HandleOpenWindow(type, onlyAnotherWindow: true);
	}

	private void HandleOpenWindow(ServiceWindowsType type, bool onlyAnotherWindow)
	{
		UIVisibilityState.ShowAllUI();
		EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
		{
			h.HandleCloseFormation();
		});
		if (!m_ServiceWindowNowIsOpening && (!RootUIContext.Instance.IsBlockedFullScreenUIType() || (CanShowEncyclopedia() && Game.Instance.RootUiContext.FullScreenUIType != FullScreenUIType.Chargen)))
		{
			m_ServiceWindowNowIsOpening = true;
			bool hasColonyManagementUIGameCommandContext = ContextData<ColonyManagementUIGameCommand.Context>.Current;
			Game.Instance.CoroutinesController.InvokeInTicks(delegate
			{
				HandleOpenWindowDelayed(type, onlyAnotherWindow, hasColonyManagementUIGameCommandContext);
			}, 1);
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

	private void HandleOpenWindowDelayed(ServiceWindowsType type, bool onlyAnotherWindow, bool hasColonyManagementUIGameCommandContext)
	{
		if (RootUIContext.Instance.IsExplorationWindow)
		{
			m_ServiceWindowNowIsOpening = false;
			return;
		}
		if (ServiceWindowsMenuVM.Value == null)
		{
			if ((type == CurrentWindow || type == ServiceWindowsType.None) && onlyAnotherWindow)
			{
				m_ServiceWindowNowIsOpening = false;
				return;
			}
			ForceHideBackground.Value = type == ServiceWindowsType.ShipCustomization;
			ShowMenu();
		}
		using (ContextData<ColonyManagementUIGameCommand.Context>.RequestIf(hasColonyManagementUIGameCommandContext))
		{
			OnOpen?.Execute(type);
			ServiceWindowsMenuVM.Value?.SelectWindow(type);
		}
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
				m_ServiceWindowNowIsOpening = false;
				return;
			}
		}
		if (CurrentWindow == ServiceWindowsType.ColonyManagement && Game.Instance.GameCommandQueue.ColonyManagementUIClose(type))
		{
			m_ServiceWindowNowIsOpening = false;
		}
		else
		{
			DoSelectWindow(type);
		}
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
			m_ServiceWindowNowIsOpening = false;
		}
		else
		{
			Game.Instance.CoroutinesController.InvokeInTicks(delegate
			{
				CurrentWindow = type;
				ShowWindow(type);
			}, 1);
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
			InventoryVM disposable4 = (InventoryVM.Value = new InventoryVM());
			AddDisposable(disposable4);
			break;
		}
		case ServiceWindowsType.CharacterInfo:
		{
			CharacterInfoVM disposable8 = (CharacterInfoVM.Value = new CharacterInfoVM(m_CharInfoPageType));
			AddDisposable(disposable8);
			break;
		}
		case ServiceWindowsType.Journal:
		{
			JournalVM disposable7 = (JournalVM.Value = new JournalVM());
			AddDisposable(disposable7);
			break;
		}
		case ServiceWindowsType.LocalMap:
		{
			LocalMapVM disposable6 = (LocalMapVM.Value = new LocalMapVM());
			AddDisposable(disposable6);
			break;
		}
		case ServiceWindowsType.Encyclopedia:
		{
			EncyclopediaVM disposable5 = (EncyclopediaVM.Value = new EncyclopediaVM(m_OpenEncyclopediaPage));
			AddDisposable(disposable5);
			break;
		}
		case ServiceWindowsType.ShipCustomization:
			if (!RootUIContext.Instance.HasDialog)
			{
				ShipCustomizationVM disposable3 = (ShipCustomizationVM.Value = new ShipCustomizationVM(null, m_ShipCustomizationTabType));
				AddDisposable(disposable3);
			}
			break;
		case ServiceWindowsType.ColonyManagement:
		{
			ColonyManagementVM disposable2 = (ColonyManagementVM.Value = new ColonyManagementVM());
			AddDisposable(disposable2);
			break;
		}
		case ServiceWindowsType.CargoManagement:
		{
			CargoManagementVM disposable = (CargoManagementVM.Value = new CargoManagementVM());
			AddDisposable(disposable);
			break;
		}
		}
		m_ServiceWindowNowIsOpening = false;
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
			_ => FullScreenUIType.Unknown, 
		};
	}

	public void HandleLevelUpStart(BaseUnitEntity unit, Action onCommit = null, Action onStop = null, LevelUpState.CharBuildMode mode = LevelUpState.CharBuildMode.LevelUp)
	{
		HandleOpenCharacterInfoPage(CharInfoPageType.LevelProgression, unit);
	}

	public void HandleForceOpenShipCustomization()
	{
		HandleOpenShipCustomization();
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
