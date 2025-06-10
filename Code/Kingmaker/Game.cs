using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;
using Code.GameCore.Mics;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.AI;
using Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;
using Kingmaker.AreaLogic.SceneControllables;
using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Assets.Controllers.GlobalMap;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.Code.Controllers.Interactions;
using Kingmaker.Code.UI.Legacy.BugReportDrawing;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.VM.ChoseControllerMode;
using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.FX;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.MovePrediction;
using Kingmaker.Controllers.Net;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.SpaceCombat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.Controllers.UnityEventsReplacements;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.GameCommands;
using Kingmaker.GameInfo;
using Kingmaker.GameModes;
using Kingmaker.IngameConsole;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Networking.Settings;
using Kingmaker.Networking.Tests;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.Settings;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.TextTools;
using Kingmaker.TextTools.Core;
using Kingmaker.Tutorial;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.Visual;
using Kingmaker.Visual.CharactersRigidbody;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.Critters;
using Kingmaker.Visual.FX;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.Visual.DxtCompressor;
using Owlcat.Runtime.Visual.Effects.ParticleSumEmitter;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker;

public class Game : IGameDoStartMode, IGameDoStopMode, IGameDoSwitchCutsceneLock
{
	public enum ControllerModeType
	{
		Mouse,
		Gamepad
	}

	public class SwitchingModes : ContextFlag<SwitchingModes>
	{
	}

	private readonly struct StartModeInvoker : GameCommandQueue.IInvokable
	{
		private readonly Game m_Game;

		private readonly GameModeType m_Type;

		public StartModeInvoker(Game game, GameModeType type)
		{
			m_Game = game;
			m_Type = type;
		}

		public void Invoke()
		{
			if (m_Game.m_GameModeTicking || (bool)ContextData<SwitchingModes>.Current || 0 < m_Game.GameCommandQueue.Count)
			{
				m_Game.GameCommandQueue.AddCommand(new StartGameModeCommand(m_Type));
			}
			else
			{
				m_Game.DoStartMode(m_Type);
			}
		}
	}

	private readonly struct StopModeInvoker : GameCommandQueue.IInvokable
	{
		private readonly Game m_Game;

		private readonly GameModeType m_Type;

		public StopModeInvoker(Game game, GameModeType type)
		{
			m_Game = game;
			m_Type = type;
		}

		public void Invoke()
		{
			if (m_Game.m_GameModeTicking || (bool)ContextData<SwitchingModes>.Current || 0 < m_Game.GameCommandQueue.Count)
			{
				m_Game.GameCommandQueue.AddCommand(new StopGameModeCommand(m_Type));
			}
			else
			{
				m_Game.DoStopMode(m_Type);
			}
		}
	}

	private class GameModeSwitchOnLoadHandler : IGameModeHandler, ISubscriber
	{
		private readonly Game m_Game;

		public GameModeSwitchOnLoadHandler(Game game)
		{
			m_Game = game;
		}

		public void OnGameModeStart(GameModeType gameMode)
		{
			m_Game.OnAreaLoadGameModeSet();
		}

		public void OnGameModeStop(GameModeType gameMode)
		{
		}
	}

	private class LoadingAreaMarker : IEnumerator
	{
		public readonly BlueprintArea Area;

		public object Current => null;

		public LoadingAreaMarker(BlueprintArea area)
		{
			Area = area;
		}

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Game");

	private static Game s_Instance;

	private ControllerModeType m_ControllerMode;

	private CompositeDisposable m_BugReportDisposable;

	public readonly GameCommandQueue GameCommandQueue = new GameCommandQueue();

	private readonly Stack<GameMode> m_GameModes = new Stack<GameMode>();

	private bool m_GameModeTicking;

	private EntityRef<UnitEntity> m_DefaultUnit;

	private bool m_AlreadyInitialized;

	public readonly ProjectileController ProjectileController = new ProjectileController();

	public readonly Rulebook Rulebook = Rulebook.Instance;

	public readonly PersistentState State;

	public GameStatistic Statistic = new GameStatistic();

	public readonly StaticInfoCollector StaticInfoCollector = new StaticInfoCollector();

	public readonly GamepadInputController GamepadInputController = new GamepadInputController();

	public readonly UnitGroupsController UnitGroupsController = new UnitGroupsController();

	public readonly TurnController TurnController = new TurnController();

	public readonly PauseController PauseController = new PauseController();

	public readonly RealTimeController RealTimeController = new RealTimeController();

	public readonly TimeController TimeController;

	public readonly TimeSpeedController TimeSpeedController = new TimeSpeedController();

	public readonly EntitySpawnController EntitySpawner = new EntitySpawnController();

	public readonly EntityDestructionController EntityDestroyer = new EntityDestructionController();

	public readonly UnitMemoryController UnitMemoryController = new UnitMemoryController();

	public readonly CursorController CursorController = new CursorController();

	public readonly SectorMapController SectorMapController = new SectorMapController();

	public readonly ColonizationController ColonizationController = new ColonizationController();

	public readonly SectorMapTravelController SectorMapTravelController = new SectorMapTravelController();

	public readonly CombatRandomEncounterController CombatRandomEncounterController = new CombatRandomEncounterController();

	public readonly StarSystemMapController StarSystemMapController = new StarSystemMapController();

	public readonly UnitCommandBuffer UnitCommandBuffer = new UnitCommandBuffer();

	public readonly GroupCommandsController GroupCommands = new GroupCommandsController();

	public readonly DialogController DialogController = new DialogController();

	public readonly RealtimeLightsController LightsController = new RealtimeLightsController();

	public readonly AbilityExecutionController AbilityExecutor = new AbilityExecutionController();

	public readonly FogOfWarScheduleController FogOfWar = new FogOfWarScheduleController();

	public readonly FogOfWarCompleteController FogOfWarComplete = new FogOfWarCompleteController();

	public readonly EntityVisibilityForPlayerController EntityVisibilityForPlayerController = new EntityVisibilityForPlayerController();

	public readonly FollowersFormationController FollowersFormationController = new FollowersFormationController();

	public readonly ProjectileSpawnerController ProjectileSpawnerController = new ProjectileSpawnerController();

	public readonly UISettingsManager UISettingsManager = new UISettingsManager();

	public readonly UpdateController<DirectorAdapter> DirectorAdapterController = new UpdateController<DirectorAdapter>(TickType.Simulation);

	public readonly UpdateController<IUpdatable> CustomUpdateBeforePhysicsController = new UpdateController<IUpdatable>(TickType.Simulation);

	public readonly UpdateController<IUpdatable> CustomUpdateController = new UpdateController<IUpdatable>(TickType.Simulation);

	public readonly UpdateController<Bird> BirdUpdateController = new UpdateController<Bird>(TickType.Simulation);

	public readonly UpdateController<FogOfWarRevealerTrigger> FogOfWarRevealerTriggerController = new UpdateController<FogOfWarRevealerTrigger>(TickType.Simulation);

	public readonly UpdateController<IUpdatable> DoorUpdateController = new UpdateController<IUpdatable>(TickType.Simulation);

	public readonly EntityBoundsController EntityBoundsController = new EntityBoundsController();

	public readonly SelectionCharacterController SelectionCharacter = new SelectionCharacterController();

	public readonly AiBrainController AiBrainController = new AiBrainController();

	public readonly AnimationManagerController AnimationManagerController = new AnimationManagerController();

	public readonly UpdateController<RigidbodyCreatureController> UpdateRigidbodyCreatureController = new UpdateController<RigidbodyCreatureController>(TickType.Simulation);

	public readonly CustomCallbackController CustomCallbackController = new CustomCallbackController();

	public readonly AttackOfOpportunityController AttackOfOpportunityController = new AttackOfOpportunityController();

	public readonly UpdateController<IUpdatable> CustomLateUpdateController = new UpdateController<IUpdatable>(TickType.EndOfFrame);

	public readonly InterpolationController<IInterpolatable> InterpolationController = new InterpolationController<IInterpolatable>();

	public readonly CoroutinesController CoroutinesController = new CoroutinesController(TickType.Simulation);

	public readonly SynchronizedDataController SynchronizedDataController = new SynchronizedDataController();

	public readonly StarshipPathController StarshipPathController = new StarshipPathController();

	public readonly MeteorStreamController MeteorStreamController = new MeteorStreamController();

	public readonly SyncStateCheckerController SyncStateCheckerController = new SyncStateCheckerController();

	public readonly InteractionFXController InteractionFXController = new InteractionFXController();

	public readonly UnitMovableAreaController UnitMovableAreaController = new UnitMovableAreaController();

	public readonly CustomGridNodeController CustomGridNodeController = new CustomGridNodeController();

	public readonly ForcedCoversController ForcedCoversController = new ForcedCoversController();

	public readonly SceneControllablesController SceneControllables = new SceneControllablesController();

	public readonly MovePredictionController MovePredictionController = new MovePredictionController();

	public readonly CameraFXController CameraFXController = new CameraFXController();

	public readonly CameraFXSoundController CameraFXSoundController = new CameraFXSoundController();

	public readonly PlayerInputInCombatController PlayerInputInCombatController = new PlayerInputInCombatController();

	public readonly PointerController DefaultPointerController = new PointerController(new ClickWithSelectedAbilityHandler(), new ClickUnitHandler(), new ClickMapObjectHandler(), new SectorMapClickObjectHandler(), new ClickGroundHandler(), new ClickOnDetectClicksObjectHandler(), new ClickSurfaceDeploymentHandler());

	public readonly GpuCrowdController GpuCrowdController = new GpuCrowdController();

	public readonly SaveManager SaveManager = new SaveManager();

	public readonly VendorLogic Vendor = new VendorLogic();

	private Runner m_Runner;

	[CanBeNull]
	private static BlueprintAreaPreset s_NewGamePreset;

	[CanBeNull]
	private static SaveInfo s_ImportSave;

	[CanBeNull]
	private static BaseUnitEntity s_NewGameUnit;

	[CanBeNull]
	private static BaseUnitEntity s_NewGameShip;

	private bool m_WillBePaused;

	private bool m_AreaWasSwitched;

	public bool PauseRequestLastValue;

	private ServiceProxy<SummonPoolsManager> m_SummonPoolsProxy;

	private bool m_MatchTimeOfDayScheduled;

	public bool InvertPauseButtonPressed;

	private int[] m_ModesCount = new int[0];

	private float m_LoadingProgress;

	private float m_LoadingScenesProgress;

	private GameModeSwitchOnLoadHandler m_GameModeSwitchOnLoadHandler;

	private bool m_WasLoadingForTheAssert;

	public ControllerModeType ControllerMode
	{
		get
		{
			return m_ControllerMode;
		}
		set
		{
			m_ControllerMode = value;
			m_BugReportDisposable?.Dispose();
			m_BugReportDisposable = null;
			Input.simulateMouseWithTouches = m_ControllerMode != ControllerModeType.Gamepad;
			if (m_ControllerMode == ControllerModeType.Gamepad)
			{
				m_BugReportDisposable = BugReportControls.AddBugReportControls();
			}
			if (BuildModeUtility.IsDevelopment)
			{
				return;
			}
			try
			{
				Cursor.visible = value == ControllerModeType.Mouse;
			}
			catch
			{
			}
		}
	}

	public bool IsControllerMouse => ControllerMode == ControllerModeType.Mouse;

	public bool IsControllerGamepad => ControllerMode == ControllerModeType.Gamepad;

	private static SceneLoader m_SceneLoader => SceneLoader.Instance;

	public bool AlreadyInitialized => m_AlreadyInitialized;

	public KeyboardAccess Keyboard => KeyboardAccess.Instance;

	public VirtualPositionController VirtualPositionController { get; private set; }

	public PointerController ClickEventsController { get; private set; }

	public CameraController CameraController { get; private set; }

	public InteractionHighlightController InteractionHighlightController { get; private set; }

	public UnitHandEquipmentController HandsEquipmentController { get; private set; }

	public static bool IsInMainMenu => !Runner.IsActive;

	[CanBeNull]
	public static BlueprintAreaPreset NewGamePreset
	{
		get
		{
			return s_NewGamePreset;
		}
		set
		{
			PFLog.System.Log($"NewGamePreset setter | old={s_NewGamePreset} | new={value}");
			s_NewGamePreset = value;
			ImportSave = null;
		}
	}

	[CanBeNull]
	public static SaveInfo ImportSave
	{
		get
		{
			return s_ImportSave;
		}
		set
		{
			PFLog.System.Log($"ImportSave setter | old={s_ImportSave} | new={value}");
			s_ImportSave = value;
		}
	}

	[CanBeNull]
	public static BaseUnitEntity NewGameUnit
	{
		get
		{
			return s_NewGameUnit;
		}
		set
		{
			PFLog.System.Log($"NewGameUnit setter | old={s_NewGameUnit} | new={value}");
			s_NewGameUnit = value;
		}
	}

	[CanBeNull]
	public static BaseUnitEntity NewGameShip
	{
		get
		{
			return s_NewGameShip;
		}
		set
		{
			PFLog.System.Log($"NewGameShip setter | old={s_NewGameShip} | new={value}");
			s_NewGameShip = value;
		}
	}

	public static Game Instance
	{
		get
		{
			if (s_Instance == null && (Application.isPlaying || ContextData<BlueprintUnitCheckerInEditorContextData>.Current != null))
			{
				EnsureGameLifetimeServices();
				s_Instance = new Game();
			}
			return s_Instance;
		}
	}

	public static bool HasInstance => s_Instance != null;

	public List<UnitGroup> UnitGroups => UnitGroupsController.Groups;

	public List<UnitGroup> ReadyForCombatUnitGroups => UnitGroupsController.AwakeGroups;

	public BlueprintRoot BlueprintRoot => BlueprintRootReferenceHelper.GetRoot();

	public AreaPersistentState LoadedAreaState => State.LoadedAreaState;

	public TimeOfDay TimeOfDay { get; private set; }

	public Player Player => State.PlayerState;

	public BlueprintArea CurrentlyLoadedArea => m_SceneLoader.CurrentlyLoadedArea;

	public BlueprintAreaPart CurrentlyLoadedAreaPart => m_SceneLoader.CurrentlyLoadedAreaPart;

	public RootGroup DynamicRoot => m_SceneLoader.DynamicRoot;

	public RootGroup CrossSceneRoot => m_SceneLoader.CrossSceneRoot;

	public CoopData CoopData => State.CoopData;

	public RootUIContext RootUiContext { get; private set; } = new RootUIContext();


	public SceneLoader SceneLoader => m_SceneLoader;

	public bool IsUnloading { get; set; }

	public bool IsPaused
	{
		get
		{
			return IsModeActive(GameModeType.Pause);
		}
		set
		{
			if (value != IsPaused)
			{
				GameCommandQueue.SetPauseManualState(value);
			}
		}
	}

	public SummonPoolsManager SummonPools
	{
		get
		{
			m_SummonPoolsProxy = ((m_SummonPoolsProxy?.Instance != null) ? m_SummonPoolsProxy : Services.GetProxy<SummonPoolsManager>());
			if (m_SummonPoolsProxy?.Instance == null)
			{
				Services.RegisterServiceInstance(new SummonPoolsManager());
				m_SummonPoolsProxy = Services.GetProxy<SummonPoolsManager>();
			}
			return m_SummonPoolsProxy?.Instance;
		}
	}

	public UnitEntity DefaultUnit
	{
		get
		{
			if (m_DefaultUnit.Entity == null)
			{
				m_DefaultUnit = Entity.Initialize(new UnitEntity("<default-unit>", isInGame: false, BlueprintRoot.SystemMechanics.DefaultUnit));
			}
			return m_DefaultUnit;
		}
	}

	public GameModeType CurrentMode
	{
		get
		{
			if (m_GameModes.Count > 0)
			{
				return m_GameModes.Peek().Type;
			}
			return GameModeType.None;
		}
	}

	public bool IsSpaceCombat => CurrentMode == GameModeType.SpaceCombat;

	public ClickWithSelectedAbilityHandler SelectedAbilityHandler { get; private set; }

	public bool PauseOnLoadPending { get; set; }

	public LevelUpController LevelUpController { get; set; }

	public float UILoadingProgress => m_LoadingProgress * 0.2f + m_LoadingScenesProgress * 0.8f;

	public bool IsLoadingProgressPaused { get; private set; }

	private int[] ModesCount
	{
		get
		{
			if (m_ModesCount.Length < GameModeType.Count)
			{
				int[] array = new int[GameModeType.Count];
				Array.Copy(m_ModesCount, array, m_ModesCount.Length);
				m_ModesCount = array;
			}
			return m_ModesCount;
		}
	}

	public static ControllerModeType? ControllerOverride
	{
		get
		{
			string text = BuildModeUtility.Data?.ForceControllerMode;
			if (text.IsNullOrEmpty())
			{
				return null;
			}
			if (text.ToLower() == "gamepad")
			{
				return ControllerModeType.Gamepad;
			}
			if (text.ToLower() == "mouse")
			{
				return ControllerModeType.Mouse;
			}
			PFLog.Default.Error("Strange value in ForceControllerMode in startup.json - defaulting to mouse");
			return ControllerModeType.Mouse;
		}
	}

	public static bool DontChangeController { get; set; }

	public static float CombatAnimSpeedUp
	{
		get
		{
			if (!Instance.Player.IsInCombat)
			{
				return 1f;
			}
			if (!Instance.TurnController.IsPlayerTurn)
			{
				return Instance.Player.UISettings.AnimSpeedUpNPC;
			}
			return Instance.Player.UISettings.AnimSpeedUpPlayer;
		}
	}

	public void RequestPauseUi(bool isPaused)
	{
		if (!IsInMainMenu)
		{
			PauseRequestLastValue = isPaused;
		}
	}

	private void TryDoPauseRequest()
	{
		if (!IsInMainMenu)
		{
			PauseController.RequestPauseUi(PauseRequestLastValue);
		}
	}

	public void SetIsPauseForce(bool value)
	{
		if (value != IsPaused && !m_WillBePaused && !(LoadingProcess.Instance.IsManualLoadingScreenActive && value))
		{
			if (value)
			{
				m_WillBePaused = true;
				DefaultPointerController.SkipDeactivation = true;
				StartMode(GameModeType.Pause);
			}
			else
			{
				DefaultPointerController.SkipDeactivation = true;
				StopMode(GameModeType.Pause);
			}
		}
	}

	private Game()
	{
		TimeController = GetTimeController();
		State = GetPersistentState();
	}

	public bool IsModeActive(GameModeType gameModeType)
	{
		return ModesCount[(int)gameModeType] > 0;
	}

	private TimeController GetTimeController()
	{
		ITimeController timeController = InterfaceServiceLocator.TryGetService<ITimeController>();
		if (timeController != null)
		{
			return (TimeController)timeController;
		}
		TimeController timeController2 = new TimeController();
		InterfaceServiceLocator.RegisterService(timeController2, typeof(ITimeController));
		return timeController2;
	}

	private PersistentState GetPersistentState()
	{
		IPersistentState persistentState = InterfaceServiceLocator.TryGetService<IPersistentState>();
		if (persistentState != null)
		{
			return (PersistentState)persistentState;
		}
		PersistentState persistentState2 = new PersistentState();
		InterfaceServiceLocator.RegisterService(persistentState2, typeof(IPersistentState));
		return persistentState2;
	}

	public void Initialize()
	{
		if (m_AlreadyInitialized)
		{
			return;
		}
		PFLog.Default.Log("Initializing Game. Version: " + GameVersion.GetVersion());
		using (CodeTimer.New("Game ctor"))
		{
			if (!DontChangeController)
			{
				ControllerMode = (GamepadConnectDisconnectVM.GamepadIsConnected ? ControllerModeType.Gamepad : ControllerModeType.Mouse);
			}
			if (ControllerOverride.HasValue)
			{
				ControllerMode = ControllerOverride.Value;
			}
			CursorController.Activate();
			TextTemplateEngineProxy.Instance.Initialize(TextTemplateEngine.Instance);
			LocalizationManager.Instance.Init(SettingsRoot.Game.Main.Localization, SettingsController.Instance, !SettingsRoot.Game.Main.LocalizationWasTouched.GetValue());
			Keyboard.RegisterBuiltinBindings();
			Screenshot.Initialize(Keyboard);
			Keyboard.Bind("QuickSave", delegate
			{
				MakeQuickSave();
			});
			Keyboard.Bind("QuickLoad", QuickLoadGame);
			Keyboard.Bind("Stop", delegate
			{
				UIAccess.SelectionManager.Stop();
			});
			Keyboard.Bind("Hold", delegate
			{
				UIAccess.SelectionManager.Hold();
			});
			if (BlueprintRootReferenceHelper.GetRoot() == null)
			{
				Debug.LogError("Boom! No root!");
			}
			using (CodeTimer.New("Initialize Cheats"))
			{
				if (BuildModeUtility.IsDevelopment)
				{
					CheatsCommon.RegisterCheats(Keyboard);
					CheatsRelease.RegisterCheats(Keyboard);
				}
				CheatsManagerHolder.System.Database.SetExternals(SmartConsole.CommandNames);
			}
			Keyboard.Bind("Pause", Pause);
			Keyboard.Bind("EndTurn", TryEndTurnBind);
			Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.SkipBark.name, GameCommandQueue.SkipBark);
			Keyboard.Bind("UnpauseOn", UnpauseBindOn);
			Keyboard.Bind("UnpauseOff", UnpauseBindOff);
			UICamera.Claim();
			Player.InitializeHack();
			SaveManager.UpdateSaveListAsync();
			SceneEntitiesState.OnAdded += CrossSceneStateHandler;
			SceneEntitiesState.OnRemoved += CrossSceneStateHandler;
			GameHistoryLog.Initialize();
			m_AlreadyInitialized = true;
		}
	}

	private void Pause()
	{
		if (IsPaused)
		{
			PauseBind();
		}
		else if (!UIUtility.IsGlobalMap())
		{
			PauseBind();
		}
	}

	private void TryEndTurnBind()
	{
		if (!IsPaused && !UIUtility.IsGlobalMap())
		{
			EndTurnBind();
		}
	}

	public void UnpauseBindOff()
	{
		InvertPauseButtonPressed = false;
	}

	public void UnpauseBindOn()
	{
		InvertPauseButtonPressed = true;
	}

	public void PauseBind()
	{
		IsPaused = !TurnController.TurnBasedModeActive && !UIUtility.IsGlobalMap() && !IsPaused;
	}

	public void EndTurnBind()
	{
		if (!TurnController.TurnBasedModeActive || (TurnController.IsSpaceCombat && !UINetUtility.IsControlMainCharacter()))
		{
			return;
		}
		if (TurnController.IsPreparationTurn)
		{
			if (TurnController.CanFinishDeploymentPhase())
			{
				UISounds.Instance.Sounds.Combat.EndTurn.Play();
				TurnController.RequestEndPreparationTurn();
			}
		}
		else if (TurnController.IsPlayerTurn && TurnController.CurrentUnit.IsMyNetRole() && TurnController.CurrentUnit.GetCommandsOptional()?.Current == null)
		{
			TurnController.TryEndPlayerTurnManually();
		}
	}

	public void SpeedUp(bool state)
	{
		if (!IsPaused && !UIUtility.IsGlobalMap() && TurnController.TurnBasedModeActive && (!TurnController.IsSpaceCombat || UINetUtility.IsControlMainCharacter()) && !TurnController.IsPreparationTurn)
		{
			if (state)
			{
				GameCommandQueue.DoSpeedUp();
			}
			else
			{
				GameCommandQueue.StopSpeedUp();
			}
		}
	}

	public static void EnsureGameLifetimeServices()
	{
		if (Services.GetInstance<SoundState>() == null)
		{
			Services.RegisterServiceInstance(new SoundState());
		}
		if (Services.GetInstance<DxtCompressorService>() == null)
		{
			Services.RegisterServiceInstance(new DxtCompressorService());
		}
		if (Services.GetInstance<CharacterAtlasService>() == null)
		{
			Services.RegisterServiceInstance(new CharacterAtlasService());
		}
		if (Services.GetInstance<FXPrewarmService>() == null)
		{
			Services.RegisterServiceInstance(new FXPrewarmService());
		}
		if (Services.GetInstance<LogThreadService>() == null)
		{
			Services.RegisterServiceInstance(new LogThreadService());
		}
		if (Services.GetInstance<ReportingUtils>() == null)
		{
			Services.RegisterServiceInstance(new ReportingUtils());
		}
		if (Services.GetInstance<MouseHoverBlueprintSystem>() == null)
		{
			Services.RegisterServiceInstance(new MouseHoverBlueprintSystem());
		}
		if (Services.GetInstance<EscHotkeyManager>() == null)
		{
			Services.RegisterServiceInstance(new EscHotkeyManager());
		}
		if (Services.GetInstance<UISounds>() == null)
		{
			Services.RegisterServiceInstance(new UISounds());
		}
		UnitAsksService.Ensure();
	}

	public void Tick()
	{
		StateUnchangedOutsideTickCheck.BeforeTick();
		bool isLoadingInProcess = LoadingProcess.Instance.IsLoadingInProcess;
		LoadingProcess.Instance.Tick();
		NetworkingManager.ReceivePackets();
		int num = 9;
		do
		{
			TickInternal(isLoadingInProcess);
			ContextData.Check();
			num--;
			if (num == 0)
			{
				if (!BuildModeUtility.Data.LimitDeltaTimeForProfiling)
				{
					PFLog.Replay.Log("Game.Tick: max tick count per frame has been exceeded!");
				}
				break;
			}
		}
		while (RealTimeController.OneMoreTick);
		StateUnchangedOutsideTickCheck.AfterTick();
		Services.GetInstance<CharacterAtlasService>()?.Update();
		Services.GetInstance<FXPrewarmService>()?.Update();
	}

	private void TickInternal(bool wasLoadingInProcess)
	{
		if (!IsLoadingInProcess())
		{
			NetService.Instance.Init();
			using (ProfileScope.New("Start Tick Real Time"))
			{
				RealTimeController.Tick();
			}
			TryDoPauseRequest();
			GameCommandQueue.Tick();
			if (m_WasLoadingForTheAssert && !RealTimeController.IsSimulationTick)
			{
				throw new Exception("Logic error in loading process. Expecting System tick as first tick after loading");
			}
			if (m_GameModes.Count <= 0)
			{
				throw new Exception("Logic error: we have zero game modes");
			}
			m_WasLoadingForTheAssert = false;
		}
		if (IsLoadingInProcess())
		{
			SoundState.Instance.UpdateScheduledAreaMusic();
			RealTimeController.Suspend();
			TimeController.Suspend();
			m_WasLoadingForTheAssert = true;
			return;
		}
		using (ProfileScope.New("Update SoundState"))
		{
			SoundState.Instance.Update();
		}
		if (LoadingProcess.Instance.IsLoadingScreenActive && m_GameModes.Count <= 0)
		{
			return;
		}
		try
		{
			m_GameModeTicking = true;
			using (ProfileScope.New("Cleanup Awake Units"))
			{
				State.AllAwakeUnits.RemoveAll((AbstractUnitEntity i) => !i.IsInState);
			}
			using (ProfileScope.New("Tick Game Mode"))
			{
				m_GameModes.Peek().Tick();
			}
			using (ProfileScope.New("Tick UnitAsksController"))
			{
				UnitAsksService.Instance.Tick();
			}
		}
		finally
		{
			m_GameModeTicking = false;
		}
		if (PauseOnLoadPending)
		{
			PauseOnLoadPending = false;
			IsPaused = true;
		}
		Services.GetInstance<DxtCompressorService>()?.Update();
		using (ProfileScope.New("Finish Tick Real Time"))
		{
			RealTimeController.FinishTick();
		}
		bool IsLoadingInProcess()
		{
			if (!wasLoadingInProcess)
			{
				return LoadingProcess.Instance.IsLoadingInProcess;
			}
			return true;
		}
	}

	public void HandleQuit()
	{
		if (!IsInMainMenu && SettingsRoot.Difficulty.OnlyOneSave.GetValue())
		{
			SaveManager.WaitCommit();
			SaveGame(SaveManager.GetNextAutoslot());
			Thread.Sleep(100);
		}
		SmartConsole.ClearRegistrations();
		CheatsManagerHolder.System.Database.SetExternals(SmartConsole.CommandNames);
		PlayerPrefs.Save();
		Statistic.Quit();
		StaticInfoCollector.Quit();
		SaveManager.WaitCommit();
		AreaDataStash.CloseAndDelete();
		Owlcat.Runtime.Core.Logging.Logger.Instance.DisposeLogSinks();
	}

	public void StartMode(GameModeType type)
	{
		if (type == CurrentMode)
		{
			PFLog.Default.Error("Game mode with type {0} already active", type);
		}
		else if (Player.GameOverReason.HasValue && type != GameModeType.GameOver)
		{
			PFLog.Default.Error("Cannot enter game mode {0}: game is over", type);
		}
		else
		{
			PFLog.System.Log("Start mode request {0}", type);
			GameCommandQueue.LockAndRun(new StartModeInvoker(this, type));
		}
	}

	public void StopMode(GameModeType type)
	{
		PFLog.System.Log("Stop mode request {0}", type);
		GameCommandQueue.LockAndRun(new StopModeInvoker(this, type));
	}

	void IGameDoStartMode.DoStartMode(GameModeType type)
	{
		DoStartMode(type);
	}

	void IGameDoStopMode.DoStopMode(GameModeType type)
	{
		DoStopMode(type);
	}

	void IGameDoSwitchCutsceneLock.DoSwitchCutsceneLock(bool @lock)
	{
		DoSwitchCutsceneLock(@lock);
	}

	private void DoStartMode(GameModeType type)
	{
		using (ContextData<SwitchingModes>.Request())
		{
			if (type == GameModeType.Pause && m_WillBePaused)
			{
				m_WillBePaused = false;
			}
			if (type == GameModeType.Pause)
			{
				GameModeType currentMode = CurrentMode;
				if (currentMode != GameModeType.Default && currentMode != GameModeType.Cutscene && currentMode != GameModeType.GlobalMap)
				{
					PFLog.System.Log("Preventing starting {0} over {1}", type, currentMode);
					return;
				}
			}
			if (type == GameModeType.Default || type == GameModeType.Dialog || type == GameModeType.Cutscene)
			{
				while (CurrentMode == GameModeType.Pause)
				{
					PFLog.System.Log("Stopping Pause before starting {0}", type);
					DoStopMode(GameModeType.Pause);
				}
			}
			using (ProfileScope.New($"Start Mode ({type})"))
			{
				ClearPublicControllers();
				GameMode gameMode = (m_GameModes.Empty() ? null : m_GameModes.Peek());
				GameMode gameMode2 = GamesModeFactoryFacade.Instance.Create(type);
				ModesCount[(int)type]++;
				gameMode?.OnDisable();
				m_GameModes.Push(gameMode2);
				gameMode2.OnStart(gameMode);
				gameMode2.OnEnable();
				SetupPublicControllers();
				PFLog.System.Log("Started mode {0} (previous mode {1})", CurrentMode, gameMode?.Type);
				HandleGameModeChanged(gameMode?.Type ?? GameModeType.None, gameMode2.Type);
			}
		}
	}

	private void DoStopMode(GameModeType type)
	{
		if (type != CurrentMode)
		{
			PFLog.Default.Warning("Cannot stop game mode {0}, mode {1} is active", type, CurrentMode);
			return;
		}
		using (ContextData<SwitchingModes>.Request())
		{
			using (ProfileScope.New($"Stop Mode ({type})"))
			{
				ClearPublicControllers();
				GameMode gameMode = m_GameModes.Pop();
				GameMode gameMode2 = (m_GameModes.Empty() ? null : m_GameModes.Peek());
				ModesCount[(int)type]--;
				gameMode.OnDisable();
				gameMode.OnStop(gameMode2);
				gameMode2?.OnEnable();
				SetupPublicControllers();
				PFLog.System.Log("Stopped mode {0} (next mode on stack {1})", gameMode.Type, CurrentMode);
				HandleGameModeChanged(gameMode.Type, gameMode2?.Type ?? GameModeType.None);
			}
		}
	}

	private void DoSwitchCutsceneLock(bool @lock)
	{
		if (CurrentMode == GameModeType.None || (IsModeActive(GameModeType.CutsceneGlobalMap) || IsModeActive(GameModeType.Cutscene)) == @lock)
		{
			return;
		}
		GameModeType type = (IsModeActive(GameModeType.GlobalMap) ? GameModeType.CutsceneGlobalMap : GameModeType.Cutscene);
		if (@lock)
		{
			DoStartMode(type);
		}
		else
		{
			DoStopMode(type);
		}
		foreach (BaseUnitEntity partyAndPet in Player.PartyAndPets)
		{
			partyAndPet.Commands.InterruptAll((AbstractUnitCommand c) => !c.FromCutscene);
			UnitPartFollowedByUnits optional = partyAndPet.GetOptional<UnitPartFollowedByUnits>();
			if (optional == null || optional.Followers.Count <= 0)
			{
				continue;
			}
			foreach (AbstractUnitEntity follower in optional.Followers)
			{
				follower.Commands.InterruptAll((AbstractUnitCommand c) => !c.FromCutscene);
			}
		}
		NetService.Instance.CancelCurrentCommands();
	}

	private void StopAllModes()
	{
		int num = 0;
		while (!m_GameModes.Empty())
		{
			DoStopMode(CurrentMode);
			if (num++ > 100)
			{
				PFLog.Default.Error("Could not stop all game modes properly");
				m_GameModes.Clear();
				break;
			}
		}
	}

	[CanBeNull]
	public T GetController<T>(bool includeInactive = false) where T : class, IController
	{
		if (m_GameModes.Count == 0)
		{
			return null;
		}
		T controller = m_GameModes.Peek().GetController<T>();
		if (controller != null || !includeInactive)
		{
			return controller;
		}
		foreach (GameMode gameMode in m_GameModes)
		{
			if (gameMode != m_GameModes.Peek())
			{
				controller = gameMode.GetController<T>();
				if (controller != null)
				{
					return controller;
				}
			}
		}
		return null;
	}

	private void HandleGameModeChanged(GameModeType oldMode, GameModeType newMode)
	{
		EventBus.RaiseEvent(delegate(IGameModeHandler h)
		{
			h.OnGameModeStop(oldMode);
		});
		if (oldMode == GameModeType.Pause || newMode == GameModeType.Pause)
		{
			EventBus.RaiseEvent(delegate(IPauseHandler h)
			{
				h.OnPauseToggled();
			});
		}
		EventBus.RaiseEvent(delegate(IGameModeHandler h)
		{
			h.OnGameModeStart(newMode);
		});
	}

	private void ClearPublicControllers()
	{
		if (DragNDropManager.Instance != null)
		{
			DragNDropManager.Instance.CancelDrag();
		}
		VirtualPositionController = null;
		ClickEventsController = null;
		SelectedAbilityHandler = null;
		InteractionHighlightController = null;
	}

	private void SetupPublicControllers()
	{
		GameModeType currentMode = CurrentMode;
		if (currentMode == GameModeType.Default || currentMode == GameModeType.Pause || currentMode == GameModeType.SpaceCombat)
		{
			ClickEventsController = m_GameModes.Peek().GetController<PointerController>();
			SelectedAbilityHandler = ClickEventsController.GetHandler<ClickWithSelectedAbilityHandler>();
		}
		else if (currentMode == GameModeType.GlobalMap || currentMode == GameModeType.StarSystem)
		{
			ClickEventsController = m_GameModes.Peek().GetController<PointerController>();
		}
		if (CurrentMode != GameModeType.None)
		{
			InteractionHighlightController = m_GameModes.Peek().GetController<InteractionHighlightController>();
			HandsEquipmentController = m_GameModes.Peek().GetController<UnitHandEquipmentController>();
			CameraController = m_GameModes.Peek().GetController<CameraController>();
			VirtualPositionController = m_GameModes.Peek().GetController<VirtualPositionController>();
		}
	}

	public void AdvanceGameTime(TimeSpan delta)
	{
		TimeController.AdvanceGameTime(delta);
	}

	public void HandleAreaBeginUnloading()
	{
		foreach (BaseUnitEntity allCrossSceneUnit in Player.AllCrossSceneUnits)
		{
			if (allCrossSceneUnit.IsInCombat)
			{
				allCrossSceneUnit.CombatState.LeaveCombat();
			}
		}
		EventBus.RaiseEvent(delegate(IAreaHandler h)
		{
			h.OnAreaBeginUnloading();
		});
		EntitySpawner.Tick();
		EntityDestroyer.Tick();
		StopAllModes();
		FxHelper.DestroyAll();
		GameObjectsPool.ClearPool();
		EntityBoundsController.HandleAreaBeginUnloading();
	}

	public void HandleAdditiveAreaBeginDeactivating()
	{
		foreach (BaseUnitEntity allCrossSceneUnit in Player.AllCrossSceneUnits)
		{
			if (allCrossSceneUnit.IsInCombat)
			{
				allCrossSceneUnit.CombatState.LeaveCombat();
			}
		}
		EventBus.RaiseEvent(delegate(IAdditiveAreaSwitchHandler h)
		{
			h.OnAdditiveAreaBeginDeactivated();
		});
		EntitySpawner.Tick();
		EntityDestroyer.Tick();
		StopAllModes();
		FxHelper.DestroyAll();
		GameObjectsPool.ClearPool();
	}

	public void LoadArea([NotNull] BlueprintAreaEnterPoint areaEnterPoint, AutoSaveMode autoSaveMode, Action callback = null)
	{
		if (areaEnterPoint == null)
		{
			throw new ArgumentException("areaEnterPoint is null", "areaEnterPoint");
		}
		DialogController.Tick();
		if (CurrentlyLoadedArea == areaEnterPoint.Area)
		{
			Teleport(areaEnterPoint, includeFollowers: true, callback);
		}
		else
		{
			LoadArea(areaEnterPoint.Area, areaEnterPoint, autoSaveMode, null, callback);
		}
	}

	public bool IsAreaLoaded(BlueprintArea area)
	{
		return m_SceneLoader.IsAreaLoaded(area);
	}

	public void Teleport([NotNull] BlueprintAreaEnterPoint areaEnterPoint, bool includeFollowers = false, Action callback = null)
	{
		if (areaEnterPoint == null)
		{
			throw new ArgumentException("areaEnterPoint is null", "areaEnterPoint");
		}
		if (CurrentlyLoadedArea != areaEnterPoint.Area)
		{
			throw new InvalidOperationException($"Cant teleport to {areaEnterPoint}. Target zone {areaEnterPoint.Area} should be same as current {CurrentlyLoadedArea}");
		}
		LoadingProcess.Instance.StartLoadingProcess(TeleportPartyCoroutine(areaEnterPoint, includeFollowers), delegate
		{
			ExecuteSafe(callback);
		}, LoadingProcessTag.TeleportParty);
		EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
		{
			h.HandleAreaTransition();
		});
	}

	private IEnumerator<object> TeleportPartyCoroutine([NotNull] BlueprintAreaEnterPoint areaEnterPoint, bool includeFollowers)
	{
		if (areaEnterPoint == null)
		{
			throw new ArgumentException("areaEnterPoint is null", "areaEnterPoint");
		}
		BlueprintAreaPart areaPart = areaEnterPoint.AreaPart;
		if (areaPart == null)
		{
			throw new Exception(string.Format("{0} {1} has null {2}. Most likely this {3} is outside of mechanics bounds of any {4}.", "BlueprintAreaEnterPoint", areaEnterPoint, "AreaPart", "BlueprintAreaEnterPoint", "AreaPart"));
		}
		AreaEnterPoint enterPoint = AreaEnterPoint.FindAreaEnterPointOnScene(areaEnterPoint);
		if (enterPoint == null)
		{
			throw new Exception($"Cant find view of area enter point {areaEnterPoint}");
		}
		LoadingProcess.Instance.StartLoadingProcess(MatchTimeOfDayCoroutine(), null, LoadingProcessTag.TeleportParty);
		NetService.Instance.CancelCurrentCommands();
		if (CurrentlyLoadedAreaPart != areaPart)
		{
			IEnumerator switchPart = m_SceneLoader.SwitchToAreaPartCoroutine(areaPart);
			while (switchPart.MoveNext())
			{
				yield return null;
			}
		}
		Player.MoveCharacters(enterPoint, includeFollowers, moveCamera: true);
	}

	public void LoadAdditiveArea(BlueprintArea area)
	{
		LoadingProcess.Instance.StartLoadingProcess(m_SceneLoader.LoadAdditiveArea(area), null, LoadingProcessTag.Save);
	}

	public void UnloadAdditiveArea(BlueprintArea area)
	{
		LoadingProcess.Instance.StartLoadingProcess(m_SceneLoader.UnloadAdditiveArea(area), null, LoadingProcessTag.Save);
	}

	public void ActivateAdditiveArea(BlueprintAreaEnterPoint enterPoint, bool showLoadingScreen, AutoSaveMode autoSaveMode = AutoSaveMode.None)
	{
		LoadingProcessTag processTag = ((!showLoadingScreen) ? LoadingProcessTag.TeleportParty : LoadingProcessTag.None);
		if (CurrentMode == GameModeType.Dialog && !DialogController.DialogStopScheduled && GameCommandQueue.ContainsCommand((StopGameModeCommand x) => x.GameMode == GameModeType.Dialog))
		{
			GameCommandQueue.Tick();
		}
		if (autoSaveMode == AutoSaveMode.BeforeExit)
		{
			LoadingProcess.Instance.StartLoadingProcess(SaveManager.SaveRoutine(SaveManager.GetNextAutoslot()), delegate
			{
				m_LoadingProgress = 0.5f;
			}, processTag);
		}
		if (CurrentMode == GameModeType.Pause)
		{
			StopMode(CurrentMode);
		}
		LoadingProcess.Instance.StartLoadingProcess(m_SceneLoader.ActivateAdditiveArea(enterPoint, isSmokeTest: false, showLoadingScreen), OnAdditiveAreaSwitched, processTag);
		MatchTimeOfDay();
		if (autoSaveMode == AutoSaveMode.AfterEntry)
		{
			LoadingProcess.Instance.StartLoadingProcess(SaveManager.SaveRoutine(SaveManager.GetNextAutoslot()), delegate
			{
				m_LoadingProgress = 0.9f;
			}, processTag);
		}
		m_LoadingProgress = 1f;
		LoadingProcess.Instance.StartLoadingProcess(AwaitingNetwork(), null, processTag);
	}

	public void LoadArbiter(BlueprintArea area)
	{
		LoadArea(area, null, AutoSaveMode.None);
	}

	public void MatchTimeOfDay()
	{
		if (!m_MatchTimeOfDayScheduled)
		{
			m_MatchTimeOfDayScheduled = true;
			LoadingProcess.Instance.StartLoadingProcess(MatchTimeOfDayCoroutine(), null, LoadingProcessTag.ReloadLight);
		}
	}

	public void MatchTimeOfDayForced()
	{
		TimeOfDay = Player.GameTime.TimeOfDay();
	}

	public IEnumerator<object> MatchTimeOfDayCoroutine()
	{
		if (!Application.isPlaying)
		{
			yield break;
		}
		m_MatchTimeOfDayScheduled = false;
		TimeOfDay oldTime = TimeOfDay;
		TimeOfDay = Player.GameTime.TimeOfDay();
		Task p = SceneLoader.Instance.MatchLightAndAudioScenesCoroutine();
		while (!p.IsCompleted)
		{
			yield return null;
		}
		p.Wait();
		if (TimeOfDay != oldTime)
		{
			EventBus.RaiseEvent(delegate(ITimeOfDayChangedHandler h)
			{
				h.OnTimeOfDayChanged();
			});
		}
	}

	public static IEnumerator<object> UnloadMainMenuRoutine()
	{
		if (Application.isPlaying && SceneManager.GetSceneByName(GameScenes.MainMenu).isLoaded)
		{
			Instance.RootUiContext?.DisposeMainMenu();
			AsyncOperation unload = SceneManager.UnloadSceneAsync(GameScenes.MainMenu);
			while (!unload.isDone)
			{
				yield return null;
			}
		}
	}

	public static IEnumerator<object> UnloadUnusedAssetsCoroutine()
	{
		if (Application.isPlaying)
		{
			ResourcesLibrary.CleanupLoadedCache();
			CustomPortraitsManager.Instance.Cleanup();
			GC.Collect();
			AsyncOperation unload = Resources.UnloadUnusedAssets();
			while (!unload.isDone)
			{
				yield return null;
			}
		}
	}

	public static void ReloadAreaMechanic(bool clearFx, [CanBeNull] Action callback = null)
	{
		EventBus.RaiseEvent(delegate(IReloadMechanicsHandler h)
		{
			h.OnBeforeMechanicsReload();
		});
		LoadingProcess.Instance.StartLoadingProcess(SceneLoader.Instance.ReloadAreaMechanicsCoroutine(), delegate
		{
			if (clearFx)
			{
				PFLog.System.Log("FxHelper.DestroyAll();");
				FxHelper.DestroyAllBlood();
			}
			EventBus.RaiseEvent(delegate(IReloadMechanicsHandler h)
			{
				h.OnMechanicsReloaded();
			});
			Instance.Player.ApplyUpgrades();
			ExecuteSafe(callback);
		}, LoadingProcessTag.ReloadMechanics);
		LoadingProcess.Instance.StartLoadingProcess(AwaitingNetwork(), null, LoadingProcessTag.ReloadMechanics);
	}

	private static void LoadArea([NotNull] BlueprintArea area, [CanBeNull] BlueprintAreaEnterPoint enterPoint, AutoSaveMode autoSaveMode, [CanBeNull] SaveInfo saveInfo = null, [CanBeNull] Action callback = null)
	{
		if (area == null)
		{
			throw new ArgumentException("area is null", "area");
		}
		BlueprintAreaPart areaPart;
		if (saveInfo != null)
		{
			if (saveInfo.Area == null)
			{
				throw new ArgumentException(string.Format("{0} {1} has null {2}", "saveInfo", saveInfo, "Area"), "saveInfo");
			}
			if (enterPoint != null)
			{
				throw new ArgumentException(string.Format("{0} {1} should be null when using {2}", "enterPoint", enterPoint, "saveInfo"), "enterPoint");
			}
			areaPart = saveInfo.AreaPart ?? throw new ArgumentException(string.Format("{0} {1} has null {2}", "saveInfo", saveInfo, "AreaPart"), "saveInfo");
			PFLog.System.Log("Load Area {0} [save name: {1}]", area, saveInfo);
		}
		else
		{
			if (enterPoint == null)
			{
				throw new ArgumentException("enterPoint is null", "enterPoint");
			}
			if (enterPoint.Area != area)
			{
				throw new ArgumentException(string.Format("{0} {1} area does not match {2} {3}", "enterPoint", enterPoint, "area", area), "enterPoint");
			}
			areaPart = enterPoint.AreaPart ?? throw new Exception(string.Format("{0} {1} has null {2}. Most likely this {3} is outside of mechanics bounds of any {4}.", "BlueprintAreaEnterPoint", enterPoint, "AreaPart", "BlueprintAreaEnterPoint", "AreaPart"));
			PFLog.System.Log("Load Area {0} [enter point: {1}]", area, enterPoint);
		}
		if (autoSaveMode != 0 && !enterPoint)
		{
			throw new ArgumentException($"Autosave {autoSaveMode} can be done only when enter point is specified");
		}
		if (LoadingProcess.Instance.FindProcess((IEnumerator v) => v is LoadingAreaMarker) is LoadingAreaMarker loadingAreaMarker)
		{
			throw new InvalidOperationException($"Cant start loading area {area} when loading queue already contains {loadingAreaMarker.Area} to load");
		}
		Instance.ResetLoadingProgress();
		Instance.IsLoadingProgressPaused = true;
		LoadingProcess.Instance.StartLoadingProcess(SceneLoader.ConsolePreloadAreaCoroutine(area), delegate
		{
			Instance.IsLoadingProgressPaused = false;
		});
		Instance.RootUiContext.LoadingScreenRootVM?.SetLoadingArea(area);
		BlueprintArea currentlyLoadedArea = m_SceneLoader.CurrentlyLoadedArea;
		HashSet<string> hotSceneNames = area.GetHotSceneNames();
		if (currentlyLoadedArea != null)
		{
			if (saveInfo == null)
			{
				LoadingProcess.Instance.StartLoadingProcess(LoadingProcessCommandsLogic.WaitTickCommandsEnd(), delegate
				{
					Instance.m_LoadingProgress = 0.05f;
				});
			}
			if (autoSaveMode == AutoSaveMode.BeforeExit)
			{
				if (saveInfo == null && Instance.CurrentMode == GameModeType.Dialog && !Instance.DialogController.DialogStopScheduled && !Instance.GameCommandQueue.ContainsCommand((StopGameModeCommand x) => x.GameMode == GameModeType.Dialog))
				{
					PFLog.System.Log($"Stop mode {Instance.CurrentMode} before autosave");
					Instance.StopMode(Instance.CurrentMode);
				}
				LoadingProcess.Instance.StartLoadingProcess(Instance.SaveManager.SaveRoutine(Instance.SaveManager.GetNextAutoslot()), delegate
				{
					Instance.m_LoadingProgress = 0.1f;
				});
			}
			if (Instance.CurrentMode == GameModeType.Pause)
			{
				Instance.StopMode(Instance.CurrentMode);
			}
			LoadingProcess.Instance.StartLoadingProcess(m_SceneLoader.UnloadAreaCoroutine(saveInfo != null, currentlyLoadedArea == area, area.ActiveUIScene.SceneName != m_SceneLoader.LoadedUIScene, hotSceneNames), delegate
			{
				Instance.m_LoadingProgress = 0.2f;
			});
		}
		if (currentlyLoadedArea != null && saveInfo != null)
		{
			LoadingProcess.Instance.StartLoadingProcess(ResetGame(area));
		}
		LoadAreaStage2(area, enterPoint, autoSaveMode, saveInfo, callback, areaPart);
	}

	private static IEnumerator<object> ResetGame(BlueprintArea area)
	{
		Task resetTask = Reset();
		while (!resetTask.IsCompleted)
		{
			yield return null;
		}
		resetTask.Wait();
		Instance.RootUiContext.InitializeCommonScene("UI_Common_Scene");
		BugReportCanvas bugReportCanvas = UnityEngine.Object.FindObjectOfType<BugReportCanvas>();
		if ((bool)bugReportCanvas)
		{
			bugReportCanvas.BindKeyboard(Instance.Keyboard);
		}
		Instance.RootUiContext.LoadingScreenRootVM?.SetLoadingArea(area);
	}

	private static IEnumerator<object> DetachFromInstanceImpl(Func<IEnumerator<object>> enumeratorFn)
	{
		IEnumerator<object> enumerator = enumeratorFn();
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	private static void StartLoadingProcessDetached(Func<IEnumerator<object>> enumeratorFn, Action callback = null)
	{
		LoadingProcess.Instance.StartLoadingProcess(DetachFromInstanceImpl(enumeratorFn), callback);
	}

	private static void LoadAreaStage2([NotNull] BlueprintArea area, [CanBeNull] BlueprintAreaEnterPoint enterPoint, AutoSaveMode autoSaveMode, [CanBeNull] SaveInfo saveInfo, [CanBeNull] Action callback, [NotNull] BlueprintAreaPart areaPart)
	{
		if (SceneManager.GetSceneByName(GameScenes.MainMenu).isLoaded)
		{
			LoadingProcess.Instance.StartLoadingProcess(UnloadMainMenuRoutine(), delegate
			{
				Instance.m_LoadingProgress = 0.3f;
			});
		}
		LoadingProcess.Instance.StartLoadingProcess(SceneLoader.Instance.UnloadEntitiesCoroutine(saveInfo != null), delegate
		{
			Instance.m_LoadingProgress = 0.5f;
		});
		LoadingProcess.Instance.StartLoadingProcess(UnloadUnusedAssetsCoroutine(), delegate
		{
			Instance.m_LoadingProgress = 0.6f;
		});
		if (autoSaveMode == AutoSaveMode.WhenAreaIsUnloaded)
		{
			Instance.Player.NextEnterPoint = enterPoint;
			StartLoadingProcessDetached(delegate
			{
				Instance.Player.NextEnterPoint = enterPoint;
				return Instance.SaveManager.SaveRoutine(Instance.SaveManager.GetNextAutoslot());
			}, delegate
			{
				Instance.m_LoadingProgress = 0.1f;
			});
		}
		if (saveInfo != null)
		{
			StartLoadingProcessDetached(() => Instance.SaveManager.LoadRoutine(saveInfo), delegate
			{
				Instance.m_LoadingProgress = 0.4f;
			});
		}
		LoadingProcess.Instance.StartLoadingProcess(SceneLoader.Instance.UnloadEntitiesCoroutine(saveInfo != null), delegate
		{
			Instance.m_LoadingProgress = 0.5f;
		});
		LoadingProcess.Instance.StartLoadingProcess(UnloadUnusedAssetsCoroutine(), delegate
		{
			Instance.m_LoadingProgress = 0.6f;
		});
		LoadingProcess.Instance.StartLoadingProcess(SceneLoader.Instance.LoadAreaCoroutine(area, areaPart, enterPoint, isSmokeTest: false, new Progress<float>(Instance.OnLoadingScenesProgress)), delegate
		{
			Instance.OnLoadingScenesProgress(1f);
			Instance.OnAreaLoaded();
		});
		StartLoadingProcessDetached(() => Instance.LoadAdditiveAreas(area));
		LoadingProcess.Instance.StartLoadingProcess(PreloadUnitResources(), delegate
		{
			Instance.m_LoadingProgress = 0.7f;
		});
		LoadingProcess.Instance.StartLoadingProcess(delegate
		{
			Instance.Player.ApplyUpgrades();
		});
		StartLoadingProcessDetached(() => Instance.MatchTimeOfDayCoroutine(), delegate
		{
			Instance.m_LoadingProgress = 0.8f;
		});
		LoadingProcess.Instance.StartLoadingProcess(AwaitTextureCompression(), delegate
		{
			Instance.m_LoadingProgress = 0.95f;
		});
		StartLoadingProcessDetached(() => Instance.AreaLoadingComplete(), delegate
		{
			if (saveInfo != null)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(WarningNotificationType.GameLoaded);
				});
			}
			Instance.m_LoadingProgress = 1f;
			Instance.m_LoadingScenesProgress = 1f;
		});
		if (autoSaveMode == AutoSaveMode.AfterEntry)
		{
			StartLoadingProcessDetached(() => Instance.SaveManager.SaveRoutine(Instance.SaveManager.GetNextAutoslot()));
		}
		LoadingProcess.Instance.StartLoadingProcess(AwaitingUserInput());
		LoadingProcess.Instance.StartLoadingProcess(AwaitingNetwork());
		LoadingProcess.Instance.StartLoadingProcess(new LoadingAreaMarker(area));
		if (callback != null)
		{
			LoadingProcess.Instance.StartLoadingProcess(delegate
			{
				ExecuteSafe(callback);
			});
		}
		EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
		{
			h.HandleAreaTransition();
		});
	}

	private void TryFixPartyPositions()
	{
		List<BaseUnitEntity> list = Instance.Player.PartyAndPets.Where(AreaEnterPoint.ShouldMoveCharacterOnAreaEnterPoint).ToList();
		BaseUnitEntity baseUnitEntity = list.Find((BaseUnitEntity c) => c == Instance.Player.MainCharacter.Entity && CurrentlyLoadedAreaPart.Bounds.MechanicBounds.Contains(c.Position));
		if (baseUnitEntity == null)
		{
			baseUnitEntity = list.Find((BaseUnitEntity c) => CurrentlyLoadedAreaPart.Bounds.MechanicBounds.Contains(c.Position));
		}
		if (baseUnitEntity == null)
		{
			return;
		}
		foreach (BaseUnitEntity item in list)
		{
			if (!CurrentlyLoadedAreaPart.Bounds.MechanicBounds.Contains(item.Position))
			{
				item.Position = baseUnitEntity.Position;
				item.SnapToGrid();
				PFLog.Pathfinding.Error("Fixing " + item.CharacterName + " position. Was in wrong area!");
			}
		}
	}

	private IEnumerator<object> LoadAdditiveAreas(BlueprintArea area)
	{
		IEnumerable<BlueprintArea> enumerable = GetAdditiveAreas(area).EmptyIfNull();
		foreach (BlueprintArea item in enumerable)
		{
			IEnumerator load = m_SceneLoader.LoadAdditiveArea(item);
			while (load.MoveNext())
			{
				yield return null;
			}
		}
	}

	public IEnumerable<BlueprintArea> GetAdditiveAreas(BlueprintArea area)
	{
		return Enumerable.Empty<BlueprintArea>();
	}

	private void OnLoadingScenesProgress(float value)
	{
		m_LoadingScenesProgress = value;
	}

	public void ResetLoadingProgress()
	{
		m_LoadingProgress = 0f;
		m_LoadingScenesProgress = 0f;
	}

	private static void ExecuteSafe([CanBeNull] Action action)
	{
		try
		{
			action?.Invoke();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	public void ReloadArea()
	{
		if ((bool)CurrentlyLoadedArea)
		{
			LoadArea(CurrentlyLoadedArea, null, AutoSaveMode.None);
		}
	}

	private void OnAreaLoaded()
	{
		TryFixPartyPositions();
		HandleActiveAreaChanged(wasSwitched: false);
	}

	private void OnAdditiveAreaSwitched()
	{
		HandleActiveAreaChanged(wasSwitched: true);
		m_LoadingProgress = 0.8f;
	}

	private void HandleActiveAreaChanged(bool wasSwitched)
	{
		PFLog.System.Log("HandleActiveAreaChanged: started");
		SimpleCaster.WarmupPool();
		EventBus.RaiseEvent(delegate(IAreaLoadingStagesHandler h)
		{
			h.OnAreaScenesLoaded();
		});
		m_AreaWasSwitched = wasSwitched;
		PFLog.System.Log("HandleActiveAreaChanged: OnAreaScenesLoaded() finished");
		State.SetNewAwakeUnits(State.AllUnits.NotDead());
		GameModeType gameModeType = (Player.GameOverReason.HasValue ? GameModeType.GameOver : CurrentlyLoadedArea.AreaStatGameMode);
		PFLog.System.Log($"HandleActiveAreaChanged: start mode {gameModeType} for {CurrentlyLoadedArea} {CurrentlyLoadedArea.AreaStatGameMode}");
		if (m_GameModeSwitchOnLoadHandler == null)
		{
			m_GameModeSwitchOnLoadHandler = new GameModeSwitchOnLoadHandler(this);
		}
		EventBus.Subscribe(m_GameModeSwitchOnLoadHandler);
		DoStartMode(gameModeType);
		PFLog.System.Log("HandleActiveAreaChanged: GameMode activated");
	}

	private void OnAreaLoadGameModeSet()
	{
		PFLog.System.Log("OnAreaLoaded: GameMode activated");
		EventBus.Unsubscribe(m_GameModeSwitchOnLoadHandler);
		if (m_AreaWasSwitched)
		{
			EventBus.RaiseEvent(delegate(IAdditiveAreaSwitchHandler h)
			{
				h.OnAdditiveAreaDidActivated();
			});
			PFLog.System.Log("HandleActiveAreaChanged: OnAdditiveAreaDidActivated() finished");
		}
		else
		{
			EventBus.RaiseEvent(delegate(IAreaHandler h)
			{
				h.OnAreaDidLoad();
			});
			PFLog.System.Log("HandleActiveAreaChanged: OnAreaDidLoad() finished");
		}
		m_AreaWasSwitched = false;
		EntitySpawner.Tick();
		PFLog.System.Log("HandleActiveAreaChanged: RandomEncounterInitializer.HandleAreaLoaded() finished");
		LoadedAreaState.Activate();
		PFLog.System.Log("HandleActiveAreaChanged: CurrentScene.Activate() finished");
		Player.EtudesSystem.UpdateEtudes();
		_ = MonoSingleton<ParticleSystemCustomSubEmitterDelegate>.Instance;
		_ = BlueprintWarhammerRoot.Instance.BlueprintDismembermentRoot?.WeaponsListDismArray;
		EventBus.RaiseEvent(delegate(IAreaActivationHandler h)
		{
			h.OnAreaActivated();
		});
		PFLog.System.Log("HandleActiveAreaChanged: OnAreaActivated() finished");
		EntitySpawner.Tick();
		EntityDestroyer.Tick();
		Player.OnAreaLoaded();
		PFLog.System.Log("HandleActiveAreaChanged: Player.OnAreaLoaded() finished");
		foreach (BaseUnitEntity partyAndPet in Player.PartyAndPets)
		{
			partyAndPet.LifeState.HideIfDead();
		}
		PFLog.System.Log("HandleActiveAreaChanged: finished");
	}

	private static IEnumerator AwaitTextureCompression()
	{
		using (CodeTimerTraceScope.New("Texture compression"))
		{
			CharacterAtlasService atlasService = Services.GetInstance<CharacterAtlasService>();
			DxtCompressorService dxtService = Services.GetInstance<DxtCompressorService>();
			double startTime = Time.realtimeSinceStartupAsDouble;
			bool allDone = false;
			while (!allDone)
			{
				allDone = atlasService.RequestsCount == 0 && dxtService.RequestsCount == 0;
				if (allDone)
				{
					if (Time.realtimeSinceStartupAsDouble - startTime > 60.0)
					{
						break;
					}
					foreach (BaseUnitEntity item in Instance.Player.Party)
					{
						if (item.IsInGame)
						{
							Character character = item.View?.CharacterAvatar;
							if (character != null && character.BakedCharacter == null && !character.OverlaysMerged)
							{
								allDone = false;
								break;
							}
						}
					}
				}
				atlasService.Update();
				yield return null;
			}
			foreach (BaseUnitEntity item2 in Instance.Player.Party)
			{
				Character character2 = item2.View?.CharacterAvatar;
				if (character2 != null && character2.BakedCharacter == null && !character2.OverlaysMerged)
				{
					PFLog.Default.Error($"Timeout while waiting for texture to be compressed ({item2})");
				}
			}
		}
	}

	private IEnumerator<object> AreaLoadingComplete()
	{
		yield return null;
		LightProbes.TetrahedralizeAsync();
		Player.VisitedAreas.Add(CurrentlyLoadedArea);
		yield return null;
		UpdateNavMesh();
		UnitsPlacer.MovePartyToNavmesh();
		EventBus.RaiseEvent(delegate(IAreaLoadingStagesHandler h)
		{
			h.OnAreaLoadingComplete();
		});
		MaybeSuggestDLCImport();
		yield return null;
		ParticleSystemsQualityController.Instance.Init();
		void UpdateNavMesh()
		{
			AstarPath active = AstarPath.active;
			if (!(active == null) && CurrentlyLoadedArea.IsNavmeshArea)
			{
				GraphUpdateRouter.ForceUpdateAll();
				active.FlushGraphUpdates();
			}
		}
	}

	public void MaybeSuggestDLCImport()
	{
		if (Player.StartPreset?.Campaign.DlcReward != null)
		{
			return;
		}
		foreach (IBlueprintDlcReward dlc in (BlueprintRoot.Instance.DlcSettings.Dlcs?.SelectMany((IBlueprintDlc dlc) => dlc.Rewards)).EmptyIfNull())
		{
			if (Player.UsedDlcRewards.Any((BlueprintDlcReward dlcReward) => dlcReward == dlc))
			{
				continue;
			}
			BlueprintDlcRewardCampaign dlcCampaign = dlc as BlueprintDlcRewardCampaign;
			if (dlcCampaign == null || !dlcCampaign.IsAvailable)
			{
				continue;
			}
			SaveImportSettings importSettings = Player.Campaign.GetImportSettings(dlcCampaign.Campaign, newGame: false);
			if (importSettings == null || !importSettings.Condition.Check())
			{
				continue;
			}
			List<SaveInfo> saves = DlcSaveImporter.GetSavesForImport(dlcCampaign.Campaign);
			if (saves != null && saves.Count > 0)
			{
				PFLog.Default.Log($"Can import save from {dlcCampaign.Campaign} campaign from {dlcCampaign} DLC reward.");
				EventBus.RaiseEvent(delegate(ICampaignImportHandler h)
				{
					h.HandleSaveImport(dlcCampaign.Campaign, saves);
				});
			}
		}
	}

	private static IEnumerator PreloadUnitResources()
	{
		using (CodeTimer.New("Preloading Unit Resources"))
		{
			try
			{
				ResourcesLibrary.StartPreloadingMode();
				using (CodeTimer.New("Preloading Unit Resources: Schedule"))
				{
					ResourcesPreload.PreloadUnitResources();
					BlueprintRoot.Instance.HitSystemRoot.PreloadCommonHits();
				}
				while (ResourcesLibrary.TickPreloading())
				{
					yield return null;
				}
			}
			finally
			{
				ResourcesLibrary.StopPreloadingMode();
			}
		}
		using (CodeTimer.New("Prewarm pooled FX"))
		{
			BlueprintRoot.Instance.HitSystemRoot.WarmupCommonHits();
		}
	}

	public void LoadNewGame()
	{
		if (NewGamePreset == null)
		{
			throw new Exception("Cannot start new game. No preset specified");
		}
		LoadNewGame(NewGamePreset, ImportSave);
	}

	public void LoadNewGameAndApplyDLCFromAreaPreset(BlueprintAreaPreset preset)
	{
		LoadNewGame(preset);
	}

	public void LoadNewGame(BlueprintAreaPreset preset, SaveInfo importFrom = null)
	{
		BaseUnitEntity baseUnitEntity;
		if (NewGameUnit != null)
		{
			baseUnitEntity = NewGameUnit;
			NewGameUnit = null;
			State.PlayerState.CrossSceneState.AddEntityData(baseUnitEntity);
		}
		else
		{
			BlueprintUnit unit = preset.PlayerCharacter ?? BlueprintRoot.Instance.DefaultPlayerCharacter;
			baseUnitEntity = AddUnitToPersistentSate(unit);
		}
		BaseUnitEntity baseUnitEntity2;
		if (NewGameShip != null)
		{
			baseUnitEntity2 = NewGameShip;
			NewGameShip = null;
			State.PlayerState.CrossSceneState.AddEntityData(baseUnitEntity2);
		}
		else
		{
			BlueprintUnit unit2 = preset.PlayerShip ?? BlueprintRoot.Instance.DefaultPlayerShip;
			baseUnitEntity2 = AddUnitToPersistentSate(unit2);
		}
		State.PlayerState.SetMainStarship(baseUnitEntity2);
		State.PlayerState.InitMainStarship(preset);
		State.PlayerState.SetMainCharacter(baseUnitEntity);
		State.PlayerState.UpdateClaimedDlcRewardsByChosenAppearance(baseUnitEntity);
		State.PlayerState.GameId = Guid.NewGuid().ToString("N");
		Player.StartDate = DateTime.UtcNow;
		foreach (BlueprintUnitReference companion in preset.Companions)
		{
			if ((bool)companion.Get())
			{
				BaseUnitEntity baseUnitEntity3 = AddUnitToPersistentSate(companion.Get());
				if ((bool)baseUnitEntity3.Faction != (bool)BlueprintRoot.Instance.PlayerFaction)
				{
					baseUnitEntity3.Faction.Set(BlueprintRoot.Instance.PlayerFaction);
				}
				if (!baseUnitEntity3.IsPet)
				{
					Player.AddCompanion(baseUnitEntity3);
				}
			}
		}
		foreach (BlueprintUnitReference exCompanion in preset.ExCompanions)
		{
			if ((bool)exCompanion.Get() && !preset.Companions.Contains(exCompanion))
			{
				BaseUnitEntity baseUnitEntity4 = AddUnitToPersistentSate(exCompanion.Get());
				baseUnitEntity4.IsInGame = false;
				baseUnitEntity4.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.ExCompanion);
			}
		}
		foreach (BlueprintUnitReference item in preset.CompanionsRemote)
		{
			if ((bool)item.Get() && !preset.ExCompanions.Contains(item) && !preset.Companions.Contains(item))
			{
				BaseUnitEntity baseUnitEntity5 = AddUnitToPersistentSate(item.Get());
				baseUnitEntity5.IsInGame = false;
				baseUnitEntity5.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.Remote);
			}
		}
		EntitySpawner.Tick();
		try
		{
			preset.SetupState();
		}
		catch (Exception ex)
		{
			if (preset.IsNewGamePreset)
			{
				throw;
			}
			Logger.Exception(ex);
		}
		Player.MinDifficultyController.UpdateMinDifficulty(force: true);
		Player.InvalidateCharacterLists();
		Player.InitializeOnGameLoad();
		AreaDataStash.ClearDirectory();
		AreaDataStash.PrepareFirstLaunch();
		LoadArea(preset.EnterPoint, preset.MakeAutosave ? AutoSaveMode.AfterEntry : AutoSaveMode.None);
		LoadingProcess.Instance.StartLoadingProcess(Enumerable.Empty<object>().GetEnumerator(), delegate
		{
			PFLog.Default.Log("Running start-game actions");
			BlueprintRoot.Instance.StartGameActions.Run();
			preset.StartGameActions.Run();
			if (importFrom != null && importFrom.Campaign.DlcReward == null && preset.Campaign != null)
			{
				PFLog.Default.Log($"Importing from main campaign save {importFrom}");
				BlueprintCampaign campaign = preset.Campaign;
				if (campaign.IsImportRequired && campaign.ImportFromMainCampaign.Condition.Check())
				{
					campaign.ImportFromMainCampaign.DoImport(importFrom);
				}
			}
			BlueprintCampaign campaign2 = preset.Campaign;
			if (campaign2 != null && campaign2.DlcReward != null)
			{
				Player.UsedDlcRewards.Add(preset.Campaign.DlcReward);
			}
			if (preset.Campaign != null)
			{
				foreach (BlueprintDlc item2 in preset.Campaign.AdditionalContentDlc)
				{
					if (item2.IsActive)
					{
						foreach (IBlueprintDlcReward reward in item2.Rewards)
						{
							BlueprintDlcRewardCampaignAdditionalContent bpAc = reward as BlueprintDlcRewardCampaignAdditionalContent;
							if (bpAc != null && bpAc.Campaign == preset.Campaign && !Player.UsedDlcRewards.Any((BlueprintDlcReward o) => o.AssetGuid == bpAc.AssetGuid))
							{
								Player.UsedDlcRewards.Add(bpAc);
							}
						}
					}
				}
			}
		});
		LoadingProcess.Instance.StartLoadingProcess(Enumerable.Empty<object>().GetEnumerator(), delegate
		{
			SettingsController.Instance.StartInSaveSettings();
		});
	}

	public void ResetToMainMenu(Exception exception = null)
	{
		LoadingProcess.Instance.StopAll();
		LoadingProcess.Instance.StartLoadingProcess(m_SceneLoader.LoadMainMenuCoroutine(exception), delegate
		{
			SettingsController.Instance.StopInSaveSettings();
			Time.timeScale = 1f;
			m_LoadingProgress = 1f;
		});
	}

	[NotNull]
	private BaseUnitEntity AddUnitToPersistentSate(BlueprintUnit unit)
	{
		BaseUnitEntity baseUnitEntity = unit.CreateEntity();
		State.PlayerState.CrossSceneState.AddEntityData(baseUnitEntity);
		return baseUnitEntity;
	}

	public void QuickLoadGame()
	{
		SaveManager.UpdateSaveListIfNeeded();
		SaveInfo newestQuickslot = SaveManager.GetNewestQuickslot();
		if (newestQuickslot == null)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.NoQuickSaves);
			});
			PFLog.Default.Warning($"Unable to find any Quick Saves for current GameId={Instance.Player.GameId}. IronMan={SettingsRoot.Difficulty.OnlyOneSave.GetValue()}. List of found saves below.");
			StringBuilder stringBuilder = new StringBuilder();
			foreach (SaveInfo item in SaveManager)
			{
				stringBuilder.Append($"\"{item.FileName}\";{item.Type};{item.GameId}\n");
			}
			PFLog.Default.Log(stringBuilder.ToString());
		}
		else if (!SettingsRoot.Difficulty.OnlyOneSave.GetValue() || !(newestQuickslot.GameId == Player.GameId))
		{
			LoadGame(newestQuickslot);
		}
	}

	public void LoadGame([NotNull] SaveInfo saveInfo, [CanBeNull] Action callback = null)
	{
		try
		{
			Services.GetInstance<LogThreadService>()?.Cleanup();
		}
		catch (Exception ex)
		{
			PFLog.Default.Log(ex);
		}
		if (PhotonManager.Lobby.IsActive)
		{
			PhotonManager.NetGame.StartGame((SaveInfoKey)saveInfo, callback);
		}
		else
		{
			LoadGameLocal(saveInfo, callback);
		}
	}

	public void LoadGameLocal([NotNull] SaveInfo saveInfo, [CanBeNull] Action callback = null)
	{
		if (IsInMainMenu)
		{
			RootUIContext.Instance.CommonVM.Load(saveInfo, callback);
		}
		else
		{
			LoadGameForce(saveInfo, callback);
		}
	}

	private void LoadGameForce([NotNull] SaveInfo saveInfo, [CanBeNull] Action callback = null)
	{
		SoundState.Instance.ResetBeforeUnloading();
		LoadArea(saveInfo.Area, null, AutoSaveMode.None, saveInfo, callback);
	}

	public void LoadGameForSmokeTest(SaveInfo save)
	{
		RootUiContext.LoadingScreenRootVM.SetLoadingArea(save.Area);
		if (CurrentlyLoadedArea != null)
		{
			LoadingProcess.Instance.StartLoadingProcess(m_SceneLoader.UnloadAreaCoroutine(forDispose: true));
			LoadingProcess.Instance.StartLoadingProcess(m_SceneLoader.UnloadEntitiesCoroutine(unloadCrossScene: true));
		}
		LoadingProcess.Instance.StartLoadingProcess(SaveManager.LoadRoutine(save, isSmokeTest: true));
		LoadingProcess.Instance.StartLoadingProcess(m_SceneLoader.LoadAreaCoroutine(save.Area, save.AreaPart, null, isSmokeTest: true), OnAreaLoaded);
	}

	public void LoadGameFromMainMenu(SaveInfo saveInfo, [CanBeNull] Action callback = null)
	{
		if ((bool)CurrentlyLoadedArea)
		{
			DisposeState();
			State.PlayerState = Entity.Initialize(new Player());
			State.SavedAreaStates.Clear();
		}
		LoadGameForce(saveInfo, callback);
	}

	public void DisposeState()
	{
		foreach (AreaPersistentState savedAreaState in State.SavedAreaStates)
		{
			savedAreaState.Dispose();
		}
		m_SceneLoader.ClearLoadedArea();
		State.PlayerState?.Dispose();
		EntitySpawner.Dispose();
		UnitGroupsController.Clear();
		DialogController.Dispose();
		State.ClearAwakeUnits();
		Services.EndLifetime(ServiceLifetimeType.GameSession);
		GamesModeFactoryFacade.ResetControllers();
		State.PlayerState = null;
		State.LoadedAreaState = null;
	}

	public void MakeQuickSave(Action callback = null)
	{
		SaveManager.UpdateSaveListIfNeeded();
		RequestSaveGame(SaveManager.GetNextQuickslot(), null, callback);
	}

	public void RequestSaveGame([CanBeNull] SaveInfo saveInfo, [CanBeNull] string saveName = null, Action callback = null)
	{
		Instance.GameCommandQueue.SaveGame(saveInfo, saveName, callback);
	}

	public void SaveGame(SaveInfo saveInfo, Action callback = null)
	{
		if (saveInfo.Type != SaveInfo.SaveType.ForImport)
		{
			if (!SaveManager.IsSaveAllowed(saveInfo.Type))
			{
				if (saveInfo.Type != SaveInfo.SaveType.Auto)
				{
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(WarningNotificationType.SavingImpossible);
					});
				}
				return;
			}
			if (saveInfo.Type != SaveInfo.SaveType.Bugreport && (bool)SettingsRoot.Difficulty.OnlyOneSave)
			{
				if (!SaveManager.IsIronmanSave(saveInfo))
				{
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(WarningNotificationType.SavingImpossibleIronmanWillSavedAutomaticaly, addToLog: true, WarningNotificationFormat.Short);
					});
					return;
				}
				if (saveInfo.Type != SaveInfo.SaveType.Auto && saveInfo.Type != SaveInfo.SaveType.IronMan)
				{
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(WarningNotificationType.SavingImpossibleIronmanWillSavedAutomaticaly, addToLog: true, WarningNotificationFormat.Short);
					});
					return;
				}
				saveInfo.Type = SaveInfo.SaveType.IronMan;
			}
		}
		LoadingProcess.Instance.StartLoadingProcess(SaveManager.SaveRoutine(saveInfo, forceAuto: false, showNotification: true), callback, LoadingProcessTag.Save);
	}

	private void CrossSceneStateHandler(SceneEntitiesState state, Entity data)
	{
		if (state == Player.CrossSceneState)
		{
			Player.InvalidateCharacterLists();
		}
	}

	public static async Task Reset()
	{
		ControllerModeType controllerMode = s_Instance.ControllerMode;
		await GamesModeFactoryFacade.Instance.Reset();
		SceneEntitiesState.ClearSubscriptions();
		LoadingScreenRootVM loadingScreenRootVM = s_Instance?.RootUiContext.LoadingScreenRootVM;
		if (s_Instance != null)
		{
			s_Instance.Statistic?.Reset();
			s_Instance.DisposeState();
			s_Instance.RootUiContext.Dispose();
			s_Instance.StopAllModes();
			s_Instance.CursorController?.Deactivate();
		}
		KeyboardAccess.Instance.UnbindAll();
		InterfaceServiceLocator.UnregisterService(typeof(ITimeController));
		InterfaceServiceLocator.UnregisterService(typeof(IPersistentState));
		s_Instance = new Game
		{
			ControllerMode = controllerMode,
			m_LoadingProgress = 0.2f
		};
		DontChangeController = true;
		s_Instance.Initialize();
		if (loadingScreenRootVM != null)
		{
			s_Instance.RootUiContext.InitializeLoadingScreenScene(loadingScreenRootVM);
		}
		ObjectExtensions.Or(CameraRig.Instance, null)?.RenewKeyboardBindings();
		IngameConsoleKeybinds.Refresh();
	}

	public static void ResetUI(Action onComplete = null)
	{
		s_Instance?.RootUiContext.ResetUI(onComplete);
	}

	public static IEnumerator ResetUICoroutine(Action onComplete = null)
	{
		IEnumerator resetUi = s_Instance.RootUiContext.InitializeUiSceneCoroutine(s_Instance.SceneLoader.LoadedUIScene, onComplete);
		while (resetUi.MoveNext())
		{
			yield return null;
		}
	}

	public static void ChangeUIPlatform(bool nextPlatform)
	{
		GamePad gamePad = GamePad.Instance;
		int length = Enum.GetValues(typeof(ConsoleType)).Length;
		gamePad.ConsoleTypeProperty.Value = (ConsoleType)((int)(gamePad.ConsoleTypeProperty.Value + length + (nextPlatform ? 1 : (-1))) % length);
		Instance.ControllerMode = ((gamePad.ConsoleTypeProperty.Value != 0) ? ControllerModeType.Gamepad : ControllerModeType.Mouse);
		ResetUI(delegate
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning($"Input controller: {gamePad.ConsoleTypeProperty.Value}");
			});
		});
	}

	public static Camera GetCamera()
	{
		if (!Application.isPlaying)
		{
			return Camera.main;
		}
		CameraRig instance = CameraRig.Instance;
		if ((bool)instance && instance.Camera.enabled && instance.gameObject.activeInHierarchy)
		{
			return instance.Camera;
		}
		return CameraStackManager.Instance.GetMain();
	}

	private static IEnumerator<object> AwaitingUserInput()
	{
		EventBus.RaiseEvent(delegate(IStartAwaitingUserInput h)
		{
			h.OnStartAwaitingUserInput();
		});
		while ((bool)LoadingProcess.Instance.IsAwaitingUserInput)
		{
			yield return null;
		}
	}

	public static IEnumerator<object> AwaitingNetwork()
	{
		while (!NetworkingManager.LockOn(NetLockPointId.LoadingProcess))
		{
			yield return null;
		}
	}

	public static IEnumerator<object> AwaitingCommands()
	{
		while (Wait())
		{
			yield return null;
		}
		static bool Wait()
		{
			if (!NetService.Instance.Initialized)
			{
				return false;
			}
			if (!Instance.RealTimeController.TickCompleted)
			{
				return false;
			}
			int currentNetworkTick = Instance.RealTimeController.CurrentNetworkTick;
			if (Instance.RealTimeController.NextTickCommandsReady(currentNetworkTick))
			{
				return false;
			}
			return true;
		}
	}
}
