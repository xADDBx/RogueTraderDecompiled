using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Settings;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.Stores;
using Kingmaker.UnitLogic.Visual.Blueprints;
using Kingmaker.Utility.FlagCountable;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class MusicStateHandler
{
	public enum MusicSettingState
	{
		Combat,
		Exploration,
		Death
	}

	public enum MusicState
	{
		Chargen,
		MainMenu,
		Credits,
		BossFight,
		Story,
		Setting,
		CoopLobby
	}

	public static readonly string MainMusicEventStart = "Music_Play";

	public static readonly string MainMusicEventStop = "Music_Stop";

	private readonly GameObject m_MusicPlayerObject;

	private UnitVisualSettings.MusicCombatState m_CombatState;

	private MusicSettingState m_CurrentMusicSettingState;

	private List<string> m_OverridedStates = new List<string>();

	private bool m_StoryModeActive;

	private bool m_ProlongTillNextCombat;

	private bool m_EventStarted;

	private bool m_OverrideSettingState;

	private AkStateReference m_OverrideMusicSetting;

	private CountableFlag m_ActiveBossFight = new CountableFlag();

	private CountableFlag m_ActiveHardUnit = new CountableFlag();

	public MusicStateHandler()
	{
		m_MusicPlayerObject = new GameObject("[Music Player]", typeof(AudioObject));
		m_MusicPlayerObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(m_MusicPlayerObject);
		}
	}

	public void SetDefaultState(bool setDefaultMusicState = true)
	{
		SoundEventsManager.PostEvent(MainMusicEventStart, m_MusicPlayerObject);
		m_EventStarted = true;
		m_ActiveBossFight.ReleaseAll();
		m_ActiveHardUnit.ReleaseAll();
		if (setDefaultMusicState)
		{
			SetMusicSettingState(MusicSettingState.Exploration);
			SetMusicState(MusicState.MainMenu);
		}
	}

	public void HandleUpdateArea()
	{
		if (!IsOverrideAvailable("MusicSettingType", isMainMenuState: false))
		{
			return;
		}
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintAreaPart blueprintAreaPart = SimpleBlueprintExtendAsObject.Or(Game.Instance.CurrentlyLoadedAreaPart, currentlyLoadedArea);
		if (currentlyLoadedArea != null)
		{
			if (m_OverrideSettingState)
			{
				AkSoundEngine.SetState("MusicSettingType", m_OverrideMusicSetting.Value);
			}
			else if (blueprintAreaPart != null && blueprintAreaPart.MusicSetting != null)
			{
				AkSoundEngine.SetState("MusicSettingType", blueprintAreaPart.MusicSetting.Value);
			}
			else if (currentlyLoadedArea.MusicSetting != null)
			{
				AkSoundEngine.SetState("MusicSettingType", currentlyLoadedArea.MusicSetting.Value);
			}
			AkSoundEngine.SetState("MusicSettingState", ((!Game.Instance.Player.IsInCombat) ? MusicSettingState.Exploration : MusicSettingState.Combat).ToString());
			if (Game.Instance.Player.IsInCombat)
			{
				foreach (BaseUnitEntity item in Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity unit) => unit.IsInCombat))
				{
					OnEnemyJoinCombat(item);
				}
			}
			AkSoundEngine.SetState("MusicState", (m_ActiveBossFight ? MusicState.BossFight : MusicState.Setting).ToString());
			AkSoundEngine.SetState("MusicStoryType", "None");
			m_StoryModeActive = false;
			m_ProlongTillNextCombat = false;
			m_OverridedStates.Clear();
		}
		else
		{
			SetMusicState(MusicState.MainMenu);
			SetMusicSettingState(MusicSettingState.Exploration);
			AkSoundEngine.SetState("MusicStoryType", "None");
		}
	}

	public void SetMusicCombatState(UnitVisualSettings.MusicCombatState state)
	{
		if (IsOverrideAvailable("MusicCombatState", isMainMenuState: false))
		{
			m_CombatState = state;
			AkSoundEngine.SetState("MusicCombatState", m_CombatState.ToString());
		}
	}

	public void OnEnemyJoinCombat(MechanicEntity unit)
	{
		UnitVisualSettings.MusicCombatState? combatMusic = unit.GetCombatMusic();
		if (combatMusic.HasValue && m_CombatState < combatMusic.Value)
		{
			SetMusicCombatState(combatMusic.Value);
			if (combatMusic == UnitVisualSettings.MusicCombatState.Hard)
			{
				m_ActiveHardUnit.Retain();
			}
		}
		if (unit is UnitEntity { MusicBossFightTypeGroup: not null, MusicBossFightTypeGroup: not null } unitEntity)
		{
			m_ActiveBossFight.Retain();
			if (IsOverrideAvailable(unitEntity.MusicBossFightTypeGroup, isMainMenuState: false))
			{
				SetMusicState(MusicState.BossFight);
				AkSoundEngine.SetState(unitEntity.MusicBossFightTypeGroup, unitEntity.MusicBossFightTypeValue);
			}
		}
	}

	public void OnEnemyLeaveCombat(MechanicEntity unit)
	{
		UnitVisualSettings.MusicCombatState? combatMusic = unit.GetCombatMusic();
		if (combatMusic.HasValue && combatMusic == UnitVisualSettings.MusicCombatState.Hard)
		{
			m_ActiveHardUnit.Release();
		}
		if (!m_ActiveHardUnit)
		{
			SetMusicCombatState(UnitVisualSettings.MusicCombatState.Normal);
		}
		if (unit is UnitEntity { MusicBossFightTypeGroup: not null, MusicBossFightTypeGroup: not null, IsDead: not false })
		{
			m_ActiveBossFight.Release();
		}
	}

	public void HandlePartyCombatStateChange(bool isCombatStarted)
	{
		if (m_ProlongTillNextCombat && m_OverridedStates != null)
		{
			m_ProlongTillNextCombat = false;
			m_OverridedStates.Clear();
			HandleUpdateArea();
		}
		if (!isCombatStarted || (isCombatStarted && m_CombatState == UnitVisualSettings.MusicCombatState.Normal))
		{
			SetMusicCombatState(UnitVisualSettings.MusicCombatState.Normal);
		}
		if (!isCombatStarted)
		{
			m_ActiveBossFight.ReleaseAll();
			m_ActiveHardUnit.ReleaseAll();
			AkSoundEngine.SetState("MusicState", MusicState.Setting.ToString());
			AkSoundEngine.SetState("MusicCombatState", UnitVisualSettings.MusicCombatState.Normal.ToString());
		}
	}

	public void OnChargenChange(bool chargen)
	{
		SetMusicState((!chargen) ? MusicState.MainMenu : MusicState.Chargen);
	}

	public void OnMusicStateChange(MusicState state)
	{
		SetMusicState(state);
	}

	public void SetMusicSettingState(MusicSettingState state)
	{
		if (IsOverrideAvailable("MusicSettingState", isMainMenuState: false))
		{
			AkSoundEngine.SetState("MusicSettingState", state.ToString());
			if (state != m_CurrentMusicSettingState && m_CurrentMusicSettingState == MusicSettingState.Combat && state == MusicSettingState.Exploration)
			{
				m_ActiveBossFight.ReleaseAll();
				m_ActiveHardUnit.ReleaseAll();
				AkSoundEngine.SetState("MusicState", MusicState.Setting.ToString());
				AkSoundEngine.SetState("MusicCombatState", UnitVisualSettings.MusicCombatState.Normal.ToString());
			}
			m_CurrentMusicSettingState = state;
		}
	}

	public void SetMusicState(MusicState state)
	{
		if (!m_EventStarted)
		{
			SetDefaultState();
		}
		string text = string.Empty;
		if (state == MusicState.MainMenu)
		{
			BlueprintDlc blueprintDlc = StoreManager.GetPurchasableDLCs().OfType<BlueprintDlc>().LastOrDefault();
			text = ((SettingsRoot.Game.MainMenu.MainMenuTheme.GetValue() == MainMenuTheme.Original || string.IsNullOrWhiteSpace(blueprintDlc?.MusicSetting?.Value)) ? string.Empty : blueprintDlc.MusicSetting.Value);
		}
		if (IsOverrideAvailable("MusicState", state == MusicState.MainMenu || state == MusicState.Credits))
		{
			AkSoundEngine.SetState("MusicState", (!string.IsNullOrWhiteSpace(text)) ? text : state.ToString());
		}
	}

	public void SetMusicStoryType(AkStateReference state, bool prolongTillNextCombat)
	{
		AkSoundEngine.SetState(state.Group, state.Value);
		if (state.Group == "MusicState" && state.Value != "Story")
		{
			AkSoundEngine.SetState("MusicStoryType", "None");
		}
		m_StoryModeActive = true;
		m_ProlongTillNextCombat = prolongTillNextCombat;
		if (m_OverridedStates == null)
		{
			m_OverridedStates = new List<string>();
		}
		if (state.Group == "MusicCombatState")
		{
			string value = state.Value;
			UnitVisualSettings.MusicCombatState combatState = ((value == "Hard") ? UnitVisualSettings.MusicCombatState.Hard : ((!(value == "Normal")) ? m_CombatState : UnitVisualSettings.MusicCombatState.Normal));
			m_CombatState = combatState;
		}
		m_OverridedStates.Add(state.Group);
	}

	private bool IsOverrideAvailable(string stateGroup, bool isMainMenuState)
	{
		if (isMainMenuState)
		{
			return true;
		}
		if (m_StoryModeActive || m_ProlongTillNextCombat)
		{
			return !m_OverridedStates.Contains(stateGroup);
		}
		return true;
	}

	public void ResetStoryMode()
	{
		m_StoryModeActive = false;
		if (!m_ProlongTillNextCombat && m_OverridedStates != null)
		{
			m_OverridedStates.Clear();
			SetMusicState(m_ActiveBossFight ? MusicState.BossFight : MusicState.Setting);
			HandleUpdateArea();
		}
	}

	public void StartMusicStopEvent()
	{
		SoundEventsManager.PostEvent(MainMusicEventStop, m_MusicPlayerObject);
		m_EventStarted = false;
	}

	public void OverrideAreaSetting(AkStateReference overrideMusicSetting)
	{
		if (overrideMusicSetting != null)
		{
			m_OverrideMusicSetting = overrideMusicSetting;
			m_OverrideSettingState = true;
			AkSoundEngine.SetState("MusicSettingType", overrideMusicSetting.Value);
		}
	}

	public void DisableOverrideAreaSetting()
	{
		m_OverrideMusicSetting = null;
		m_OverrideSettingState = false;
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintAreaPart blueprintAreaPart = SimpleBlueprintExtendAsObject.Or(Game.Instance.CurrentlyLoadedAreaPart, currentlyLoadedArea);
		if (currentlyLoadedArea != null)
		{
			if (blueprintAreaPart != null && blueprintAreaPart.MusicSetting != null)
			{
				AkSoundEngine.SetState("MusicSettingType", blueprintAreaPart.MusicSetting.Value);
			}
			else if (currentlyLoadedArea.MusicSetting != null)
			{
				AkSoundEngine.SetState("MusicSettingType", currentlyLoadedArea.MusicSetting.Value);
			}
		}
	}
}
