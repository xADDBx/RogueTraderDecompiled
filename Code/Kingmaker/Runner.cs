using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Code.GameCore.Modding;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.BundlesLoading;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.Scenes;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.QA.Analytics;
using Kingmaker.QA.Arbiter;
using Kingmaker.QA.Arbiter.Profiling;
using Kingmaker.QA.Overlays;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Core.Overlays;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kingmaker;

public class Runner : MonoBehaviour
{
	private static Runner s_ActiveRunner;

	public static bool SkipInit;

	public static bool ShouldStartManually;

	private bool m_IsStarted;

	private RecorderAdapter m_RecorderAdapter;

	[CanBeNull]
	public static Exception LastError { get; private set; }

	public static bool IsActive => s_ActiveRunner != null;

	public static bool BasicAudioBanksLoaded { get; private set; }

	public static event Action OnBasicAudioBanksLoaded;

	private void Awake()
	{
		if (!SkipInit)
		{
			Physics.autoSimulation = false;
			OverlayService.EnsureOverlaysInitialized();
			OverlayService.Instance.RegisterOverlay(new CountersOverlay());
			OverlayService.Instance.RegisterOverlay(new NetworkingOverlay());
			if (BuildModeUtility.IsDevelopment)
			{
				OverlayService.Instance.RegisterOverlay(new WwiseMonitorOverlay());
			}
			PhotonManager.NetGame.InitPlatform();
		}
	}

	private void OnEnable()
	{
		if (s_ActiveRunner == null)
		{
			s_ActiveRunner = this;
			OwlcatAnalytics.Instance.SendStartGameSession();
			return;
		}
		PFLog.Default.Error("Previous Runner was not disabled properly! Current: '" + base.transform.GetHierarchyPath("/") + "' Previous: '" + s_ActiveRunner.transform.GetHierarchyPath("/") + "'");
	}

	private void OnDisable()
	{
		if (s_ActiveRunner == this)
		{
			s_ActiveRunner = null;
			OwlcatAnalytics.Instance.SendEndGameSession();
			return;
		}
		PFLog.Default.Error("Another Runner was activated before disabling of current! Current: '" + base.transform.GetHierarchyPath("/") + "' Previous: '" + s_ActiveRunner.transform.GetHierarchyPath("/") + "'");
	}

	private void Start()
	{
		if (!ShouldStartManually)
		{
			StartImpl();
		}
	}

	public static void StartManually()
	{
		if (s_ActiveRunner == null)
		{
			PFLog.System.Error("No active Runner to start");
		}
		else
		{
			s_ActiveRunner.StartImpl();
		}
	}

	private void StartImpl()
	{
		if (m_IsStarted)
		{
			PFLog.System.Error("Runner is already started");
			return;
		}
		try
		{
			PFLog.System.Log("GameCore logger starting");
			QAModeExceptionReporterHelper.Initialize();
			if (m_RecorderAdapter == null)
			{
				m_RecorderAdapter = new RecorderAdapter();
			}
			AstarData.RegisterTypes = GameStarter.RegisterGraphTypes;
			SuperluminalPerf.Initialize();
			if (!SkipInit)
			{
				AkAudioService.EnsureAudioInitialized();
				SettingsController.Instance.InitializeSoundController();
				Game.EnsureGameLifetimeServices();
				ModInitializer.InitializeMods();
				IResourceReplacementProvider resourceReplacementProvider = ModInitializer.ResourceReplacementProvider;
				BundledSceneLoader.SetResourceReplacementProvider(resourceReplacementProvider);
				if (Services.GetInstance<BundlesLoadService>() == null)
				{
					Services.RegisterServiceInstance(new BundlesLoadService(resourceReplacementProvider));
				}
				ResourcesLibrary.InitializeLibrary(resourceReplacementProvider);
				PFLog.System.Log("Game.Instance.Initialize();");
				Game.Instance.Initialize();
				m_IsStarted = true;
			}
		}
		catch (Exception ex)
		{
			ReportException(ex);
		}
	}

	private static async void LoadAndInitCommonScene()
	{
		_ = 1;
		try
		{
			if (!SceneManager.GetSceneByName("LoadingScreen").isLoaded)
			{
				await BundledSceneLoader.LoadSceneAsync("LoadingScreen");
				Game.Instance.RootUiContext.InitializeLoadingScreenScene("LoadingScreen");
			}
			await BundledSceneLoader.LoadSceneAsync("UI_Common_Scene");
			Game.Instance.RootUiContext.InitializeCommonScene("UI_Common_Scene");
		}
		catch (Exception ex)
		{
			PFLog.System.Error(ex);
		}
	}

	public static void EnsureBasicAudioBanks()
	{
		Runner.OnBasicAudioBanksLoaded?.Invoke();
		BasicAudioBanksLoaded = true;
	}

	[UsedImplicitly]
	private void Update()
	{
		using (Counters.Runner?.Measure())
		{
			if (m_IsStarted && LastError == null)
			{
				try
				{
					Game.Instance.Tick();
				}
				catch (Exception ex)
				{
					ReportException(ex);
				}
				TempList.Release();
				TempHashSet.Release();
				m_RecorderAdapter.AddMeasurements();
			}
		}
	}

	private void OnApplicationQuit()
	{
		Game.Instance.HandleQuit();
	}

	[StackTraceHidden]
	public static void ReportException(Exception ex)
	{
		PFLog.System.Exception(ex, "Game stopped because of a fatal error");
		if (SavesSmokeTest.IsActive)
		{
			LastError = ex;
			LoadingProcess.Instance.StopAll();
			return;
		}
		if (Arbiter.IsRunning)
		{
			ArbiterClientIntegration.HandleFatalError(ex);
			Game.Instance.ResetToMainMenu();
			return;
		}
		if (ex is LoadGameException)
		{
			ex = ex.InnerException ?? ex;
		}
		string text = ((!(ex is LoadGameException)) ? "Error: " : "Cannot load game. Are you trying to load an older save?\n");
		string text2 = text;
		if (Game.HasInstance)
		{
			if (ex is LoadingInterruptedByUserException)
			{
				Game.Instance.ResetToMainMenu();
				return;
			}
			string message = text2 + ex.Message + "\n!STACKTRACESTART!" + ex.StackTrace;
			OwlcatAnalytics.Instance.SendFatalError(message);
			Game.Instance.ResetToMainMenu(ex);
		}
	}

	private static string LimitLines(string str, int lines)
	{
		StringReader reader = new StringReader(str);
		return string.Join("\n", from _ in Enumerable.Repeat(0, lines)
			select reader.ReadLine());
	}

	public static void ClearError()
	{
		LastError = null;
	}

	public void OnApplicationFocus(bool focus)
	{
		if (!ApplicationFocusEvents.EventBusDisabled)
		{
			EventBus.RaiseEvent(delegate(IFocusHandler h)
			{
				h.OnApplicationFocusChanged(focus);
			});
		}
	}
}
