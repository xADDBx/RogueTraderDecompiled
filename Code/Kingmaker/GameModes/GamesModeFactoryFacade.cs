using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.Assets.Controllers.GlobalMap;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.Controllers.MovePrediction;
using Kingmaker.Controllers.Net;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.Controllers.Rest;
using Kingmaker.Controllers.SpaceCombat;
using Kingmaker.Controllers.StarSystem;
using Kingmaker.Controllers.Timer;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.Controllers.UnityEventsReplacements;
using Kingmaker.Tutorial;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.Log;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.GameModes;

public class GamesModeFactoryFacade
{
	private static GamesModeFactoryFacade s_Instance;

	private static readonly GameModeType None = GameModeType.None;

	private static readonly GameModeType Default = GameModeType.Default;

	private static readonly GameModeType GlobalMap = GameModeType.GlobalMap;

	private static readonly GameModeType Dialog = GameModeType.Dialog;

	private static readonly GameModeType Pause = GameModeType.Pause;

	private static readonly GameModeType Cutscene = GameModeType.Cutscene;

	private static readonly GameModeType GameOver = GameModeType.GameOver;

	private static readonly GameModeType BugReport = GameModeType.BugReport;

	private static readonly GameModeType CutsceneGlobalMap = GameModeType.CutsceneGlobalMap;

	private static readonly GameModeType SpaceCombat = GameModeType.SpaceCombat;

	private static readonly GameModeType StarSystem = GameModeType.StarSystem;

	public static GamesModeFactoryFacade Instance => s_Instance ?? (s_Instance = new GamesModeFactoryFacade());

	private static ReadonlyList<GameModeType> All => GameModeType.All;

	private GamesModeFactoryFacade()
	{
		Initialize();
	}

	public GameMode Create(GameModeType type)
	{
		if (GameModesFactory.AllControllers.Empty())
		{
			Initialize();
		}
		return GameModesFactory.Create(type);
	}

	public Task Reset()
	{
		return GameModesFactory.Reset();
	}

	private void Initialize()
	{
		Register(new StabilizeInterpolationController(), All);
		Register(new SuppressEntitiesController(), Default, Dialog, Pause, Cutscene);
		Register(Game.Instance.UnitGroupsController, Default, SpaceCombat, StarSystem, Dialog, Cutscene);
		Register(new SlowMoController(), GameModeType.Default);
		Register(Game.Instance.TimeController, Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap, CutsceneGlobalMap);
		Register(new KeyboardAccessTicker(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap, GameOver, BugReport);
		Register(new UnitPlaceOnGroundController(), Default, Pause);
		Register(Game.Instance.PauseController, All);
		Register(new UnpauseController(), Default, SpaceCombat, StarSystem, Pause);
		Register(Game.Instance.DefaultPointerController, Default, SpaceCombat, Pause);
		Register(new InteractionHighlightController(), Default, SpaceCombat, StarSystem, Pause);
		Register(Game.Instance.DirectorAdapterController, Default, Dialog, Cutscene, CutsceneGlobalMap);
		Register(Game.Instance.DoorUpdateController, Default, Pause, Cutscene, StarSystem, Dialog);
		Register(new PathfindingController(), GameModeType.All);
		Register(new CutsceneController(tickBackground: false), Dialog, Cutscene, CutsceneGlobalMap);
		Register(new CutsceneController(tickBackground: true), Default);
		Register(new SummonedUnitsController(), Default);
		Register(new UnitLineOfSightCacheController(), Default, Cutscene);
		Register(Game.Instance.FollowersFormationController, Default);
		Register(new UnitFollowUnitController(), Default);
		Register(new UnitMoveController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
		Register(new UnitMoveControllerLate(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
		Register(Game.Instance.MovePredictionController, Default);
		Register(new ModePredictionInterpolationController(), Default);
		Register(Game.Instance.CustomUpdateBeforePhysicsController, All);
		Register(new PhysicsSimulationController(), All);
		Register(new CameraFollowController(), Default, SpaceCombat);
		Register(Game.Instance.EntityBoundsController, Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene);
		Register(Game.Instance.FogOfWarRevealerTriggerController, All);
		Register(new FogOfWarBlockerController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap);
		Register(Game.Instance.FogOfWar, Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap);
		Register(new PartyAwarenessController(), Default);
		Register(Game.Instance.EntityVisibilityForPlayerController, Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap);
		Register(new UnitMimicController(), Default);
		Register(new UnitCombatJoinController(), Default, SpaceCombat, StarSystem);
		Register(Game.Instance.SelectionCharacter, Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap, GameOver, BugReport);
		Register(new UnitGuardController(), Default);
		Register(new UnitFearController(), Default);
		Register(Game.Instance.UnitMemoryController, Default, SpaceCombat, StarSystem);
		Register(Game.Instance.AiBrainController, Default, SpaceCombat, StarSystem);
		Register(new UnitForcedTargetController(), Default);
		Register(Game.Instance.AttackOfOpportunityController, Default);
		Register(new UnitActivatableAbilitiesController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
		Register(Game.Instance.SynchronizedDataController, All);
		Register(Game.Instance.GamepadInputController, All);
		Register(Game.Instance.UnitCommandBuffer, All);
		Register(new NetSendController(), All);
		Register(new UnitCommandController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
		Register(Game.Instance.GroupCommands, Default, Dialog, StarSystem, Cutscene);
		Register(new BuffsController(), Default, SpaceCombat, StarSystem, Dialog, GlobalMap);
		Register(new UnitHandEquipmentController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene);
		Register(new UnitRoamingController(), Default, SpaceCombat, Dialog, Cutscene);
		Register(new UnitsProximityController(), Default, Cutscene);
		Register(new ScriptZoneController(), Default, Dialog, StarSystem, Cutscene);
		Register(new AreaEffectsController(), Default, SpaceCombat, Dialog);
		Register(Game.Instance.ProjectileController, Default, SpaceCombat, StarSystem, Dialog, Cutscene);
		Register(Game.Instance.ProjectileSpawnerController, Default, SpaceCombat, StarSystem, Dialog, Cutscene);
		Register(new ProjectileHitController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
		Register(Game.Instance.AbilityExecutor, Default, SpaceCombat, StarSystem, Dialog, Cutscene);
		Register(new PsychicPhenomenaController(), Default, Dialog, Cutscene);
		Register(Game.Instance.DialogController, Dialog);
		Register(new UnitLifeController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap, CutsceneGlobalMap);
		Register(new UnitReturnToConsciousController(), Default, Dialog, Cutscene, GlobalMap, CutsceneGlobalMap);
		Register(new HealthController(), All.Except(Pause, Cutscene, CutsceneGlobalMap, GameOver));
		Register(new UnitAnimationController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene);
		Register(new FootprintsController(), Default, Dialog, Cutscene);
		Register(new ItemsEnchantmentController(), Default);
		Register(new UnitForceMoveController(), Default);
		Register(new UnitJumpMoveController(), Default);
		Register(new WeatherController(), Default, Dialog, Pause, Cutscene);
		Register(new CameraController(), Default, SpaceCombat, Pause);
		Register(new CameraController(allowScroll: false), Dialog);
		Register(new CameraController(allowScroll: false, allowZoom: false, clamp: false, rotate: false), Cutscene);
		Register(new GameOverController(), Default, SpaceCombat, StarSystem, GlobalMap);
		Register(new InspectUnitsController(), Default, Dialog, Cutscene);
		Register(new EtudeSystemController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap, CutsceneGlobalMap);
		Register(new ShadowSpellController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene);
		Register(new OffensiveActionsController(), Default);
		Register(new GameOverIronmanController(), GameOver);
		Register(new TutorialController(), Default, SpaceCombat, Dialog, Pause, Cutscene, StarSystem, GlobalMap);
		Register(Game.Instance.CustomUpdateController, Default, SpaceCombat, Pause, Cutscene, StarSystem, Dialog);
		Register(new UpdatePreviousPositionController(), Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
		Register(Game.Instance.BirdUpdateController, Default, Pause, Cutscene, StarSystem, Dialog);
		Register(Game.Instance.TurnController, Default, SpaceCombat);
		Register(new AutoEndTurnController(), Default, SpaceCombat);
		Register(new CombatStateSwitchController(), Default, SpaceCombat);
		Register(new AutoJoinSpaceCombatController(), SpaceCombat);
		Register(new ExitSpaceCombatController(), SpaceCombat);
		Register(new StarshipPostController(), Default, SpaceCombat, Dialog, Pause, Cutscene, StarSystem, GlobalMap);
		Register(new StarshipTurretController(), SpaceCombat);
		Register(new StarshipEngineSoundController(), SpaceCombat, StarSystem);
		Register(new StarshipVantagePointsController(), SpaceCombat);
		Register(Game.Instance.StarshipPathController, SpaceCombat);
		Register(Game.Instance.MeteorStreamController, SpaceCombat);
		Register(new ShipPathNodeMarkersController(), SpaceCombat);
		Register(new SpaceCombatBackgroundComposerController(), StarSystem, SpaceCombat);
		Register(new StarSystemTimeController(), StarSystem);
		Register(new StarSystemAiController(), StarSystem);
		Register(new StarSystemFoWController(), StarSystem, GlobalMap);
		Register(new StarSystemCameraController(), StarSystem);
		Register(new AnomaliesController(), StarSystem);
		Register(Game.Instance.StarSystemMapController, StarSystem);
		Register(new PointerController(new StarSystemMapClickObjectHandler()), StarSystem);
		Register(new StarSystemMapMoveController(), StarSystem);
		Register(new SectorMapTimeController(), GlobalMap);
		Register(Game.Instance.SectorMapTravelController, GlobalMap);
		Register(Game.Instance.SectorMapController, GlobalMap, CutsceneGlobalMap);
		Register(Game.Instance.ColonizationController, Default, StarSystem, GlobalMap, CutsceneGlobalMap);
		Register(Game.Instance.CombatRandomEncounterController, Default, SpaceCombat);
		Register(new SectorMapCameraController(), GlobalMap, CutsceneGlobalMap);
		Register(Game.Instance.UnitMovableAreaController, Default);
		Register(new OverwatchController(), Default, SpaceCombat);
		Register(new AutoPauseController(), Default);
		Register(new TimerController(), Default);
		Register(new BarkBanterController(), Default, StarSystem);
		Register(new DopplerSoundController(), Default, SpaceCombat);
		Register(new UnitCombatLeaveController(), Default, Dialog, SpaceCombat, StarSystem, CutsceneGlobalMap);
		Register(new VisualEffectsController(), All.Except(None, BugReport));
		Register(new OccluderClipController(), Default, Dialog, Cutscene, Pause);
		Register(new GameLogController(), All);
		Register(Game.Instance.EntitySpawner, Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
		Register(Game.Instance.EntityDestroyer, Default, SpaceCombat, StarSystem, Dialog, Cutscene, GlobalMap);
		Register(new GarbageEntityController(), All);
		Register(new VirtualPositionController(Game.Instance.TurnController), All);
		Register(Game.Instance.Vendor, All);
		Register(new EntitiesInCameraFrustumController(), Default, Dialog, Pause, Cutscene);
		Register(new SleepingUnitsController(), Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap);
		Register(Game.Instance.CustomGridNodeController, All);
		Register(Game.Instance.AnimationManagerController, All);
		Register(Game.Instance.UpdateRigidbodyCreatureController, All);
		Register(new UnitMovementController(), All);
		Register(Game.Instance.CustomCallbackController, All);
		Register(Game.Instance.CoroutinesController, All);
		Register(new StateSerializationController(), All);
		Register(Game.Instance.SyncStateCheckerController, All);
		Register(Game.Instance.CustomLateUpdateController, All);
		Register(Game.Instance.InterpolationController, All);
		Register(Game.Instance.TimeSpeedController, All);
		Register(Game.Instance.InteractionFXController, All);
		Register(new WarpMoveEffectController(), GlobalMap);
		Register(Game.Instance.FogOfWarComplete, Default, SpaceCombat, StarSystem, Dialog, Pause, Cutscene, GlobalMap, CutsceneGlobalMap);
		Register(Game.Instance.ForcedCoversController, All);
		Register(new LevelUpFxController(), Default);
		Register(Game.Instance.CameraFXController, All);
		Register(Game.Instance.SceneControllables, All);
	}

	private static void Register(IController controller, IEnumerable<GameModeType> gameModes)
	{
		Register(controller, gameModes.ToArray());
	}

	private static void Register(IController controller, params GameModeType[] gameModes)
	{
		GameModesFactory.Register(controller, gameModes);
	}

	public static void ResetControllers()
	{
		foreach (ControllerData allController in GameModesFactory.AllControllers)
		{
			if (allController.Controller is IControllerReset controllerReset)
			{
				try
				{
					controllerReset.OnReset();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
	}
}
