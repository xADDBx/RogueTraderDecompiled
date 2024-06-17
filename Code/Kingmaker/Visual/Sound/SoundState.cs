using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes.Commands;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Area;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Settings.Graphics;
using Kingmaker.Sound.Base;
using Kingmaker.UI;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class SoundState : IService, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IQuestHandler, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, IAreaLoadingStagesHandler, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ITimeOfDayChangedHandler, IAreaPartHandler, IPartyCombatHandler, IFullScreenUIHandler, IModalWindowUIHandler
{
	private const float MusicChangeDelay = 2f;

	private const float DefaultCameraZoom = 0f;

	private const float CameraZoomMultiplier = 100f;

	public const string GameAudioStateGroupName = "GameAudioState";

	private static ServiceProxy<SoundState> s_Proxy;

	private SoundStateType m_State;

	private static SoundState s_Instance;

	private bool m_CombatTriggered;

	private bool m_WasPaused;

	private CameraZoom m_CameraZoomComponent;

	private float m_CameraZoom;

	private float m_CurrentCameraZoom;

	[CanBeNull]
	private BlueprintArea m_ScheduledMusicArea;

	private bool m_ScheduledMainMenuMusic;

	private float m_ChangeMusicTime;

	private bool m_TurnWeatherOnNextFrame;

	private bool m_DialogPaused = true;

	private FullScreenUIType m_UIType;

	private ModalWindowUIType m_ModalWindowUIType;

	public static SoundState Instance
	{
		get
		{
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<SoundState>());
			return s_Proxy?.Instance;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public MusicStateHandler MusicStateHandler { get; } = new MusicStateHandler();


	private SoundEventsManager SoundEvents => SoundEventsManager.Instance;

	private bool MusicChangeScheduled => m_ScheduledMusicArea != null;

	public SoundState()
	{
		EventBus.Subscribe(this);
		MonoSingleton<ApplicationFocusObserver>.Instance.OnApplicationChangedFocus += OnApplicationFocusChanged;
	}

	public void Update()
	{
		SetState();
		UpdateScheduledAreaMusic();
		UpdateDialogPaused();
		UpdateCameraZoom();
		SoundEvents.Update();
	}

	private void SetState()
	{
		GameModeType currentMode = Game.Instance.CurrentMode;
		if (currentMode == GameModeType.None)
		{
			return;
		}
		SoundStateType soundStateType;
		if (CommandPlayVideo.Flag.Value)
		{
			soundStateType = SoundStateType.Video;
		}
		else if (LoadingProcess.Instance.IsLoadingInProcess || LoadingProcess.Instance.IsLoadingScreenActive)
		{
			soundStateType = SoundStateType.LoadingScreen;
		}
		else if (m_ModalWindowUIType != 0)
		{
			soundStateType = ((m_ModalWindowUIType == ModalWindowUIType.GameEndingTitles) ? SoundStateType.CreditsAfterEpilogues : SoundStateType.InGameMenu);
		}
		else if (m_UIType != 0)
		{
			soundStateType = SoundStateType.InGameMenu;
		}
		else if (!(currentMode == GameModeType.Dialog))
		{
			soundStateType = ((currentMode == GameModeType.Cutscene) ? SoundStateType.CutScene : ((currentMode == GameModeType.GlobalMap) ? SoundStateType.GlobalMap : ((currentMode == GameModeType.CutsceneGlobalMap) ? SoundStateType.GlobalMap : ((currentMode == GameModeType.GameOver) ? SoundStateType.Death : ((Game.Instance.Player.PartyAndPets.Any((BaseUnitEntity c) => c.IsInCombat) || currentMode == GameModeType.SpaceCombat) ? SoundStateType.Combat : (Game.Instance.IsPaused ? SoundStateType.InGamePause : ((!(currentMode == GameModeType.BugReport)) ? SoundStateType.Default : SoundStateType.InGameMenu)))))));
		}
		else
		{
			DialogController dialogController = Game.Instance.DialogController;
			soundStateType = ((dialogController != null && dialogController.Dialog?.Type == DialogType.Book) ? SoundStateType.BookQuest : SoundStateType.Dialog);
		}
		SoundStateType soundStateType2 = soundStateType;
		if (soundStateType2 == SoundStateType.Dialog && m_State == SoundStateType.LoadingScreen)
		{
			soundStateType2 = SoundStateType.Default;
		}
		ResetState(soundStateType2);
		if (Time.timeScale == 0f != m_WasPaused)
		{
			m_WasPaused = Time.timeScale == 0f;
			SoundEventsManager.PostEvent(m_WasPaused ? "PauseFX" : "ResumeFX", null);
		}
		if ((bool)VFXWeatherSystem.Instance)
		{
			UpdateWeather();
		}
	}

	private void UpdateWeather()
	{
		if (!LoadingProcess.Instance.IsLoadingScreenActive)
		{
			StartWeatherAmbienceIfNeeded();
		}
	}

	private void UpdateCameraZoom()
	{
		if (m_CameraZoomComponent == null)
		{
			m_CameraZoomComponent = CameraRig.Instance?.CameraZoom;
		}
		m_CameraZoom = ((m_CameraZoomComponent != null) ? m_CameraZoomComponent.CurrentNormalizePosition : 0f);
		if (!(Math.Abs(m_CameraZoom - m_CurrentCameraZoom) < 0.005f))
		{
			m_CurrentCameraZoom = m_CameraZoom;
			AkSoundEngine.SetRTPCValue("CameraZoom", m_CameraZoom * 100f);
		}
	}

	public void ResetState(SoundStateType state)
	{
		if (state != m_State)
		{
			AkSoundEngine.SetState("GameAudioState", state.ToString());
			if (state == SoundStateType.Death)
			{
				MusicStateHandler.SetMusicSettingState(MusicStateHandler.MusicSettingState.Death);
			}
			if (state == SoundStateType.Default || state == SoundStateType.Dialog || (state == SoundStateType.LoadingScreen && LoadingProcess.Instance.CurrentProcessTag != LoadingProcessTag.Save))
			{
				MusicStateHandler.SetMusicSettingState(MusicStateHandler.MusicSettingState.Exploration);
			}
			if (state == SoundStateType.Combat)
			{
				MusicStateHandler.SetMusicSettingState(MusicStateHandler.MusicSettingState.Combat);
			}
			if (state == SoundStateType.GlobalMap)
			{
				AkSoundEngine.SetRTPCValue("PartyBanterPositioning", 1f);
			}
			if (state == SoundStateType.Default)
			{
				AkSoundEngine.SetRTPCValue("PartyBanterPositioning", 0f);
			}
			if ((m_State == SoundStateType.Dialog && m_UIType != FullScreenUIType.Chargen) || m_State == SoundStateType.CutScene || state == SoundStateType.LoadingScreen)
			{
				MusicStateHandler.ResetStoryMode();
			}
			m_State = state;
			if (state == SoundStateType.MainMenu)
			{
				MusicStateHandler.SetMusicState(MusicStateHandler.MusicState.MainMenu);
			}
		}
	}

	public void ResetBeforeUnloading()
	{
		SoundEventsManager.PostEvent("ResetSoundBeforeLoading", null);
	}

	public void HandleUnitJoinCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity.IsPlayerEnemy)
		{
			MusicStateHandler.OnEnemyJoinCombat(baseUnitEntity);
		}
	}

	public void HandleUnitLeaveCombat()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity.IsPlayerEnemy)
		{
			RecalculateEnemyCombatMusic();
		}
		MusicStateHandler.OnEnemyLeaveCombat(baseUnitEntity);
	}

	public void HandleQuestStarted(Quest quest)
	{
	}

	public void HandleQuestCompleted(Quest objective)
	{
		AkSoundEngine.PostTrigger("QuestCompleted", null);
	}

	public void HandleQuestFailed(Quest objective)
	{
		AkSoundEngine.PostTrigger("QuestFailed", null);
	}

	public void HandleQuestUpdated(Quest objective)
	{
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.LifeState.State == UnitLifeState.Dead)
		{
			if (baseUnitEntity.Health.LastHandledDamage != null && baseUnitEntity.Health.LastHandledDamage.Initiator.IsPlayerFaction && !baseUnitEntity.Blueprint.VisualSettings.NoFinishingBlow && !string.IsNullOrEmpty(Game.Instance.BlueprintRoot.Sound.FinishingBlow))
			{
				SoundEventsManager.PostEvent(Game.Instance.BlueprintRoot.Sound.FinishingBlow, baseUnitEntity.View.gameObject);
			}
			if (baseUnitEntity.Faction.IsPlayerEnemy)
			{
				RecalculateEnemyCombatMusic();
			}
		}
	}

	private void RecalculateEnemyCombatMusic()
	{
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity unit) => unit.IsInCombat);
		UnitVisualSettings.MusicCombatState musicCombatState = UnitVisualSettings.MusicCombatState.Normal;
		foreach (BaseUnitEntity item in enumerable)
		{
			if (item.LifeState.State != UnitLifeState.Dead && item.View.CombatMusic > musicCombatState)
			{
				musicCombatState = item.View.CombatMusic;
			}
		}
		MusicStateHandler.SetMusicCombatState(musicCombatState);
	}

	public void OnEntityNoticed(BaseUnitEntity character)
	{
		MapObjectEntity entity = EventInvokerExtensions.GetEntity<MapObjectEntity>();
		SoundEventsManager.PostEvent("DiscoveryNotification", entity.View.gameObject);
	}

	private void OnApplicationFocusChanged(bool isFocused)
	{
		if (!ApplicationFocusEvents.SoundDisabled && !ApplicationFocusEvents.EventBusDisabled)
		{
			if (SettingsRoot.Sound.MuteAudioWhileTheGameIsOutFocus.GetValue())
			{
				SoundEventsManager.PostEvent(isFocused ? "ResumeAudio" : "PauseAudio", null);
			}
			if (isFocused && CutsceneController.Skipping)
			{
				SoundEvents.SetStoppingAllState(active: true);
			}
		}
	}

	public void OnChargenChange(bool chargen)
	{
		MusicStateHandler.OnChargenChange(chargen);
	}

	public void OnMusicStateChange(MusicStateHandler.MusicState state)
	{
		MusicStateHandler.OnMusicStateChange(state);
	}

	public void OnAreaScenesLoaded()
	{
		ScheduleNewAreaMusic();
		OnTimeOfDayChanged();
	}

	public void ScheduleNewAreaMusic()
	{
		m_ScheduledMusicArea = Game.Instance.CurrentlyLoadedArea;
		PFLog.System.Log($"Scheduled music for area: {m_ScheduledMusicArea}");
		if (m_ScheduledMusicArea == null)
		{
			m_ScheduledMainMenuMusic = true;
		}
		m_ChangeMusicTime = Time.unscaledTime + 2f;
	}

	public void OnAreaLoadingComplete()
	{
	}

	public void OnAreaPartChanged(BlueprintAreaPart previous)
	{
		if (!MusicChangeScheduled)
		{
			MusicStateHandler.HandleUpdateArea();
		}
	}

	public void UpdateScheduledAreaMusic()
	{
		if (m_ScheduledMusicArea == null && !m_ScheduledMainMenuMusic)
		{
			return;
		}
		if (m_ScheduledMusicArea != Game.Instance.CurrentlyLoadedArea)
		{
			m_ScheduledMusicArea = null;
			m_ScheduledMainMenuMusic = false;
			m_ChangeMusicTime = 0f;
		}
		else if (m_ChangeMusicTime <= Time.unscaledTime)
		{
			PFLog.System.Log($"Starting music for area: {m_ScheduledMusicArea}");
			MusicStateHandler.SetMusicSettingState(MusicStateHandler.MusicSettingState.Exploration);
			MusicStateHandler.HandleUpdateArea();
			m_ScheduledMusicArea = null;
			m_ScheduledMainMenuMusic = false;
			m_ChangeMusicTime = 0f;
			if (!LoadingProcess.Instance.IsLoadingScreenActive)
			{
				StartWeatherAmbienceIfNeeded();
			}
		}
	}

	private void StartWeatherAmbienceIfNeeded()
	{
		_ = m_TurnWeatherOnNextFrame;
	}

	public void OnLoadingScreenShown()
	{
		_ = (bool)VFXWeatherSystem.Instance;
	}

	public void OnTimeOfDayChanged()
	{
		AkSoundEngine.SetState("TimeOfDay", Game.Instance.TimeOfDay.ToString());
	}

	public static GameObject Get2DSoundObject()
	{
		GameObject gameObject = null;
		CameraRig instance = CameraRig.Instance;
		if (instance != null)
		{
			gameObject = instance.gameObject;
		}
		if (gameObject == null)
		{
			GameObject soundGameObject = UIAccess.SoundGameObject;
			if (soundGameObject != null)
			{
				gameObject = soundGameObject.gameObject;
			}
		}
		if (!(gameObject == null))
		{
			return gameObject;
		}
		return null;
	}

	public void StartCutsceneSkip()
	{
		SoundEvents.SetStoppingAllState(active: true);
	}

	public void StopCutsceneSkip()
	{
		SoundEvents.SetStoppingAllState(active: false);
	}

	public void StopDialog()
	{
		SoundEventsManager.PostEvent("StopDialog", null);
	}

	private void UpdateDialogPaused()
	{
		bool flag = m_State != SoundStateType.Dialog && m_State != SoundStateType.CutScene;
		if (m_DialogPaused != flag)
		{
			SoundEventsManager.PostEvent(flag ? "PauseDialog" : "ResumeDialog", null);
			m_DialogPaused = flag;
		}
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat)
		{
			UISounds.Instance.Sounds.Combat.CombatStart.Play();
			m_CombatTriggered = true;
			MusicStateHandler.HandlePartyCombatStateChange(isCombatStarted: true);
		}
		else if (m_CombatTriggered)
		{
			UISounds.Instance.Sounds.Combat.CombatEnd.Play();
			m_CombatTriggered = false;
			MusicStateHandler.HandlePartyCombatStateChange(isCombatStarted: false);
		}
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		m_UIType = (state ? fullScreenUIType : FullScreenUIType.Unknown);
		SetState();
	}

	public void HandleModalWindowUiChanged(bool state, ModalWindowUIType modalWindowUIType)
	{
		m_ModalWindowUIType = (state ? modalWindowUIType : ModalWindowUIType.Unknown);
		SetState();
	}
}
