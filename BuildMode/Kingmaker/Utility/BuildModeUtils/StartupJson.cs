using System;

namespace Kingmaker.Utility.BuildModeUtils;

[Serializable]
public class StartupJson
{
	[Serializable]
	public class LoadingResearchSettings
	{
		public bool IgnoreUI;

		public bool DestroyUIPrefabs;

		public bool IgnoreSpawners;

		public bool IgnoreStatic;

		public bool IgnoreLight;

		public bool IgnoreParts;

		public bool LogSceneLoading;

		public bool UnloadAllOnEnter;

		public bool UnloadUnitsWhenSpawning;

		public bool UnloadSceneBundles;

		public bool WidgetStashCleanup;

		public bool DisableLowMemoryOverrides;
	}

	public bool playtest;

	public bool development;

	public bool showDevComment;

	public bool stackTraceSpamDetectionDisabled;

	public string analyticsUrl;

	public bool forceAnalyticsInEditor;

	public bool forceSpamDetectionInEditor;

	public string address;

	public string logsDir;

	public bool logsExtended;

	public bool hideConsoleIcon;

	public bool EnableCheats;

	public bool EnableCheatStore;

	public bool ForceLogging;

	public LoadingResearchSettings Loading = new LoadingResearchSettings();

	public bool DisableProfileScope;

	public bool DisablePreload;

	public bool DisableUnitOptimization;

	public bool WaitForDebugger;

	public bool ThrowOnBundleErrors = true;

	public bool UsePackedLocalization;

	public bool NoCommonDeps;

	public bool UseLightController;

	public string ForceControllerMode;

	public float ThreadsCountRatio = 1f;

	public string OverrideBundlesPath;

	public bool ForceGCOnSaves;

	public bool DisableFogOfWarScissorRect;

	public bool DisableAchievements;

	public int MaxFxPoolsCount = 1000;

	public bool DisableShadowsOnDynamicObjects;

	public bool NoSoundsLoading;

	public int ForceTextureMipLevel;

	public bool EnableTextureQualityLoweringToReduceMemoryUsage;

	public bool EnableExtraOptimizations;

	public bool CompensateCharacterAtlasMips;

	public bool ShowSwitchController;

	public bool CloudSwitchSettings;

	public bool ShowPhotoModeOption;

	public bool ForceDisableAutosave;

	public int PS4Restriction = 8;

	public bool DisableWeatherVFX;

	public string OverrideDisplayedGameVersion;

	public bool LimitDeltaTimeForProfiling;

	public string ForceSkipLogChannels;

	public string AppIdRealtime;

	public bool IncludeAllDesyncs;

	public bool HoldToSkipCutscene;

	public int ClientInstanceId;

	public bool ReplaySaveState;
}
