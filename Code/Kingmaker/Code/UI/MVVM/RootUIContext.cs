using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Package.Runtime.Extensions.Dependencies;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Sound;
using Kingmaker.UI.UIKitDependencies;
using Kingmaker.Utility;
using Kingmaker.Utility.DisposableExtension;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Owlcat.Runtime.UI.Dependencies;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.Code.UI.MVVM;

public class RootUIContext : BaseDisposable, IFullScreenUIHandler, ISubscriber, IModalWindowUIHandler
{
	private MonoBehaviour m_CommonView;

	private MonoBehaviour m_UIView;

	private MonoBehaviour m_UILoadingScreenView;

	private FullScreenUIType m_FullScreenUIType;

	private ModalWindowUIType m_ModalWindowUIType;

	private List<FullScreenUIType> m_FullScreenUITypeStack = new List<FullScreenUIType>();

	private static bool s_UIKitDependenciesInitialized;

	private static readonly DisposableBooleanFlag UIKitDependenciesInitializing = new DisposableBooleanFlag();

	public static RootUIContext Instance => Game.Instance.RootUiContext;

	public string LoadedUIScene { get; private set; }

	public CommonVM CommonVM { get; private set; }

	public SurfaceVM SurfaceVM { get; private set; }

	public SpaceVM SpaceVM { get; private set; }

	public MainMenuVM MainMenuVM { get; private set; }

	public LoadingScreenRootVM LoadingScreenRootVM { get; private set; }

	public BaseUnitEntity PreviousLoadingScreenCompanion { get; set; }

	public bool ServiceWindowNowIsOpening { get; set; }

	public bool IsDebugBlueprintsInformationShow { get; set; }

	public bool IsMainMenu => MainMenuVM != null;

	public bool IsSurface => SurfaceVM != null;

	public bool IsSpace => SpaceVM != null;

	public bool IsLoadingScreen => LoadingScreenRootVM?.LoadingScreenVM.Value != null;

	public bool TooltipIsShown
	{
		get
		{
			if (CommonVM?.TooltipContextVM.TooltipVM.Value == null)
			{
				return CommonVM?.TooltipContextVM.ComparativeTooltipVM.Value != null;
			}
			return true;
		}
	}

	public bool GroupChangerIsShown => SurfaceVM?.StaticPartVM?.GroupChangerContextVM?.GroupChangerVm.Value != null;

	public bool SaveLoadIsShown => CommonVM?.SaveLoadVM.Value != null;

	public bool IsBugReportOpen => CommonVM?.BugReportVM?.Value != null;

	public bool IsLootShow
	{
		get
		{
			if (SurfaceVM?.StaticPartVM.LootContextVM.LootVM.Value == null)
			{
				return SpaceVM?.StaticPartVM.LootContextVM.LootVM.Value != null;
			}
			return true;
		}
	}

	public bool IsLootPlayerStash => (SurfaceVM?.StaticPartVM?.LootContextVM?.LootVM?.Value?.IsPlayerStash).GetValueOrDefault();

	public bool IsLootOneSlot => (SurfaceVM?.StaticPartVM?.LootContextVM?.LootVM?.Value?.IsOneSlot).GetValueOrDefault();

	public bool IsVendorShow
	{
		get
		{
			if (SurfaceVM?.StaticPartVM.VendorVM?.Value == null)
			{
				return SpaceVM?.StaticPartVM.VendorVM?.Value != null;
			}
			return true;
		}
	}

	public bool IsVendorSelectingWindowShow
	{
		get
		{
			if (SurfaceVM?.StaticPartVM.VendorSelectingWindowVM?.Value == null)
			{
				return SpaceVM?.StaticPartVM.VendorSelectingWindowVM?.Value != null;
			}
			return true;
		}
	}

	public bool IsGroupChangerWindowShow
	{
		get
		{
			if (SurfaceVM?.StaticPartVM.GroupChangerContextVM.GroupChangerVm?.Value == null)
			{
				return SpaceVM?.StaticPartVM.GroupChangerContextVM.GroupChangerVm?.Value != null;
			}
			return true;
		}
	}

	public bool IsMessageWindowShow => CommonVM?.MessageBoxVM?.Value != null;

	public bool IsBugReportShow => CommonVM?.BugReportVM?.Value != null;

	public bool IsInventoryShow
	{
		get
		{
			if (SurfaceVM?.StaticPartVM.ServiceWindowsVM?.InventoryVM.Value == null)
			{
				return SpaceVM?.StaticPartVM.ServiceWindowsVM?.InventoryVM.Value != null;
			}
			return true;
		}
	}

	public bool IsCargoShow
	{
		get
		{
			if (SurfaceVM?.StaticPartVM.ServiceWindowsVM.CargoManagementVM.Value == null)
			{
				return SpaceVM?.StaticPartVM.ServiceWindowsVM.CargoManagementVM.Value != null;
			}
			return true;
		}
	}

	public bool IsShipInventoryShown
	{
		get
		{
			if (SurfaceVM?.StaticPartVM.ServiceWindowsVM.ShipCustomizationVM.Value == null)
			{
				return SpaceVM?.StaticPartVM.ServiceWindowsVM.ShipCustomizationVM.Value != null;
			}
			return true;
		}
	}

	public bool IsChargenShown
	{
		get
		{
			if (MainMenuVM?.CharGenContextVM?.CharGenVM.Value == null)
			{
				return SurfaceVM?.StaticPartVM?.CharGenContextVM?.CharGenVM.Value != null;
			}
			return true;
		}
	}

	public bool IsCharInfoAbilitiesChooseMode
	{
		get
		{
			SurfaceVM surfaceVM = SurfaceVM;
			if (surfaceVM == null || !surfaceVM.StaticPartVM.ServiceWindowsVM.CharInfoAbilitiesChooseMode)
			{
				return SpaceVM?.StaticPartVM.ServiceWindowsVM.CharInfoAbilitiesChooseMode ?? false;
			}
			return true;
		}
	}

	public bool IsCharInfoLevelProgression
	{
		get
		{
			SurfaceVM surfaceVM = SurfaceVM;
			if (surfaceVM == null || surfaceVM.StaticPartVM?.ServiceWindowsVM?.CharInfoPageType != CharInfoPageType.LevelProgression)
			{
				SpaceVM spaceVM = SpaceVM;
				if (spaceVM == null)
				{
					return false;
				}
				return spaceVM.StaticPartVM?.ServiceWindowsVM?.CharInfoPageType == CharInfoPageType.LevelProgression;
			}
			return true;
		}
	}

	public static bool IsSpaceCombatRewardsShown => CheckSpaceCombatRewardsShown();

	public bool IsColonyRewardsShown
	{
		get
		{
			SpaceVM spaceVM = SpaceVM;
			if (spaceVM == null)
			{
				return false;
			}
			return spaceVM.StaticPartVM.ServiceWindowsVM.ColonyManagementVM.Value?.ColonyManagementPage.Value?.ColonyRewardsVM?.ShouldShow.Value == true;
		}
	}

	public bool IsShipMoving
	{
		get
		{
			if (IsSpace)
			{
				return SpaceVM.StaticPartVM.ZoneExitVM.ShipIsMoving.Value;
			}
			return false;
		}
	}

	public FullScreenUIType FullScreenUIType => m_FullScreenUIType;

	public ServiceWindowsType CurrentServiceWindow
	{
		get
		{
			if (IsSurface)
			{
				return SurfaceVM.StaticPartVM.ServiceWindowsVM.CurrentWindow;
			}
			if (!IsSpace)
			{
				return ServiceWindowsType.None;
			}
			return SpaceVM.StaticPartVM.ServiceWindowsVM.CurrentWindow;
		}
	}

	public bool IsExplorationWindow
	{
		get
		{
			if (IsSpace)
			{
				return SpaceVM.StaticPartVM.ExplorationVM.IsExploring.Value;
			}
			return false;
		}
	}

	public bool IsInitiativeTrackerActive => (SurfaceVM?.StaticPartVM?.SurfaceHUDVM?.InitiativeTrackerVM?.Value?.ConsoleActive?.Value).GetValueOrDefault();

	public bool CreditsAreShown => MainMenuVM?.CreditsVM?.Value != null;

	public bool QuestNotificatorIsShown => (CommonVM?.QuestNotificatorVM?.IsShowUp?.Value).GetValueOrDefault();

	public bool HasDialog
	{
		get
		{
			if (!IsSpace)
			{
				return SurfaceVM?.StaticPartVM?.DialogContextVM?.DialogVM?.Value != null;
			}
			return SpaceVM?.StaticPartVM?.DialogContextVM?.DialogVM?.Value != null;
		}
	}

	public RootUIContext()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	public static void InitializeUIKitDependencies()
	{
		if (!s_UIKitDependenciesInitialized && !UIKitDependenciesInitializing)
		{
			using (UIKitDependenciesInitializing.Retain())
			{
				LogChannelLoggerWrapper logger = new LogChannelLoggerWrapper(PFLog.UI, "UI");
				UIKitLogger.SetLogger(logger);
				UniRxLogger.SetLogger(logger);
				UIKitSoundManager.SetSoundManager(UISounds.Instance);
				UIKitRewiredCursorController.SetRewiredCursorController(ConsoleCursor.Instance);
				InputLayer.SetCanReceiveInputPredicate(LoadingProcess.Instance.CanReceiveInput);
				GamePadIcons.SetInstance(ConsoleRoot.Instance.Icons);
				GamePad.Instance.SetIsRunOnSteamDeck(ApplicationHelper.IsRunOnSteamDeck);
			}
			s_UIKitDependenciesInitialized = true;
		}
	}

	protected override void DisposeImplementation()
	{
		DisposeUiScene();
		DisposeCommon();
	}

	public void DisposeUiScene()
	{
		MainMenuVM?.Dispose();
		MainMenuVM = null;
		SurfaceVM?.Dispose();
		SurfaceVM = null;
		SpaceVM?.Dispose();
		SpaceVM = null;
		DestroyView(m_UIView);
		m_UIView = null;
		LoadedUIScene = string.Empty;
	}

	public void DisposeMainMenu()
	{
		MainMenuVM?.Dispose();
		MainMenuVM = null;
	}

	private void DisposeCommon()
	{
		CommonVM?.Dispose();
		CommonVM = null;
		DestroyView(m_CommonView);
		m_CommonView = null;
	}

	private void SoftDisposeUiScene()
	{
		MainMenuVM?.DestroyViewRecursive();
		SurfaceVM?.DestroyViewRecursive();
		SpaceVM?.DestroyViewRecursive();
		DestroyView(m_UIView);
		m_UIView = null;
		LoadedUIScene = string.Empty;
	}

	private void SoftDisposeCommon()
	{
		CommonVM?.DestroyViewRecursive();
		DestroyView(m_CommonView);
		m_CommonView = null;
	}

	public IEnumerator InitializeUiSceneCoroutine(string UIScene, Action onComplete = null)
	{
		if (string.IsNullOrEmpty(UIScene))
		{
			UberDebug.LogError("InitializeUiScene: ERROR (IS NULL OR EMPTY)");
			yield break;
		}
		DisposeUiScene();
		LocalizationManager.Instance.Init(SettingsRoot.Game.Main.Localization, SettingsController.Instance, !SettingsRoot.Game.Main.LocalizationWasTouched.GetValue());
		Game.Instance.UISettingsManager.Initialize();
		WidgetFactoryStash.ResetStash();
		EscHotkeyManager.Instance.Initialize();
		InitializeUIKitDependencies();
		RootUIConfig config = GetConfig(UIScene);
		config.Unload();
		IViewModel viewModel = null;
		if (UIScene == GameScenes.MainMenu)
		{
			MainMenuVM = new MainMenuVM();
			viewModel = MainMenuVM;
		}
		else if (UIScene == "UI_Surface_Scene")
		{
			SurfaceVM = new SurfaceVM();
			viewModel = SurfaceVM;
		}
		else if (UIScene == "UI_Space_Scene")
		{
			SpaceVM = new SpaceVM();
			viewModel = SpaceVM;
		}
		yield return null;
		IEnumerator createView = config.TryCreateViewCoroutine(viewModel, m_UIView);
		while (createView.MoveNext())
		{
			yield return null;
		}
		if (m_UIView == null)
		{
			m_UIView = config.View;
		}
		config.Destroy();
		LoadedUIScene = UIScene;
		UIAccess.SoundGameObject = config.gameObject;
		onComplete?.Invoke();
	}

	public void InitializeLoadingScreenScene(LoadingScreenRootVM vm)
	{
		LoadingScreenRootVM = vm;
	}

	public void InitializeLoadingScreenScene(string loadedUIScene)
	{
		RootUIConfig config = GetConfig(loadedUIScene);
		LoadingScreenRootVM = new LoadingScreenRootVM();
		m_UILoadingScreenView = config.TryCreateView(LoadingScreenRootVM);
	}

	public void InitializeCommonScene(string loadedUIScene, bool showLoadingScreen = false)
	{
		if (!string.IsNullOrEmpty(loadedUIScene))
		{
			DisposeCommon();
			RootUIConfig config = GetConfig(loadedUIScene);
			CommonVM = new CommonVM();
			m_CommonView = config.TryCreateView(CommonVM);
			if (showLoadingScreen)
			{
				CommonVM.CloseTutorialOnLoad();
				LoadingScreenRootVM.ShowLoadingScreen();
			}
		}
	}

	private void SoftResetCommonScene(string loadedUIScene)
	{
		if (!string.IsNullOrEmpty(loadedUIScene))
		{
			SoftDisposeCommon();
			RootUIConfig config = GetConfig(loadedUIScene);
			m_CommonView = config.TryCreateView(CommonVM);
		}
	}

	private void SoftResetUiScene(string UIScene)
	{
		if (!string.IsNullOrEmpty(UIScene))
		{
			SoftDisposeUiScene();
			RootUIConfig config = GetConfig(UIScene);
			if (UIScene == GameScenes.MainMenu)
			{
				m_UIView = config.TryCreateView(MainMenuVM);
			}
			else if (UIScene == "UI_Surface_Scene")
			{
				m_UIView = config.TryCreateView(SurfaceVM);
			}
			else if (UIScene == "UI_Space_Scene")
			{
				m_UIView = config.TryCreateView(SpaceVM);
			}
			LoadedUIScene = UIScene;
			config.Unload();
		}
	}

	private static void DestroyView(MonoBehaviour view)
	{
		if (!(view == null))
		{
			UnityEngine.Object.Destroy(view.gameObject);
		}
	}

	public void ResetUI(Action onComplete = null)
	{
		if (!IsLoadingScreen)
		{
			InitializeCommonScene("UI_Common_Scene");
		}
		if (!LoadingProcess.Instance.IsLoadingInProcess)
		{
			LoadingProcess.Instance.StartLoadingProcess(InitializeUiSceneCoroutine(Game.Instance.SceneLoader.LoadedUIScene, onComplete), null, LoadingProcessTag.ResetUI);
		}
		else
		{
			MonoSingleton<CoroutineRunner>.Instance.StartCoroutine(InitializeUiSceneCoroutine(Game.Instance.SceneLoader.LoadedUIScene, onComplete));
		}
	}

	public static bool CanChangeInput()
	{
		if (Game.Instance.CurrentMode != GameModeType.Dialog && Game.Instance.CurrentMode != GameModeType.GameOver && Game.Instance.CurrentMode != GameModeType.BugReport && !Instance.GroupChangerIsShown && !Instance.SaveLoadIsShown && !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.CurrentState != SaveManager.State.Saving && Game.Instance.SaveManager.CurrentState != SaveManager.State.Loading && Instance.LoadingScreenRootVM.GetLoadingScreenState() != LoadingScreenState.Shown && Game.Instance.SaveManager.AreSavesUpToDate)
		{
			return Game.Instance.RootUiContext.FullScreenUIType != FullScreenUIType.Chargen;
		}
		return false;
	}

	public static bool CanChangeLanguage()
	{
		if (Game.Instance.CurrentMode != GameModeType.Dialog && Game.Instance.CurrentMode != GameModeType.GameOver && !Instance.GroupChangerIsShown)
		{
			return !Instance.IsChargenShown;
		}
		return false;
	}

	public bool IsBlockedFullScreenUIType()
	{
		if (!(Game.Instance.CurrentMode == GameModeType.Cutscene) && !(Game.Instance.CurrentMode == GameModeType.Dialog) && !LoadingProcess.Instance.IsLoadingScreenActive && (!Game.Instance.SectorMapTravelController.IsTravelling || !(Game.Instance.CurrentMode == GameModeType.GlobalMap)))
		{
			FullScreenUIType fullScreenUIType = m_FullScreenUIType;
			if (fullScreenUIType != FullScreenUIType.EscapeMenu && fullScreenUIType != FullScreenUIType.TransitionMap && fullScreenUIType != FullScreenUIType.Vendor && fullScreenUIType != FullScreenUIType.Credits && fullScreenUIType != FullScreenUIType.Chargen && fullScreenUIType != FullScreenUIType.NewGame)
			{
				return m_ModalWindowUIType == ModalWindowUIType.GameEndingTitles;
			}
		}
		return true;
	}

	private static bool CheckSpaceCombatRewardsShown()
	{
		if (Instance.SpaceVM == null)
		{
			return false;
		}
		return (Instance.SpaceVM.StaticPartVM.TryGetComponentVM(SpaceStaticComponentType.SpaceCombat) as SpaceCombatVM)?.ExitBattlePopupVM != null;
	}

	private RootUIConfig GetConfig(string loadedUIScene)
	{
		return GetSceneRootObject<RootUIConfig>(loadedUIScene);
	}

	private TSceneCtxConfig GetSceneRootObject<TSceneCtxConfig>(string loadedUIScene) where TSceneCtxConfig : MonoBehaviour
	{
		return (from root in SceneManager.GetSceneByName(loadedUIScene).GetRootGameObjects()
			select root.GetComponent<TSceneCtxConfig>()).FirstOrDefault((TSceneCtxConfig config) => config != null);
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		m_FullScreenUIType = (state ? fullScreenUIType : FullScreenUIType.Unknown);
	}

	public void HandleModalWindowUiChanged(bool state, ModalWindowUIType modalWindowUIType)
	{
		m_ModalWindowUIType = (state ? modalWindowUIType : ModalWindowUIType.Unknown);
	}
}
