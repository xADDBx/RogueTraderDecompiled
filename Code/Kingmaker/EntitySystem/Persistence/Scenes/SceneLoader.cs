using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.Legacy.BugReportDrawing;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Controllers;
using Kingmaker.Controllers.FogOfWar.LineOfSight;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.IngameConsole;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Spawners;
using Kingmaker.Visual;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.DayNightCycle;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Registry;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker.EntitySystem.Persistence.Scenes;

public class SceneLoader
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("System Loading");

	public const string CrossSceneName = "[cross-area objects]";

	private string m_LoadedUIScene = "";

	private readonly HashSet<SceneReference> m_LoadedAreaScenes = new HashSet<SceneReference>();

	private readonly List<BlueprintArea> m_LoadedAreas = new List<BlueprintArea>();

	public static SceneLoader Instance { get; } = new SceneLoader();


	public BlueprintArea CurrentlyLoadedArea { get; private set; }

	public BlueprintAreaPart CurrentlyLoadedAreaPart { get; private set; }

	public TimeOfDay CurrentlyLoadedTimeOfDay { get; private set; }

	public string LoadedUIScene
	{
		get
		{
			return m_LoadedUIScene;
		}
		set
		{
			m_LoadedUIScene = value;
		}
	}

	private bool TraceLoading { get; } = CommandLineArguments.Parse().Contains("trace-loading");


	public RootGroup DynamicRoot { get; } = new RootGroup("[dynamic objects]");


	public RootGroup CrossSceneRoot { get; } = new RootGroup("[cross-area objects]");


	private SceneLoader()
	{
	}

	public static Scene GetBaseMechanicsScene()
	{
		return SceneManager.GetSceneByName("BaseMechanics");
	}

	public static void LoadObligatoryScenes()
	{
		if (!GetBaseMechanicsScene().isLoaded)
		{
			SceneManagerWrapper.LoadScene("BaseMechanics", LoadSceneMode.Additive);
		}
	}

	public static AsyncOperation LoadObligatoryScenesAsync()
	{
		if (GetBaseMechanicsScene().isLoaded)
		{
			return null;
		}
		return SceneManagerWrapper.LoadSceneAsync("BaseMechanics", LoadSceneMode.Additive);
	}

	public IEnumerator SwitchToAreaPartCoroutine([NotNull] BlueprintAreaPart part)
	{
		if (part == null)
		{
			throw new ArgumentException("part is null", "part");
		}
		BlueprintAreaPart prev = CurrentlyLoadedAreaPart;
		BlueprintAreaPart currentlyLoadedAreaPart = CurrentlyLoadedAreaPart;
		if (currentlyLoadedAreaPart != null && currentlyLoadedAreaPart.ManageBanksSeparately)
		{
			SoundBanksManager.MarkBanksToUnload(CurrentlyLoadedAreaPart.GetActiveSoundBankNames(isCurrentPart: true), CurrentlyLoadedAreaPart.UnloadBanksDelay);
		}
		LightController.Active?.gameObject.SetActive(value: false);
		if ((BuildModeUtility.Data?.Loading?.IgnoreParts).GetValueOrDefault())
		{
			Task fow = SetupFogOfWar(stash: true, part);
			while (!fow.IsCompleted)
			{
				yield return null;
			}
			fow.Wait();
			IEnumerable<BlueprintAreaPart> partsToLoad = GetPartsToLoad(CurrentlyLoadedArea, part);
			IEnumerable<BlueprintAreaPart> partsToLoad2 = GetPartsToLoad(CurrentlyLoadedArea, CurrentlyLoadedAreaPart);
			List<SceneReference> list = partsToLoad.Select((BlueprintAreaPart p) => p.StaticScene).ToList();
			List<Task> list2 = new List<Task>();
			foreach (BlueprintAreaPart item in partsToLoad2)
			{
				SceneReference scene = item.StaticScene;
				if (!list.Any((SceneReference s) => s.SceneName == scene.SceneName))
				{
					list2.Add(UnloadSceneCoroutine(scene, keepHot: false));
					m_LoadedAreaScenes.Remove(scene);
				}
			}
			foreach (SceneReference item2 in list)
			{
				list2.Add(LoadSceneCoroutine(item2));
				m_LoadedAreaScenes.Add(item2);
			}
			Task combined = Task.WhenAll(list2);
			while (!combined.IsCompleted)
			{
				yield return null;
			}
			combined.Wait();
		}
		CurrentlyLoadedAreaPart = part;
		Task light = MatchLightAndAudioScenesCoroutine();
		while (!light.IsCompleted)
		{
			yield return null;
		}
		light.Wait();
		EventBus.RaiseEvent(delegate(IAreaPartHandler h)
		{
			h.OnAreaPartChanged(prev);
		});
		AsyncOperation unload = Resources.UnloadUnusedAssets();
		while (!unload.isDone)
		{
			yield return null;
		}
	}

	public IEnumerator ReloadUIScene(BlueprintArea area, bool isSmokeTest = false)
	{
		if (BuildModeUtility.Data?.Loading.IgnoreUI ?? false)
		{
			yield break;
		}
		IEnumerator resetUi;
		if (area.ActiveUIScene.SceneName == m_LoadedUIScene)
		{
			using (CodeTimer.New(Logger, "Reset UI Scene"))
			{
				resetUi = Game.ResetUICoroutine();
				while (resetUi.MoveNext())
				{
					yield return null;
				}
			}
			yield break;
		}
		WidgetFactory.DestroyAll();
		resetUi = UnloadInGameUiSceneCoroutine();
		while (resetUi.MoveNext())
		{
			yield return null;
		}
		m_LoadedUIScene = area.ActiveUIScene.SceneName;
		if (!isSmokeTest && !string.IsNullOrEmpty(m_LoadedUIScene) && !SceneManager.GetSceneByName(m_LoadedUIScene).isLoaded && !(BuildModeUtility.Data?.Loading.IgnoreUI ?? false) && Application.isPlaying)
		{
			using (CodeTimer.New(Logger, "Loading UI scene"))
			{
				Task op = BundledSceneLoader.LoadSceneAsync(m_LoadedUIScene);
				while (!op.IsCompleted)
				{
					yield return null;
				}
				op.Wait();
			}
		}
		using (CodeTimer.New(Logger, "Initialize UI Scene"))
		{
			using (ProfileScope.New("Initialize UI Scene"))
			{
				if (!SceneManager.GetSceneByName(m_LoadedUIScene).isLoaded)
				{
					yield break;
				}
				resetUi = Game.Instance.RootUiContext.InitializeUiSceneCoroutine(m_LoadedUIScene);
				while (resetUi.MoveNext())
				{
					yield return null;
				}
				if ((BuildModeUtility.Data?.Loading?.DestroyUIPrefabs).GetValueOrDefault())
				{
					GC.Collect();
					yield return null;
					AsyncOperation uua = Resources.UnloadUnusedAssets();
					while (!uua.isDone)
					{
						yield return null;
					}
				}
			}
		}
	}

	public static IEnumerator<object> ConsolePreloadAreaCoroutine(BlueprintArea area)
	{
		yield break;
	}

	public IEnumerator<object> LoadAreaCoroutine([NotNull] BlueprintArea area, [NotNull] BlueprintAreaPart areaPart, [CanBeNull] BlueprintAreaEnterPoint enterPoint, bool isSmokeTest = false, IProgress<float> progressValueCallback = null)
	{
		if (area == null)
		{
			throw new ArgumentException("area is null", "area");
		}
		if (areaPart == null)
		{
			throw new ArgumentException(string.Format("{0} is null, area {1}, enterPoint {2}", "areaPart", area, enterPoint?.ToString() ?? "null"), "areaPart");
		}
		bool isFromArea = CurrentlyLoadedArea != null;
		CurrentlyLoadedArea = area;
		BlueprintAreaPart currentlyLoadedAreaPart = CurrentlyLoadedAreaPart;
		if (currentlyLoadedAreaPart != null && currentlyLoadedAreaPart.ManageBanksSeparately)
		{
			SoundBanksManager.MarkBanksToUnload(CurrentlyLoadedAreaPart.GetActiveSoundBankNames(isCurrentPart: true), CurrentlyLoadedAreaPart.UnloadBanksDelay);
		}
		CurrentlyLoadedAreaPart = areaPart;
		m_LoadedAreas.Add(area);
		if (Game.Instance.Player.NextEnterPoint != null)
		{
			enterPoint = Game.Instance.Player.NextEnterPoint;
			Game.Instance.Player.NextEnterPoint = null;
		}
		if (!isFromArea && enterPoint == null)
		{
			UnitPartCompanion optional = Game.Instance.Player.MainCharacterEntity.GetOptional<UnitPartCompanion>();
			Vector3 vector = ((optional != null && optional.State == CompanionState.InPartyDetached) ? Game.Instance.Player.PartyCharacters.FirstItem().Entity.Position : Game.Instance.Player.MainCharacter.Entity.Position);
			BlueprintAreaPart blueprintAreaPart = AreaService.FindMechanicBoundsContainsPoint(vector);
			if (blueprintAreaPart != null && blueprintAreaPart != areaPart)
			{
				PFLog.System.Warning($"When loading, player position {vector} does not match area part {areaPart}. Fixing to {blueprintAreaPart}");
				areaPart = blueprintAreaPart;
			}
		}
		using (CodeTimer.New(Logger, "Load Sound Banks"))
		{
			using (ProfileScope.New("Load Sound Banks"))
			{
				SoundBanksManager.LoadBanks(area.GetActiveSoundBankNames());
			}
		}
		Task reload;
		if (Application.isPlaying)
		{
			using (CodeTimer.New(Logger, "Match area scenes"))
			{
				reload = MatchAreaStaticAndDynamicScenesCoroutine(area, areaPart, isSmokeTest, progressValueCallback);
				while (!reload.IsCompleted)
				{
					yield return null;
				}
				reload.Wait();
			}
			if (!isFromArea && Application.isEditor)
			{
				List<Scene> list = (from i in Enumerable.Range(0, SceneManager.sceneCount)
					select SceneManager.GetSceneAt(i)).ToList();
				foreach (Scene scene in list)
				{
					if (scene.isLoaded && !(scene.name == m_LoadedUIScene) && !(scene.name == "BaseMechanics") && !(scene.name == "EntityBounds") && !(scene.name == "UI_Common_Scene") && !(scene.name == "Arbiter") && !(scene.name == "IngameConsole") && !(scene.name == "LoadingScreen") && (CrossSceneRoot == null || !(scene.name == CrossSceneRoot.Name)) && !m_LoadedAreaScenes.Any((SceneReference sr) => sr.SceneName == scene.name) && !HotScenesManager.IsSceneDeactivated(scene.name))
					{
						PFLog.SceneLoader.Log("Unload scene: {0}", scene.name);
						reload = BundledSceneLoader.UnloadSceneAsync(scene.name);
						while (!reload.IsCompleted)
						{
							yield return null;
						}
						reload.Wait();
					}
				}
			}
			MatchStaticSceneActiveStatus(null);
			AstarPath active = AstarPath.active;
			if ((bool)active)
			{
				IEnumerator scan = active.ScanCoroutine();
				while (scan.MoveNext())
				{
					yield return null;
				}
			}
			else if (CurrentlyLoadedArea.IsNavmeshArea)
			{
				throw new InvalidOperationException($"AStarPath not found on area {CurrentlyLoadedArea}");
			}
			if (area.IsPartyArea)
			{
				((CustomGridGraph)AstarPath.active.graphs[0])?.InitLosCache();
			}
		}
		PFLog.SceneLoader.Log("Scene set matched");
		using (ProfileScope.New("Clean Up Dynamic Roots"))
		{
			using (CodeTimer.New(Logger, "Clean up dynamic roots"))
			{
				yield return DynamicRoot.Clear();
				if ((bool)FxHelper.FxRoot)
				{
					DynamicRoot.Add(FxHelper.FxRoot);
				}
			}
		}
		yield return null;
		AreaPersistentState state = Game.Instance.State.GetStateForArea(area);
		bool shouldLoad = state.ShouldLoad;
		if (shouldLoad)
		{
			using (CodeTimer.New(Logger, $"Unstash area state: {state.Blueprint}"))
			{
				Game.Instance.State.SavedAreaStates.Remove(state);
				state = AreaDataStash.UnstashAreaState(state);
				Game.Instance.State.SavedAreaStates.Add(state);
			}
		}
		Game.Instance.State.LoadedAreaState = state;
		if (shouldLoad)
		{
			using (ProfileScope.New("PostLoad area state"))
			{
				state.PostLoad();
			}
		}
		using (CodeTimer.New(Logger, "Preloading Unit Views in states"))
		{
			try
			{
				ResourcesLibrary.StartPreloadingMode();
				using (CodeTimer.New(Logger, "Preloading Unit Views in states: Schedule"))
				{
					EventBus.RaiseEvent(delegate(IResourcePreloadHandler h)
					{
						h.OnPreloadResources();
					});
					ResourcesPreload.PreloadUnitViews();
					ResourcesPreload.PreloadMapObjectViews();
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
		foreach (SceneReference activeDynamicScene in area.GetActiveDynamicScenes())
		{
			SceneEntitiesState ses = state.GetStateForScene(activeDynamicScene);
			if (ses != state.MainState && !ses.HasEntityData)
			{
				ses = AreaDataStash.UnstashAreaSubState(state, ses);
				if (!ses.IsPostLoadExecuted)
				{
					ses.PostLoad();
				}
			}
			using (CodeTimer.New(Logger, "Match Scene: " + ses.SceneName))
			{
				using (ProfileScope.New("Match Scene: " + ses.SceneName))
				{
					IEnumerator scan = MatchStateWithSceneCoroutine(ses);
					while (scan.MoveNext())
					{
						yield return null;
					}
				}
			}
			ses.Subscribe();
			yield return null;
		}
		if (CrossSceneRoot.IsEmpty)
		{
			using (CodeTimer.New(Logger, "Load Cross Scene Entities"))
			{
				using (ProfileScope.New("Load Cross Scene Entities"))
				{
					IEnumerator scan = LoadSceneEntitiesCoroutine(Game.Instance.State.PlayerState.CrossSceneState, Enumerable.Empty<EntityViewBase>());
					while (scan.MoveNext())
					{
						yield return null;
					}
				}
			}
		}
		else
		{
			foreach (Entity allEntityDatum in Game.Instance.State.PlayerState.CrossSceneState.AllEntityData)
			{
				if (allEntityDatum?.View?.GO == null)
				{
					allEntityDatum.AttachToViewOnLoad(null);
					Game.Instance.CrossSceneRoot.Add(allEntityDatum.View.ViewTransform);
					Logger.Warning("Cross-scene entity {0} had no view on area change. Created {1}", allEntityDatum, allEntityDatum.View);
				}
			}
		}
		yield return null;
		ObjectExtensions.Or(AstarPath.active, null)?.FlushGraphUpdates();
		yield return null;
		Game.Instance.EntitySpawner.Tick();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			allBaseUnit.CombatGroup.EnsureAndUpdateGroup();
		}
		if (enterPoint != null)
		{
			using (ProfileScope.New("Move Characters"))
			{
				AreaEnterPoint areaEnterPoint = AreaEnterPoint.FindAreaEnterPointOnScene(enterPoint);
				if (!areaEnterPoint)
				{
					throw new ArgumentException($"Cant find EnterPoint {enterPoint} on scene", "enterPoint");
				}
				Game.Instance.Player.MoveCharacters(areaEnterPoint, moveFollowers: false, area.IsPartyArea);
			}
		}
		yield return null;
		using (CodeTimer.New(Logger, "Destroy Unattached Views"))
		{
			using (ProfileScope.New("Destroy Unattached Views"))
			{
				EntityViewBase[] array = UnityEngine.Object.FindObjectsOfType<EntityViewBase>();
				foreach (EntityViewBase entityViewBase in array)
				{
					if (!entityViewBase.IsInState && !Game.Instance.EntitySpawner.IsViewWaitToCreate(entityViewBase))
					{
						if (TraceLoading)
						{
							PFLog.SceneLoader.Log("Destroying extra view " + entityViewBase.name + " #" + entityViewBase.UniqueId);
						}
						entityViewBase.DestroyViewObject();
					}
				}
			}
		}
		using (CodeTimer.New(Logger, "Turn On"))
		{
			using (ProfileScope.New("Turn On"))
			{
				state.Subscribe();
			}
		}
		yield return null;
		IEnumerator reloadUIScene = ReloadUIScene(area);
		while (reloadUIScene.MoveNext())
		{
			yield return null;
		}
		yield return null;
		using (ProfileScope.New("Finish Loading State"))
		{
			CurrentlyLoadedTimeOfDay = Game.Instance.TimeOfDay;
		}
		using (ProfileScope.New("Process area spawners"))
		{
			using (CodeTimerTraceScope.New("Spawners"))
			{
				IEnumerator<object> sp = ProcessSpawnerActivationCoroutine(enterPoint);
				while (sp.MoveNext())
				{
					yield return null;
				}
			}
		}
		using (ProfileScope.New("AreaParts Bounds Setup"))
		{
			foreach (BlueprintAreaPart item in area.GetParts().Concat(area))
			{
				if (item.Bounds == null)
				{
					continue;
				}
				foreach (FogOfWarArea fowArea in FogOfWarArea.All)
				{
					if (item.GetAllScenes(Application.isConsolePlatform).Any((SceneReference sceneRef) => sceneRef.SceneName == fowArea.gameObject.scene.name))
					{
						SetFogOfWarParameters(fowArea, item.Bounds);
					}
				}
			}
		}
		reload = SetupFogOfWar(stash: false);
		while (!reload.IsCompleted)
		{
			yield return null;
		}
		reload.Wait();
		if (area.CameraRotationOnEnter)
		{
			CameraRig.Instance.RotateToImmediately(area.DefaultCameraRotation);
		}
	}

	private static void SetFogOfWarParameters(FogOfWarArea fowArea, AreaPartBounds areaPartBounds)
	{
		Bounds fogOfWarBounds = areaPartBounds.FogOfWarBounds;
		fogOfWarBounds.center -= fowArea.transform.position;
		fowArea.Bounds = fogOfWarBounds;
		fowArea.IsBlurEnabled = areaPartBounds.FogOfWarIsBlurEnabled;
		fowArea.ShadowFalloff = areaPartBounds.FogOfWarShadowFalloff;
		fowArea.StaticMask = areaPartBounds.FogOfWarStaticMask;
		fowArea.RevealOnStart = areaPartBounds.FowStartOptions.HasFlag(FoWStartOptions.RevealFoW);
		fowArea.BorderSettings = areaPartBounds.FogOfWarBorderSettings;
		LineOfSightGeometry.Instance.Init(fowArea.GetWorldBounds());
		ApplyFowStartOptions(areaPartBounds);
	}

	private static void ApplyFowStartOptions(AreaPartBounds areaPartBounds)
	{
		if (!areaPartBounds.FowStartOptions.HasFlag(FoWStartOptions.ShowTransitionMarkers))
		{
			return;
		}
		foreach (Entity entity in Game.Instance.State.Entities)
		{
			if (entity is MapObjectEntity mapObjectEntity && mapObjectEntity.Parts.GetOptional<AreaTransitionPart>() != null)
			{
				mapObjectEntity.IsRevealed = true;
			}
		}
	}

	public IEnumerator<object> ReloadAreaMechanicsCoroutine()
	{
		if (!Application.isPlaying)
		{
			yield break;
		}
		yield return null;
		Game.Instance.EntitySpawner.Tick();
		Game.Instance.EntityDestroyer.Tick();
		List<SceneEntitiesState> oldStates = (from s in Game.Instance.State.LoadedAreaState.GetAllSceneStates()
			where s.IsSceneLoaded
			select s).ToList();
		ReloadSoundBanks(CurrentlyLoadedArea, CurrentlyLoadedAreaPart);
		Task reload = MatchAreaStaticAndDynamicScenesCoroutine(CurrentlyLoadedArea, CurrentlyLoadedAreaPart);
		while (!reload.IsCompleted)
		{
			yield return null;
		}
		reload.Wait();
		AreaPersistentState state = Game.Instance.State.LoadedAreaState;
		Game.Instance.EntitySpawner.SuppressSpawn.Retain();
		foreach (SceneReference activeDynamicScene in CurrentlyLoadedArea.GetActiveDynamicScenes())
		{
			SceneEntitiesState ses = state.GetStateForScene(activeDynamicScene);
			if (!oldStates.Contains(ses))
			{
				ses = AreaDataStash.UnstashAreaSubState(state, ses);
				if (!ses.IsPostLoadExecuted)
				{
					ses.PostLoad();
				}
				IEnumerator mathcState = MatchStateWithSceneCoroutine(ses);
				while (mathcState.MoveNext())
				{
					yield return null;
				}
				ses.Subscribe();
				yield return null;
			}
		}
		foreach (SceneEntitiesState item in oldStates)
		{
			if (!item.IsSceneLoaded)
			{
				item.Unsubscribe();
				item.PreSave();
			}
		}
		foreach (SceneEntitiesState item2 in oldStates)
		{
			if (!item2.IsSceneLoaded)
			{
				AreaDataStash.StashAreaSubState(state, item2);
				item2.Dispose();
			}
			yield return null;
		}
		Game.Instance.EntitySpawner.SuppressSpawn.Release();
		yield return null;
		EntitySpawnController entitySpawner = Game.Instance.EntitySpawner;
		GameObject[] children = DynamicRoot.Children;
		for (int i = 0; i < children.Length; i++)
		{
			EntityViewBase component = children[i].GetComponent<EntityViewBase>();
			if ((bool)component && !component.IsInState && !entitySpawner.IsViewWaitToCreate(component))
			{
				if (TraceLoading)
				{
					PFLog.SceneLoader.Log("Reload: Destroying unattached dynamic view " + component.name + " #" + component.UniqueId);
				}
				component.DestroyViewObject();
			}
		}
		IEnumerator<object> sp = ProcessSpawnerActivationCoroutine(alsoRaiseDead: true);
		while (sp.MoveNext())
		{
			yield return null;
		}
		SoundBanksManager.UnloadNotUsed();
	}

	private static void ReloadSoundBanks(BlueprintArea area, BlueprintAreaPart areaPart)
	{
		SoundBanksManager.MarkBanksToUnload(area.GetActiveSoundBankNames(), area.UnloadBanksDelay);
		if (areaPart != null && areaPart.ManageBanksSeparately)
		{
			SoundBanksManager.MarkBanksToUnload(areaPart.GetActiveSoundBankNames(isCurrentPart: true), areaPart.UnloadBanksDelay);
		}
		SoundBanksManager.LoadBanks(area.GetActiveSoundBankNames());
		if ((bool)areaPart)
		{
			SoundBanksManager.LoadBanks(areaPart.GetActiveSoundBankNames());
		}
	}

	public IEnumerator<object> ProcessSpawnerActivationCoroutine(bool alsoRaiseDead)
	{
		if ((BuildModeUtility.Data?.Loading?.IgnoreSpawners).GetValueOrDefault())
		{
			yield break;
		}
		List<UnitSpawnerBase.MyData> list = (from s in Game.Instance.State.Entities.OfType<UnitSpawnerBase.MyData>()
			where s.ShouldProcessActivation(alsoRaiseDead)
			select s).ToList();
		list.Sort(delegate(UnitSpawnerBase.MyData s1, UnitSpawnerBase.MyData s2)
		{
			string prefabId2 = GetPrefabId(s1);
			string prefabId3 = GetPrefabId(s2);
			return string.Compare(prefabId2, prefabId3, StringComparison.Ordinal);
		});
		HashSet<string> loaded = new HashSet<string>();
		string lastPrefabId = "";
		bool unloadUnitsWhenSpawning = (BuildModeUtility.Data?.Loading?.UnloadUnitsWhenSpawning).GetValueOrDefault();
		foreach (UnitSpawnerBase.MyData spawner in list)
		{
			string prefabId = GetPrefabId(spawner);
			if (prefabId.IsNullOrEmpty())
			{
				PFLog.Default.Warning($"Spawner {spawner.View} has no prefab Id");
				continue;
			}
			if (unloadUnitsWhenSpawning && prefabId != lastPrefabId && loaded.Count >= 3)
			{
				using (CodeTimerTraceScope.New("Unload"))
				{
					ResourcesLibrary.CleanupLoadedCache(ResourcesLibrary.CleanupMode.UnloadNonRequested);
					AsyncOperation uua = Resources.UnloadUnusedAssets();
					while (!uua.isDone)
					{
						yield return null;
					}
					loaded.Clear();
				}
			}
			lastPrefabId = prefabId;
			loaded.Add(prefabId);
			try
			{
				spawner.View.HandleAreaSpawnerInit();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex, $"When spawning {spawner} in {spawner.View?.GO.scene.name}");
			}
			yield return null;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (!allUnit.View)
			{
				PFLog.System.Warning($"Unit {allUnit} has no view, attempting fix");
				try
				{
					allUnit.AttachToViewOnLoad(null);
				}
				catch (Exception ex2)
				{
					PFLog.System.Exception(ex2, $"When fixing lost view for {allUnit}");
				}
			}
			yield return null;
		}
		static string GetPrefabId(UnitSpawnerBase.MyData s1)
		{
			return s1.View?.Blueprint?.Prefab?.AssetId;
		}
	}

	private IEnumerator MatchStateWithSceneCoroutine(SceneEntitiesState state)
	{
		Scene sceneByName = SceneManager.GetSceneByName(state.SceneName);
		if (!sceneByName.isLoaded)
		{
			state.IsSceneLoadedThreadSafe = false;
			yield break;
		}
		Game.Instance?.EntitySpawner?.SuppressSpawn.Retain();
		IEnumerable<EntityViewBase> enumerable = sceneByName.GetRootGameObjects().SelectMany((GameObject obj) => obj.GetComponentsInChildren<EntityViewBase>(includeInactive: true));
		if (!state.HasEntityData)
		{
			IEnumerator createEntities = CreateSceneEntitiesCoroutine(state, enumerable);
			while (createEntities.MoveNext())
			{
				yield return null;
			}
		}
		else
		{
			IEnumerator createEntities = LoadSceneEntitiesCoroutine(state, enumerable);
			while (createEntities.MoveNext())
			{
				yield return null;
			}
		}
		state.IsSceneLoadedThreadSafe = true;
		state.HasEntityData = true;
		Game.Instance?.EntitySpawner?.SuppressSpawn.Release();
	}

	private IEnumerable<BlueprintAreaPart> GetPartsToLoad(BlueprintArea area, BlueprintAreaPart part)
	{
		if ((BuildModeUtility.Data?.Loading?.IgnoreParts).GetValueOrDefault())
		{
			return new BlueprintAreaPart[1] { part ?? area };
		}
		return area.PartsAndSelf;
	}

	private async Task MatchAreaStaticAndDynamicScenesCoroutine(BlueprintArea area, [CanBeNull] BlueprintAreaPart blueprintAreaPart, bool isSmokeTest = false, IProgress<float> progressValueCallback = null)
	{
		Game.Instance.MatchTimeOfDayForced();
		Game.Instance.Player.EtudesSystem.ForceUpdateEtudes(SimpleBlueprintExtendAsObject.Or(blueprintAreaPart, area));
		IEnumerable<BlueprintAreaPart> partsToLoad = GetPartsToLoad(area, blueprintAreaPart);
		IEnumerable<SceneReference> activeDynamicScenes = area.GetActiveDynamicScenes();
		IEnumerable<SceneReference> second = partsToLoad.Select((BlueprintAreaPart p) => p.StaticScene);
		IEnumerable<SceneReference> enumerable = activeDynamicScenes;
		if (!(BuildModeUtility.Data?.Loading?.IgnoreStatic).GetValueOrDefault() && !isSmokeTest)
		{
			enumerable = enumerable.Concat(second);
		}
		SceneReference[] array = enumerable.Distinct().ToArray();
		SceneReference[] source = ((isSmokeTest || (BuildModeUtility.Data?.Loading?.IgnoreLight).GetValueOrDefault()) ? Array.Empty<SceneReference>() : partsToLoad.SelectMany((BlueprintAreaPart p) => new SceneReference[2]
		{
			p.LightScene,
			p.GetAudioScene(Game.Instance.TimeOfDay)
		}).NotNull().ToArray());
		TasksWithCombinedProgress tasksWithCombinedProgress = new TasksWithCombinedProgress(progressValueCallback);
		HashSet<string> hotSceneNames = area.GetHotSceneNames();
		SceneReference[] source2 = Game.Instance.GetAdditiveAreas(area).SelectMany((BlueprintArea a) => a.GetStaticAndActiveDynamicScenes().Concat(a.LightScenes.Where((SceneReference s) => s.IsDefined)).Concat(a.AudioScenes.Where((SceneReference s) => s.IsDefined))).Distinct()
			.ToArray();
		foreach (SceneReference sceneRef in m_LoadedAreaScenes.ToList())
		{
			if (!array.Any((SceneReference s) => s.SceneName == sceneRef.SceneName) && !source2.Any((SceneReference s) => s.SceneName == sceneRef.SceneName) && !source.Any((SceneReference s) => s.SceneName == sceneRef.SceneName))
			{
				bool keepHot = hotSceneNames.Contains(sceneRef.SceneName);
				tasksWithCombinedProgress.Add(UnloadSceneCoroutine(sceneRef, keepHot));
			}
		}
		m_LoadedAreas.Clear();
		m_LoadedAreas.Add(area);
		if (SceneManager.GetSceneByName(GameScenes.MainMenu).isLoaded)
		{
			tasksWithCombinedProgress.Add(UnloadSceneCoroutine(new SceneReference(GameScenes.MainMenu), keepHot: false));
		}
		GameHistoryLog.Instance.AreaLoading(CurrentlyLoadedArea, area, array);
		SceneReference[] array2 = array;
		foreach (SceneReference scene in array2)
		{
			tasksWithCombinedProgress.Add(LoadSceneCoroutine(scene));
		}
		await tasksWithCombinedProgress;
	}

	public static async Task SetupFogOfWar(bool stash, BlueprintAreaPart areaPart = null)
	{
		BlueprintAreaPart currentAreaPart = areaPart ?? SimpleBlueprintExtendAsObject.Or(Game.Instance.CurrentlyLoadedAreaPart, null) ?? Game.Instance.CurrentlyLoadedArea;
		bool ignoreParts = (BuildModeUtility.Data?.Loading?.IgnoreParts).GetValueOrDefault();
		if (currentAreaPart == null)
		{
			LogChannel.Default.Error("SceneLoader.SetupFogOfWar: current area part is null");
			return;
		}
		FogOfWarArea fowArea = FogOfWarArea.Active;
		foreach (FogOfWarArea fa in FogOfWarArea.All)
		{
			if (currentAreaPart.GetAllScenes(Application.isConsolePlatform).Any((SceneReference sceneRef) => sceneRef.SceneName == fa.gameObject.scene.name))
			{
				fowArea = fa;
			}
		}
		if (stash && FogOfWarArea.Active != null && (ignoreParts || fowArea != FogOfWarArea.Active))
		{
			byte[] data = await FogOfWarArea.Active.RequestData();
			Game.Instance.LoadedAreaState.SavedFogOfWarMasks.Add(FogOfWarArea.Active.gameObject.scene.name, data);
		}
		FogOfWarArea.Active = (fowArea ? fowArea : null);
		if (fowArea != null)
		{
			byte[] array = Game.Instance.LoadedAreaState.SavedFogOfWarMasks.Get(fowArea.gameObject.scene.name);
			if ((bool)fowArea && array != null)
			{
				fowArea.RestoreFogOfWarMask(array);
			}
			if (ignoreParts)
			{
				SetFogOfWarParameters(fowArea, currentAreaPart.Bounds);
			}
		}
	}

	public async Task MatchLightAndAudioScenesCoroutine()
	{
		CameraRig.Instance.UpdateForce();
		if ((bool)VFXWeatherSystem.Instance)
		{
			VFXWeatherSystem.Instance.Stop();
		}
		using (CodeTimer.New(Logger, "Match Light: Refresh Wind Controllers"))
		{
			WindController.RefreshInstance();
		}
		BlueprintAreaPart currentAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
		IEnumerable<BlueprintAreaPart> partsToLoad = GetPartsToLoad(CurrentlyLoadedArea, currentAreaPart);
		SoundEventsSwitchingProcess soundSwitching = new SoundEventsSwitchingProcess();
		soundSwitching.BeforeAreaPartChange(currentAreaPart);
		HashSet<string> hotSceneNames;
		SceneReference[] source;
		SceneReference[] array;
		SceneReference[] activeLightScenes;
		SceneReference[] array2;
		bool shouldUnloadAudio;
		bool shouldLoadAudio;
		using (CodeTimer.New(Logger, "Match Light: Collect Required Scenes"))
		{
			hotSceneNames = CurrentlyLoadedArea.GetHotSceneNames();
			source = (CurrentlyLoadedArea.HasLight ? CurrentlyLoadedArea.PartsAndSelf.Select((BlueprintAreaPart p) => p.LightScene).ToArray() : Array.Empty<SceneReference>());
			array = CurrentlyLoadedArea.PartsAndSelf.SelectMany((BlueprintAreaPart p) => p.AudioScenes).ToArray();
			activeLightScenes = ((!CurrentlyLoadedArea.HasLight) ? Array.Empty<SceneReference>() : ((!currentAreaPart.IsSingleLightScene) ? partsToLoad.Select((BlueprintAreaPart p) => p.GetLightScene()).ToArray() : new SceneReference[1] { currentAreaPart.GetLightScene() }));
			if (CurrentlyLoadedArea == currentAreaPart)
			{
				activeLightScenes = activeLightScenes.Except(from p in CurrentlyLoadedArea.GetParts()
					where p.IsSingleLightScene
					select p.LightScene).ToArray();
			}
			array2 = CurrentlyLoadedArea.GetAudioScenes(Game.Instance.TimeOfDay).ToArray();
			if ((BuildModeUtility.Data?.Loading?.IgnoreLight).GetValueOrDefault())
			{
				activeLightScenes = Array.Empty<SceneReference>();
				array2 = Array.Empty<SceneReference>();
			}
			shouldUnloadAudio = m_LoadedAreaScenes.Intersect(array).Except(array2).Any();
			shouldLoadAudio = array2.Except(m_LoadedAreaScenes).Any();
			if (shouldUnloadAudio || shouldLoadAudio)
			{
				AkAudioService.Log.Log("Stopping all audio due to scene reload [StopAllKeepMusic]");
				SoundEventsManager.PostEvent("StopAllKeepMusic", null);
			}
		}
		using (CodeTimer.New(Logger, "Match Light: Scenes Loading"))
		{
			List<Task> list = new List<Task>();
			SceneReference[] array3 = m_LoadedAreaScenes.ToArray();
			foreach (SceneReference sceneReference in array3)
			{
				bool num = array.Contains(sceneReference) || source.Contains(sceneReference);
				bool flag = array2.Contains(sceneReference) || activeLightScenes.Contains(sceneReference);
				if (num && !flag)
				{
					PFLog.SceneLoader.Log("Unload scene: " + sceneReference.SceneName);
					bool keepHot = hotSceneNames.Contains(sceneReference.SceneName);
					list.Add(UnloadSceneCoroutine(sceneReference, keepHot));
				}
			}
			array3 = array2;
			foreach (SceneReference sceneReference2 in array3)
			{
				PFLog.SceneLoader.Log("Load scene: " + sceneReference2.SceneName);
				list.Add(LoadSceneCoroutine(sceneReference2));
			}
			array3 = activeLightScenes;
			foreach (SceneReference sceneReference3 in array3)
			{
				PFLog.SceneLoader.Log("Load scene: " + sceneReference3.SceneName);
				list.Add(LoadSceneCoroutine(sceneReference3));
			}
			await Task.WhenAll(list);
		}
		if (activeLightScenes.Any((SceneReference s) => s.IsDefined && !SceneManager.GetSceneByName(s.SceneName).isLoaded))
		{
			PFLog.SceneLoader.Log("Failed to load light scene. Desired scenes: " + activeLightScenes.Aggregate("", (string a, SceneReference b) => a + ", " + b.SceneName));
			SceneReference sceneReference4 = activeLightScenes.First((SceneReference s) => s.IsDefined && !SceneManager.GetSceneByName(s.SceneName).isLoaded);
			string text = PathUtils.BundlePath((sceneReference4.SceneName + "_bundle").ToLowerInvariant());
			PFLog.SceneLoader.Log("Bad scene: " + sceneReference4.SceneName + " path: " + text);
			PFLog.SceneLoader.Log("Bundles path: " + PathUtils.BundlesFolder);
			if (Directory.Exists(PathUtils.BundlesFolder))
			{
				PFLog.SceneLoader.Log("Bundles path contents: " + Directory.EnumerateFiles(PathUtils.BundlesFolder).Aggregate("", (string a, string b) => a + "\n" + b));
			}
		}
		using (CodeTimer.New(Logger, "Match Light: Set Active Scene"))
		{
			if (currentAreaPart.HasLight)
			{
				SceneReference lightScene = currentAreaPart.GetLightScene();
				Scene sceneByName = SceneManager.GetSceneByName(lightScene.SceneName);
				if (!sceneByName.isLoaded)
				{
					PFLog.SceneLoader.Log("Could not load light scene!");
					SceneManager.SetActiveScene(GetBaseMechanicsScene());
				}
				else
				{
					SceneReference[] array3 = activeLightScenes;
					foreach (SceneReference sceneReference5 in array3)
					{
						bool active = sceneReference5.SceneName == lightScene.SceneName;
						Scene sceneByName2 = SceneManager.GetSceneByName(sceneReference5.SceneName);
						if (!sceneByName2.IsValid())
						{
							PFLog.SceneLoader.Error("Light scene " + sceneReference5.SceneName + " should be loaded, but is not.");
							continue;
						}
						GameObject[] rootGameObjects = sceneByName2.GetRootGameObjects();
						for (int j = 0; j < rootGameObjects.Length; j++)
						{
							rootGameObjects[j].SetActive(active);
						}
					}
					PFLog.SceneLoader.Log("Set active scene: " + sceneByName.name);
					SceneManager.SetActiveScene(sceneByName);
					LightController lightController = LightController.Active;
					if (lightController != null)
					{
						float timeout = 60f;
						DynamicRoot.Hide();
						CrossSceneRoot.Hide();
						try
						{
							lightController.ChangeDayTime(Game.Instance.TimeOfDay);
							if (QualitySettings.realtimeReflectionProbes)
							{
								while (lightController.IsProbeBaking)
								{
									if (timeout <= 0f)
									{
										PFLog.SceneLoader.Error("Light probes baking process takes too long!");
										break;
									}
									await Task.Yield();
									timeout -= Time.unscaledDeltaTime;
								}
								PFLog.SceneLoader.Log("Ended waiting for light probes baking.");
							}
							else
							{
								PFLog.SceneLoader.Log("Can't bake reflections. realtimeReflectionProbes is off.");
							}
						}
						finally
						{
							DynamicRoot.Unhide();
							CrossSceneRoot.Unhide();
						}
					}
				}
			}
		}
		MatchStaticSceneActiveStatus(currentAreaPart);
		using (CodeTimer.New(Logger, "Set active FogOfWarArea"))
		{
			if (currentAreaPart != null && currentAreaPart.Bounds != null)
			{
				await SetupFogOfWar(!(BuildModeUtility.Data?.Loading?.IgnoreParts).GetValueOrDefault());
			}
		}
		Physics.RebuildBroadphaseRegions(currentAreaPart.Bounds.MaxBounds, 10);
		if (shouldLoadAudio || shouldUnloadAudio)
		{
			AkAudioService.Log.Log("Triggering [OnAudioReloaded]");
			EventBus.RaiseEvent(delegate(IAudioSceneHandler h)
			{
				h.OnAudioReloaded();
			});
		}
		using (CodeTimer.New(Logger, "Match Light: Apply Rendering Settings"))
		{
			if (Application.isPlaying)
			{
				RenderingManager.Instance?.ApplySettings();
			}
		}
		using (CodeTimer.New(Logger, "Match Light: Reinit Weather"))
		{
			if ((bool)VFXWeatherSystem.Instance)
			{
				VFXLocationWeatherData[] array4 = Resources.FindObjectsOfTypeAll<VFXLocationWeatherData>();
				foreach (VFXLocationWeatherData bakedGroundArea in array4)
				{
					if (currentAreaPart.GetAllScenes(Application.isConsolePlatform).Any((SceneReference sceneRef) => sceneRef.SceneName == bakedGroundArea.gameObject.scene.name))
					{
						bakedGroundArea.SetCurrentWeatherProfile(currentAreaPart.WeatherProfile);
						break;
					}
				}
				VFXTotalLocationWeatherData.RemoveAllEmptyAreas();
				if (VFXWeatherSystem.Instance != null)
				{
					VFXWeatherSystem.Instance.Profile = currentAreaPart.WeatherProfile;
					VFXWeatherSystem.Instance.Reset();
				}
				EventBus.RaiseEvent(delegate(IWeatherUpdateHandler h)
				{
					h.OnUpdateWeatherSystem(overrideWeather: false);
				});
			}
		}
		IEnumerator switchProcess = soundSwitching.AfterAreaPartChange(currentAreaPart);
		while (switchProcess.MoveNext())
		{
			await Task.Yield();
		}
	}

	private void MatchStaticSceneActiveStatus(BlueprintAreaPart currentAreaPart)
	{
		using (CodeTimer.New(Logger, "Match Light: De/activate static scenes for parts"))
		{
			foreach (BlueprintAreaPartReference part in CurrentlyLoadedArea.Parts)
			{
				BlueprintAreaPart blueprintAreaPart = part.Get();
				if ((bool)blueprintAreaPart && blueprintAreaPart.StaticScene.IsDefined)
				{
					Scene sceneByName = SceneManager.GetSceneByName(blueprintAreaPart.StaticScene.SceneName);
					SetSceneEnabled(sceneByName, currentAreaPart == null || blueprintAreaPart == currentAreaPart);
				}
			}
			SetSceneEnabled(SceneManager.GetSceneByName(CurrentlyLoadedArea.StaticScene.SceneName), currentAreaPart == null || currentAreaPart == CurrentlyLoadedArea);
		}
	}

	private void SetSceneEnabled(Scene scene, bool enabled)
	{
		if (!scene.IsValid() || !scene.isLoaded)
		{
			return;
		}
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		foreach (GameObject gameObject in rootGameObjects)
		{
			bool alreadyExists;
			StaticObjectMark staticObjectMark = gameObject.EnsureComponent<StaticObjectMark>(out alreadyExists);
			if (!alreadyExists && !gameObject.activeSelf)
			{
				staticObjectMark.AlwaysDisabled = true;
			}
			if (!staticObjectMark.AlwaysDisabled)
			{
				gameObject.SetActive(enabled);
			}
		}
	}

	private async Task LoadSceneCoroutine(SceneReference scene)
	{
		if (!Application.isPlaying || !scene.IsDefined)
		{
			return;
		}
		bool flag = m_LoadedAreaScenes.Any((SceneReference sr) => sr.SceneName == scene.SceneName);
		m_LoadedAreaScenes.Add(scene);
		if (HotScenesManager.IsSceneDeactivated(scene.SceneName))
		{
			HotScenesManager.ActivateScene(scene.SceneName);
			return;
		}
		Scene sceneByName = SceneManager.GetSceneByName(scene.SceneName);
		if (flag || sceneByName.isLoaded)
		{
			PFLog.System.Log("Already loaded: {0}", scene.SceneName);
		}
		else
		{
			await BundledSceneLoader.LoadSceneAsync(scene.SceneName);
		}
	}

	private async Task UnloadSceneCoroutine(SceneReference scene, bool keepHot)
	{
		if (!Application.isPlaying || !scene.IsDefined)
		{
			return;
		}
		m_LoadedAreaScenes.Remove(scene);
		if (m_LoadedAreaScenes.Any((SceneReference s) => s.SceneName == scene.SceneName))
		{
			PFLog.System.Log("Has another reference to scene, can't unload: {0}", scene.SceneName);
			return;
		}
		Scene sceneByName = SceneManager.GetSceneByName(scene.SceneName);
		if (!sceneByName.isLoaded)
		{
			PFLog.System.Log("Not loaded, can't unload: {0}", scene.SceneName);
			return;
		}
		if (keepHot)
		{
			HotScenesManager.DeactivateScene(scene.SceneName);
			return;
		}
		HotScenesManager.RemoveScene(scene.SceneName);
		AudioFade audioFade = (from o in sceneByName.GetRootGameObjects()
			select o.GetComponent<AudioFade>()).FirstOrDefault((AudioFade o) => o);
		int fade = (audioFade ? ((int)(audioFade.FadeTime * 1000f)) : 1000);
		AkAudioService.Log.Log("Stopping all audio on scene unload: for " + sceneByName.name);
		foreach (AudioObject item in ObjectRegistry<AudioObject>.Instance)
		{
			if (item.gameObject.scene == sceneByName)
			{
				AkAudioTriggerable[] components = item.GetComponents<AkAudioTriggerable>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].StopAndLog(fade);
				}
			}
		}
		if ((BuildModeUtility.Data?.Loading?.UnloadSceneBundles).GetValueOrDefault())
		{
			PFLog.SceneLoader.Log("Unloading area scene and bundle: " + scene.SceneName);
			await BundledSceneLoader.UnloadSceneAsync(scene.SceneName);
		}
		else
		{
			PFLog.SceneLoader.Log("Unloading area scene: " + scene.SceneName);
			await SceneManager.UnloadSceneAsync(scene.SceneName);
		}
	}

	private IEnumerator UnloadInGameUiSceneCoroutine()
	{
		if (string.IsNullOrEmpty(m_LoadedUIScene))
		{
			yield break;
		}
		using (CodeTimer.New(Logger, "Unloading UI scene"))
		{
			Game.Instance.RootUiContext?.DisposeUiScene();
			if (SceneManager.GetSceneByName(m_LoadedUIScene).isLoaded)
			{
				Task op = BundledSceneLoader.UnloadSceneAsync(m_LoadedUIScene);
				while (!op.IsCompleted)
				{
					yield return null;
				}
				op.Wait();
				m_LoadedUIScene = "";
			}
		}
	}

	private IEnumerator CreateSceneEntitiesCoroutine(SceneEntitiesState state, IEnumerable<EntityViewBase> allViews)
	{
		foreach (EntityViewBase allView in allViews)
		{
			if (allView.gameObject.activeInHierarchy && (allView.Data == null || !allView.Data.IsInState))
			{
				CreateDataForView(state, allView, load: false);
				if (TraceLoading && (bool)allView)
				{
					PFLog.SceneLoader.Log($"Spawning entity for view {allView.name} #{allView.UniqueId} ({allView.Data})");
				}
				yield return null;
			}
		}
	}

	private static void CreateDataForView(SceneEntitiesState state, EntityViewBase view, bool load)
	{
		try
		{
			Entity entity = view.CreateEntityData(load);
			if (entity == null)
			{
				PFLog.SceneLoader.Warning("Entity view {0} (id {1}) failed to create data object. Deleting view.", view.name, view.UniqueId);
				view.DestroyViewObject();
			}
			else
			{
				entity.AttachView(view);
				Game.Instance.EntitySpawner.SpawnEntityImmediately(entity, state);
			}
		}
		catch (Exception ex)
		{
			PFLog.SceneLoader.Error(view, $"Exception when creating data for {view}: {ex.Message}");
			throw;
		}
	}

	private IEnumerator LoadSceneEntitiesCoroutine(SceneEntitiesState state, IEnumerable<EntityViewBase> viewList)
	{
		Dictionary<string, EntityViewBase> allViews = new Dictionary<string, EntityViewBase>();
		foreach (EntityViewBase view in viewList)
		{
			try
			{
				allViews.Add(view.UniqueId, view);
			}
			catch (ArgumentException)
			{
				PFLog.SceneLoader.ErrorWithReport(view, "Area contains views with conflicting ID [{0}]: '{1}' in {3} and '{2}' in {4}. Skipping the second one.", view.UniqueId, allViews[view.UniqueId].name, view.name, allViews[view.UniqueId].gameObject.scene.name, view.gameObject.scene.name);
			}
		}
		foreach (Entity allEntityDatum in state.AllEntityData)
		{
			if (!allEntityDatum.IsInState || (BuildModeUtility.Data.Loading.IgnoreSpawners && allEntityDatum is UnitEntity && state != Game.Instance.Player.CrossSceneState))
			{
				continue;
			}
			EntityViewBase value = null;
			if (allEntityDatum.UniqueId == null)
			{
				PFLog.SceneLoader.Error($"Entity with null UniqueId while loading: {allEntityDatum}");
			}
			else
			{
				allViews.TryGetValue(allEntityDatum.UniqueId, out value);
			}
			if (TraceLoading)
			{
				PFLog.SceneLoader.Log("Loaded entity {0}: {1}. View found: {2}", allEntityDatum.GetType().Name, allEntityDatum.UniqueId, value ? value.name : "none");
			}
			allEntityDatum.AttachToViewOnLoad(value);
			if (!value && (bool)allEntityDatum.View?.GO)
			{
				((state == Game.Instance.State.PlayerState.CrossSceneState) ? CrossSceneRoot : DynamicRoot).Add(allEntityDatum.View.ViewTransform);
				if (state == Game.Instance.State.PlayerState.CrossSceneState)
				{
					allEntityDatum.GetOrCreate<PartHoldPrefabBundle>();
				}
				if (TraceLoading)
				{
					PFLog.SceneLoader.Log("Spawned dynamic view " + allEntityDatum.View.GO.name + " #" + allEntityDatum.View.UniqueViewId);
				}
			}
			yield return null;
		}
		foreach (EntityViewBase value2 in allViews.Values)
		{
			if (!value2.gameObject.activeInHierarchy || (value2.Data?.IsInState ?? false))
			{
				continue;
			}
			if (value2.CreatesDataOnLoad)
			{
				try
				{
					CreateDataForView(state, value2, load: true);
				}
				catch (Exception)
				{
					PFLog.SceneLoader.Error($"Exception when creating data for {value2} in {value2.gameObject.scene.name}");
					throw;
				}
			}
			yield return null;
		}
		foreach (Entity item in state.AllEntityData.Where((Entity d) => d.View == null && d.NeedsView).ToList())
		{
			if (TraceLoading)
			{
				PFLog.SceneLoader.Log("Removed entity " + item?.ToString() + " #" + item.UniqueId);
			}
			state.RemoveEntityData(item);
			item.Dispose();
		}
	}

	public IEnumerator<object> UnloadAreaCoroutine(bool forDispose = false, bool leaveStatics = false, bool unloadUi = true, [CanBeNull] HashSet<string> hotScenes = null)
	{
		Game.Instance.IsUnloading = forDispose;
		while (Game.Instance.SaveManager.CommitInProgress)
		{
			yield return null;
		}
		Game.Instance.HandleAreaBeginUnloading();
		Game.Instance.State.LoadedAreaState.Deactivate();
		if (VFXWeatherSystem.Instance != null)
		{
			VFXWeatherSystem.Instance.Stop();
		}
		if (!forDispose)
		{
			foreach (BaseUnitEntity item2 in Game.Instance.Player.CrossSceneState.AllEntityData.OfType<BaseUnitEntity>())
			{
				item2.Commands.InterruptAllInterruptible();
			}
			foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
			{
				BaseUnitEntity item = allCrossSceneUnit.Master ?? allCrossSceneUnit;
				allCrossSceneUnit.IsInGame = Game.Instance.Player.Party.Contains(item);
				if (!allCrossSceneUnit.IsInGame && !allCrossSceneUnit.GetOptional<UnitPartCompanion>())
				{
					Game.Instance.EntityDestroyer.Destroy(allCrossSceneUnit);
				}
			}
		}
		yield return null;
		if (unloadUi || BuildModeUtility.ExtraOptimization)
		{
			IEnumerator op = UnloadInGameUiSceneCoroutine();
			while (op.MoveNext())
			{
				yield return null;
			}
		}
		IEnumerable<string> additiveStaticScenes = null;
		if (CurrentlyLoadedArea != null)
		{
			additiveStaticScenes = from sr in Game.Instance.GetAdditiveAreas(CurrentlyLoadedArea).SelectMany((BlueprintArea a) => a.GetStaticAndActiveDynamicScenes()).Distinct()
				select sr.SceneName;
		}
		if (CurrentlyLoadedAreaPart != null && CurrentlyLoadedAreaPart.ManageBanksSeparately)
		{
			SoundBanksManager.MarkBanksToUnload(CurrentlyLoadedAreaPart.GetActiveSoundBankNames(isCurrentPart: true), CurrentlyLoadedAreaPart.UnloadBanksDelay);
		}
		if (CurrentlyLoadedArea != null)
		{
			SoundBanksManager.MarkBanksToUnload(CurrentlyLoadedArea.GetActiveSoundBankNames(), CurrentlyLoadedArea.UnloadBanksDelay);
		}
		CurrentlyLoadedArea = null;
		CurrentlyLoadedAreaPart = null;
		m_LoadedAreas.Clear();
		List<SoundEventsEmitter.Info> list = ListPool<SoundEventsEmitter.Info>.Claim();
		foreach (SoundEventsEmitter.Info item3 in ObjectRegistry<SoundEventsEmitter.Info>.Instance)
		{
			item3.EmitDeactivated();
			item3.UnloadBank();
			if (!leaveStatics || !item3.Scene.name.Contains("_Static"))
			{
				list.Add(item3);
			}
		}
		foreach (SoundEventsEmitter.Info item4 in list)
		{
			item4.Disable();
		}
		ListPool<SoundEventsEmitter.Info>.Release(list);
		AreaPersistentState state = Game.Instance.State.LoadedAreaState;
		if (forDispose)
		{
			state.Dispose();
		}
		else
		{
			state.Unsubscribe();
			if (state.Blueprint.ExcludeFromSave)
			{
				state.Dispose();
				Game.Instance.State.SavedAreaStates.Remove(state);
			}
			else
			{
				Task encodeFogOp = AreaDataStash.EncodeActiveAreaFog(state);
				if (encodeFogOp != null)
				{
					while (!encodeFogOp.IsCompleted)
					{
						yield return null;
					}
				}
				state.PreSave();
				AreaDataStash.StashAreaState(state, dispose: true);
			}
		}
		Game.Instance.State.LoadedAreaState = null;
		if (Application.isPlaying)
		{
			string[] array = m_LoadedAreaScenes.Select((SceneReference sr) => sr.SceneName).Concat(HotScenesManager.DeactivatedScenes).Distinct()
				.ToArray();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!leaveStatics || !text.Contains("_Static") || additiveStaticScenes.Contains(text))
				{
					bool keepHot = hotScenes?.Contains(text) ?? false;
					Task encodeFogOp = UnloadSceneCoroutine(new SceneReference(text), keepHot);
					while (!encodeFogOp.IsCompleted)
					{
						yield return null;
					}
				}
			}
		}
		SoundBanksManager.UnloadNotUsed();
		if (BuildModeUtility.ExtraOptimization)
		{
			if (CrossSceneRoot != null)
			{
				GameObject[] children = CrossSceneRoot.Children;
				foreach (GameObject gameObject in children)
				{
					Character character = (gameObject ? ObjectExtensions.Or(gameObject.GetComponent<Character>(), null) : null);
					if ((bool)character && !character.gameObject.activeInHierarchy)
					{
						character.SendMessage("OnDestroy");
					}
				}
			}
			m_LoadedAreaScenes.Clear();
			RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
			CameraStackManager.CameraStackState state2 = CameraStackManager.Instance.State;
			CameraStackManager.Instance.State = CameraStackManager.CameraStackState.UiOnly;
			Camera firstBase = CameraStackManager.Instance.GetFirstBase();
			RenderTexture targetTexture = firstBase.targetTexture;
			firstBase.targetTexture = rt;
			firstBase.Render();
			firstBase.targetTexture = targetTexture;
			CameraStackManager.Instance.State = state2;
			FakeLoadingScreen fake = new GameObject("fake loading screen").AddComponent<FakeLoadingScreen>();
			fake.Texture = rt;
			UnityEngine.Object.DontDestroyOnLoad(fake);
			WidgetFactoryStash.ResetStash();
			Game.Instance.RootUiContext.Dispose();
			yield return null;
			ResourcesLibrary.CleanupLoadedCache(ResourcesLibrary.CleanupMode.UnloadEverything);
			yield return null;
			SceneManager.LoadScene("ExtraEmpty");
			yield return null;
			yield return null;
			yield return null;
			if (!SceneManager.GetSceneByName("IngameConsole").isLoaded)
			{
				IngameConsoleInitializer.Init();
			}
			Task encodeFogOp = BundledSceneLoader.LoadSceneAsync("LoadingScreen");
			while (!encodeFogOp.IsCompleted)
			{
				yield return null;
			}
			Game.Instance.RootUiContext.InitializeLoadingScreenScene("LoadingScreen");
			Task op = BundledSceneLoader.LoadSceneAsync("UI_Common_Scene");
			while (!op.IsCompleted)
			{
				yield return null;
			}
			Game.Instance.RootUiContext.InitializeCommonScene("UI_Common_Scene", showLoadingScreen: true);
			LoadingProcess.Instance.ResetLoadingScreen();
			yield return null;
			RenderTexture.ReleaseTemporary(rt);
			UnityEngine.Object.DestroyImmediate(fake.gameObject);
			Runner.SkipInit = true;
			yield return LoadObligatoryScenesAsync();
			Runner.SkipInit = false;
		}
		ClearCachedData();
		Game.Instance.IsUnloading = false;
	}

	public IEnumerator<object> UnloadEntitiesCoroutine(bool unloadCrossScene)
	{
		yield return DynamicRoot.Clear();
		if (unloadCrossScene)
		{
			yield return CrossSceneRoot.Clear();
		}
	}

	public void ClearLoadedArea()
	{
		CurrentlyLoadedArea = null;
		CurrentlyLoadedAreaPart = null;
		m_LoadedAreas.Clear();
	}

	public IEnumerator LoadAdditiveArea(BlueprintArea area, bool isSmokeTest = false)
	{
		SceneReference[] array = (isSmokeTest ? area.GetActiveDynamicScenes() : area.GetStaticAndActiveDynamicScenes().Concat(area.LightScenes.Where((SceneReference s) => s.IsDefined)).Concat(area.AudioScenes.Where((SceneReference s) => s.IsDefined))).Distinct().ToArray();
		SceneReference[] array2 = array;
		foreach (SceneReference sceneRef in array2)
		{
			Task asyncLoad = BundledSceneLoader.LoadSceneAsync(sceneRef.SceneName);
			while (!asyncLoad.IsCompleted)
			{
				yield return null;
			}
			asyncLoad.Wait();
			Scene sceneByName = SceneManager.GetSceneByName(sceneRef.SceneName);
			m_LoadedAreaScenes.Add(sceneRef);
			SetSceneEnabled(sceneByName, enabled: false);
		}
		m_LoadedAreas.Add(area);
	}

	public IEnumerator UnloadAdditiveArea(BlueprintArea area)
	{
		if (area != null && area.ManageBanksSeparately)
		{
			SoundBanksManager.MarkBanksToUnload(area.GetActiveSoundBankNames(isCurrentPart: true));
		}
		List<SoundEventsEmitter.Info> list = ListPool<SoundEventsEmitter.Info>.Claim();
		foreach (SoundEventsEmitter.Info item in ObjectRegistry<SoundEventsEmitter.Info>.Instance)
		{
			item.EmitDeactivated();
			item.UnloadBank();
			list.Add(item);
		}
		foreach (SoundEventsEmitter.Info item2 in list)
		{
			item2.Disable();
		}
		ListPool<SoundEventsEmitter.Info>.Release(list);
		SoundBanksManager.UnloadNotUsed();
		if (Application.isPlaying)
		{
			string[] array = (from s in area.GetStaticAndActiveDynamicScenes().Concat(area.LightScenes.Where((SceneReference s) => s.IsDefined)).Concat(area.AudioScenes.Where((SceneReference s) => s.IsDefined))
				select s.SceneName).Distinct().ToArray();
			string[] array2 = array;
			foreach (string sceneName in array2)
			{
				Task sceneUnloader = UnloadSceneCoroutine(new SceneReference(sceneName), keepHot: false);
				while (!sceneUnloader.IsCompleted)
				{
					yield return null;
				}
			}
		}
		m_LoadedAreas.Remove(area);
	}

	public bool IsAreaLoaded(BlueprintArea area)
	{
		return m_LoadedAreas.Contains(area);
	}

	public IEnumerator ActivateAdditiveArea(BlueprintAreaEnterPoint enterPoint, bool isSmokeTest = false, bool showLoadingScreen = true)
	{
		Game.Instance.HandleAdditiveAreaBeginDeactivating();
		Game.Instance.State.LoadedAreaState.Deactivate();
		BlueprintArea area = enterPoint.Area;
		bool shouldReloadUiScene = CurrentlyLoadedArea.ActiveUIScene != area.ActiveUIScene;
		SceneReference[] array = (isSmokeTest ? CurrentlyLoadedArea.GetActiveDynamicScenes() : CurrentlyLoadedArea.GetStaticAndActiveDynamicScenes().Concat(CurrentlyLoadedArea.LightScenes.Where((SceneReference s) => s.IsDefined)).Concat(CurrentlyLoadedArea.AudioScenes.Where((SceneReference s) => s.IsDefined))).Distinct().ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Scene sceneByName = SceneManager.GetSceneByName(array[i].SceneName);
			SetSceneEnabled(sceneByName, enabled: false);
		}
		foreach (BaseUnitEntity item2 in Game.Instance.Player.CrossSceneState.AllEntityData.OfType<BaseUnitEntity>())
		{
			item2.Commands.InterruptAllInterruptible();
		}
		foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
		{
			BaseUnitEntity item = allCrossSceneUnit.Master ?? allCrossSceneUnit;
			allCrossSceneUnit.IsInGame = Game.Instance.Player.Party.Contains(item);
			if (!allCrossSceneUnit.IsInGame && !allCrossSceneUnit.GetOptional<UnitPartCompanion>())
			{
				Game.Instance.EntityDestroyer.Destroy(allCrossSceneUnit);
			}
		}
		AreaPersistentState loadedAreaState = Game.Instance.State.LoadedAreaState;
		loadedAreaState.Unsubscribe();
		if (loadedAreaState.Blueprint.ExcludeFromSave)
		{
			loadedAreaState.Dispose();
			Game.Instance.State.SavedAreaStates.Remove(loadedAreaState);
		}
		else
		{
			loadedAreaState.PreSave();
			AreaDataStash.StashAreaState(loadedAreaState, dispose: true);
		}
		yield return null;
		CurrentlyLoadedArea = area;
		CurrentlyLoadedAreaPart = area;
		SceneReference[] mechanicsLinkedToArea = CurrentlyLoadedArea.GetActiveDynamicScenes().ToArray();
		Game.Instance.Player.EtudesSystem.ForceUpdateEtudes(area);
		SceneReference[] scenesToActivate = (isSmokeTest ? CurrentlyLoadedArea.GetActiveDynamicScenes() : CurrentlyLoadedArea.GetStaticAndActiveDynamicScenes().Concat(CurrentlyLoadedArea.LightScenes.Where((SceneReference s) => s.IsDefined)).Concat(CurrentlyLoadedArea.AudioScenes.Where((SceneReference s) => s.IsDefined))).Distinct().ToArray();
		SceneReference[] array2 = scenesToActivate;
		Task asyncLoader;
		foreach (SceneReference sceneRef in array2)
		{
			if (m_LoadedAreaScenes.Contains(sceneRef))
			{
				Scene sceneByName2 = SceneManager.GetSceneByName(sceneRef.SceneName);
				SetSceneEnabled(sceneByName2, enabled: true);
				continue;
			}
			asyncLoader = BundledSceneLoader.LoadSceneAsync(sceneRef.SceneName);
			while (!asyncLoader.IsCompleted)
			{
				yield return null;
			}
			asyncLoader.Wait();
			m_LoadedAreaScenes.Add(sceneRef);
		}
		ReloadSoundBanks(CurrentlyLoadedArea, CurrentlyLoadedAreaPart);
		foreach (SceneReference item3 in mechanicsLinkedToArea.Except(scenesToActivate))
		{
			asyncLoader = UnloadSceneCoroutine(new SceneReference(item3.SceneName), keepHot: false);
			while (!asyncLoader.IsCompleted)
			{
				yield return null;
			}
		}
		if (CurrentlyLoadedArea.IsNavmeshArea && !AstarPath.active)
		{
			throw new InvalidOperationException($"AStarPath not found on area {CurrentlyLoadedArea}");
		}
		using (ProfileScope.New("Clean Up Dynamic Roots"))
		{
			using (CodeTimer.New(Logger, "Clean up dynamic roots"))
			{
				yield return DynamicRoot.Clear();
				if ((bool)FxHelper.FxRoot)
				{
					DynamicRoot.Add(FxHelper.FxRoot);
				}
			}
		}
		yield return null;
		AreaPersistentState state = Game.Instance.State.GetStateForArea(area);
		bool shouldLoad = state.ShouldLoad;
		if (shouldLoad)
		{
			using (CodeTimer.New(Logger, $"Unstash area state: {state.Blueprint}"))
			{
				Game.Instance.State.SavedAreaStates.Remove(state);
				state = AreaDataStash.UnstashAreaState(state);
				Game.Instance.State.SavedAreaStates.Add(state);
			}
		}
		Game.Instance.State.LoadedAreaState = state;
		if (shouldLoad)
		{
			using (ProfileScope.New("PostLoad area state"))
			{
				state.PostLoad();
			}
		}
		using (CodeTimer.New(Logger, "Preloading Unit Views in states"))
		{
			try
			{
				ResourcesLibrary.StartPreloadingMode();
				using (CodeTimer.New(Logger, "Preloading Unit Views in states: Schedule"))
				{
					EventBus.RaiseEvent(delegate(IResourcePreloadHandler h)
					{
						h.OnPreloadResources();
					});
					ResourcesPreload.PreloadUnitViews();
					ResourcesPreload.PreloadMapObjectViews();
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
		foreach (SceneReference activeDynamicScene in area.GetActiveDynamicScenes())
		{
			SceneEntitiesState sceneEntitiesState = state.GetStateForScene(activeDynamicScene);
			if (sceneEntitiesState != state.MainState && !sceneEntitiesState.HasEntityData)
			{
				sceneEntitiesState = AreaDataStash.UnstashAreaSubState(state, sceneEntitiesState);
				if (!sceneEntitiesState.IsPostLoadExecuted)
				{
					sceneEntitiesState.PostLoad();
				}
			}
			using (CodeTimer.New(Logger, "Match Scene: " + sceneEntitiesState.SceneName))
			{
				using (ProfileScope.New("Match Scene: " + sceneEntitiesState.SceneName))
				{
					IEnumerator enumerator3 = MatchStateWithSceneCoroutine(sceneEntitiesState);
					while (enumerator3.MoveNext())
					{
					}
				}
			}
			sceneEntitiesState.Subscribe();
			yield return null;
		}
		if (CrossSceneRoot.IsEmpty)
		{
			using (CodeTimer.New(Logger, "Load Cross Scene Entities"))
			{
				using (ProfileScope.New("Load Cross Scene Entities"))
				{
					IEnumerator enumerator4 = LoadSceneEntitiesCoroutine(Game.Instance.State.PlayerState.CrossSceneState, Enumerable.Empty<EntityViewBase>());
					while (enumerator4.MoveNext())
					{
					}
				}
			}
		}
		else
		{
			foreach (Entity allEntityDatum in Game.Instance.State.PlayerState.CrossSceneState.AllEntityData)
			{
				if (allEntityDatum?.View?.GO == null)
				{
					allEntityDatum.AttachToViewOnLoad(null);
					Game.Instance.CrossSceneRoot.Add(allEntityDatum.View.ViewTransform);
					Logger.Warning("Cross-scene entity {0} had no view on area change. Created {1}", allEntityDatum, allEntityDatum.View);
				}
			}
		}
		yield return null;
		Game.Instance.EntitySpawner.Tick();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			allBaseUnit.CombatGroup.EnsureAndUpdateGroup();
		}
		if ((bool)enterPoint)
		{
			using (ProfileScope.New("Move Characters"))
			{
				AreaEnterPoint areaEnterPoint = AreaEnterPoint.FindAreaEnterPointOnScene(enterPoint);
				if (areaEnterPoint != null)
				{
					Game.Instance.Player.MoveCharacters(areaEnterPoint, moveFollowers: false, area.IsPartyArea || area.IsShipArea);
				}
			}
		}
		yield return null;
		using (CodeTimer.New(Logger, "Destroy Unattached Views"))
		{
			using (ProfileScope.New("Destroy Unattached Views"))
			{
				EntityViewBase[] array3 = UnityEngine.Object.FindObjectsOfType<EntityViewBase>();
				foreach (EntityViewBase entityViewBase in array3)
				{
					if (!entityViewBase.IsInState)
					{
						if (TraceLoading)
						{
							PFLog.SceneLoader.Log("Destroying extra view " + entityViewBase.name + " #" + entityViewBase.UniqueId);
						}
						entityViewBase.DestroyViewObject();
					}
				}
			}
		}
		using (CodeTimer.New(Logger, "Turn On"))
		{
			using (ProfileScope.New("Turn On"))
			{
				state.Subscribe();
			}
		}
		yield return null;
		if (shouldReloadUiScene)
		{
			IEnumerator reloadUIScene = ReloadUIScene(area);
			while (reloadUIScene.MoveNext())
			{
				yield return null;
			}
			yield return null;
		}
		using (ProfileScope.New("Finish Loading State"))
		{
			CurrentlyLoadedTimeOfDay = Game.Instance.TimeOfDay;
		}
		using (ProfileScope.New("AreaParts Bounds Setup"))
		{
			foreach (BlueprintAreaPart item4 in area.GetParts().Concat(area))
			{
				if (item4.Bounds == null)
				{
					continue;
				}
				foreach (FogOfWarArea fowArea in FogOfWarArea.All)
				{
					if (item4.GetAllScenes(Application.isConsolePlatform).Any((SceneReference sceneRef) => sceneRef.SceneName == fowArea.gameObject.scene.name))
					{
						SetFogOfWarParameters(fowArea, item4.Bounds);
					}
				}
			}
		}
		asyncLoader = SetupFogOfWar(stash: false);
		while (!asyncLoader.IsCompleted)
		{
			yield return null;
		}
		asyncLoader.Wait();
		if (area.CameraRotationOnEnter)
		{
			CameraRig.Instance.RotateToImmediately(area.DefaultCameraRotation);
		}
	}

	public IEnumerator<object> LoadMainMenuCoroutine(Exception exception = null)
	{
		PFLog.System.Log("Returning to main menu");
		if (PhotonManager.Instance != null)
		{
			PhotonManager.Instance.StopPlaying("LoadMainMenuCoroutine");
		}
		RootUIContext rootUiContext = Game.Instance.RootUiContext;
		if (rootUiContext != null)
		{
			rootUiContext.CommonVM?.DisposeSaveLoad();
			rootUiContext.CommonVM?.EscMenuContextVM?.DisposeEscMenu();
		}
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
		SoundState.Instance.ResetBeforeUnloading();
		Task op;
		if (CurrentlyLoadedArea != null)
		{
			List<string> bankNames = CurrentlyLoadedArea.GetActiveSoundBankNames().ToList();
			bankNames.AddRange(CurrentlyLoadedArea.GetActiveSoundBankNames(isCurrentPart: true));
			float banksUnloadDelay = CurrentlyLoadedArea.UnloadBanksDelay;
			op = UnloadAreaCoroutine(forDispose: true);
			while (op.MoveNext())
			{
				yield return null;
			}
			op = UnloadEntitiesCoroutine(unloadCrossScene: true);
			while (op.MoveNext())
			{
				yield return null;
			}
			SoundBanksManager.MarkBanksToUnload(bankNames, banksUnloadDelay);
			op = Game.UnloadUnusedAssetsCoroutine();
			while (op.MoveNext())
			{
				yield return null;
			}
		}
		SoundBanksManager.UnloadNotUsed();
		Task resetTask = Game.Reset();
		while (!resetTask.IsCompleted)
		{
			yield return null;
		}
		resetTask.Wait();
		BugReportCanvas bugReportCanvas = UnityEngine.Object.FindObjectOfType<BugReportCanvas>();
		if ((bool)bugReportCanvas)
		{
			bugReportCanvas.BindKeyboard(Game.Instance.Keyboard);
		}
		CrushDumpMessage.Exception = exception;
		PFLog.System.Log("Returning to main menu: game reset");
		Scene[] array = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
		Scene[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Scene scene = array2[i];
			if (!scene.isLoaded)
			{
				continue;
			}
			switch (scene.name)
			{
			case "UI_Common_Scene":
			case "IngameConsole":
			case "LoadingScreen":
				continue;
			}
			PFLog.SceneLoader.Log("Unload scene: {0}", scene.name);
			op = BundledSceneLoader.UnloadSceneAsync(scene.name);
			while (!op.IsCompleted)
			{
				yield return null;
			}
			op.Wait();
		}
		if (exception != null)
		{
			m_LoadedAreaScenes.Clear();
		}
		SoundState.Instance.ScheduleNewAreaMusic();
		Game.Instance.SceneLoader.LoadedUIScene = GameScenes.MainMenu;
		op = BundledSceneLoader.LoadSceneAsync(GameScenes.MainMenu);
		while (!op.IsCompleted)
		{
			yield return null;
		}
		op.Wait();
		using (CodeTimer.New(Logger, "Initialize UI Scene"))
		{
			Game.Instance.RootUiContext.InitializeCommonScene("UI_Common_Scene");
			IEnumerator initUiScene = Game.Instance.RootUiContext.InitializeUiSceneCoroutine(GameScenes.MainMenu);
			while (initUiScene.MoveNext())
			{
				yield return null;
			}
		}
		PFLog.System.Log("Returning to main menu: game done");
	}

	private static void ClearCachedData()
	{
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			allUnit.Health.LastHandledDamage = null;
		}
		EntityReferenceTracker.DropCached();
		WeaponStatsHelper.ForceInvalidateCache();
		CalculateDamageCache.ForceInvalidateCache();
		Game.Instance.CustomGridNodeController.Clear();
		EntityDataLink.ClearCache();
	}
}
