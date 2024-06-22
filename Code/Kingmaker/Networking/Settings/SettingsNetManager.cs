using System;
using System.Collections.Generic;
using Kingmaker.GameCommands;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Networking.Settings;

public sealed class SettingsNetManager
{
	public ISettingsEntity[] SettingsForSync;

	private BaseSettingNetData[] m_SettingsState;

	public void Init()
	{
		SettingsForSync = new ISettingsEntity[40]
		{
			SettingsRoot.Difficulty.GameDifficulty,
			SettingsRoot.Difficulty.OnlyOneSave,
			SettingsRoot.Difficulty.CombatEncountersCapacity,
			SettingsRoot.Difficulty.AutoLevelUp,
			SettingsRoot.Difficulty.RespecAllowed,
			SettingsRoot.Difficulty.AdditionalAIBehaviors,
			SettingsRoot.Difficulty.LimitedAI,
			SettingsRoot.Difficulty.EnemyDodgePercentModifier,
			SettingsRoot.Difficulty.CoverHitBonusHalfModifier,
			SettingsRoot.Difficulty.CoverHitBonusFullModifier,
			SettingsRoot.Difficulty.MinPartyDamage,
			SettingsRoot.Difficulty.MinPartyDamageFraction,
			SettingsRoot.Difficulty.MinPartyStarshipDamage,
			SettingsRoot.Difficulty.MinPartyStarshipDamageFraction,
			SettingsRoot.Difficulty.PartyMomentumPercentModifier,
			SettingsRoot.Difficulty.NPCAttributesBaseValuePercentModifier,
			SettingsRoot.Difficulty.HardCrowdControlOnPartyMaxDurationRounds,
			SettingsRoot.Difficulty.SkillCheckModifier,
			SettingsRoot.Difficulty.EnemyHitPointsPercentModifier,
			SettingsRoot.Difficulty.AllyResolveModifier,
			SettingsRoot.Difficulty.PartyDamageDealtAfterArmorReductionPercentModifier,
			SettingsRoot.Difficulty.WoundDamagePerTurnThresholdHPFraction,
			SettingsRoot.Difficulty.OldWoundDelayRounds,
			SettingsRoot.Difficulty.WoundStacksForTrauma,
			SettingsRoot.Difficulty.MinCRScaling,
			SettingsRoot.Difficulty.MaxCRScaling,
			SettingsRoot.Difficulty.SpaceCombatDifficulty,
			SettingsRoot.Game.Autopause.PauseOnLostFocus,
			SettingsRoot.Game.Autopause.PauseOnTrapDetected,
			SettingsRoot.Game.Autopause.PauseOnHiddenObjectDetected,
			SettingsRoot.Game.Autopause.PauseOnAreaLoaded,
			SettingsRoot.Game.Autopause.PauseOnLoadingScreen,
			SettingsRoot.Game.TurnBased.SpeedUpMode,
			SettingsRoot.Game.TurnBased.FastMovement,
			SettingsRoot.Game.TurnBased.FastPartyCast,
			SettingsRoot.Game.TurnBased.DisableActionCamera,
			SettingsRoot.Game.TurnBased.TimeScaleInPlayerTurn,
			SettingsRoot.Game.TurnBased.TimeScaleInNonPlayerTurn,
			SettingsRoot.Game.Main.BloodOnCharacters,
			SettingsRoot.Game.Main.DismemberCharacters
		};
		if (255 < SettingsForSync.Length)
		{
			throw new Exception($"SettingsForSync.Length={SettingsForSync.Length} > {byte.MaxValue}");
		}
		int num = SettingsForSync.Length;
		m_SettingsState = new BaseSettingNetData[num];
	}

	public BaseSettingNetData[] CollectState()
	{
		SettingsController.Instance.RevertAllTempValues();
		BaseSettingNetData[] settingsState = m_SettingsState;
		int i = 0;
		for (int num = SettingsForSync.Length; i < num; i++)
		{
			settingsState[i] = SettingsEntityToNetData(SettingsForSync[i], i);
		}
		return settingsState;
	}

	public void Sync(List<ISettingsEntity> settings)
	{
		if (Game.IsInMainMenu)
		{
			return;
		}
		List<BaseSettingNetData> list = null;
		int i = 0;
		for (int count = settings.Count; i < count; i++)
		{
			ISettingsEntity settingsEntity = settings[i];
			int num = SettingsForSync.IndexOf(settingsEntity);
			if (num != -1)
			{
				if (list == null)
				{
					list = new List<BaseSettingNetData>(settings.Count);
				}
				list.Add(SettingsEntityToNetData(settingsEntity, num));
			}
		}
		if (0 < list.TryCount())
		{
			Game.Instance.GameCommandQueue.SetSettings(list);
		}
		int num2 = settings.Count - 1;
		while (0 <= num2)
		{
			ISettingsEntity settingsEntity2 = settings[num2];
			if (SettingsForSync.IndexOf(settingsEntity2) != -1)
			{
				settingsEntity2.RevertTempValue();
				settings.Remove(settingsEntity2);
				num2 = settings.Count;
			}
			num2--;
		}
	}

	private static BaseSettingNetData SettingsEntityToNetData(ISettingsEntity settingsEntity, int index)
	{
		if (!(settingsEntity is SettingsEntityBool settingsEntityBool))
		{
			if (!(settingsEntity is SettingsEntityInt settingsEntityInt))
			{
				if (!(settingsEntity is SettingsEntityFloat settingsEntityFloat))
				{
					if (settingsEntity is ISettingsEntityEnum settingsEntityEnum)
					{
						return new EnumSettingNetData((byte)index, settingsEntityEnum.GetTempValue());
					}
					throw new Exception("Unexpected type: " + settingsEntity.GetType().Name);
				}
				return new FloatSettingNetData((byte)index, settingsEntityFloat.GetTempValue());
			}
			return new IntSettingNetData((byte)index, settingsEntityInt.GetTempValue());
		}
		return new BoolSettingNetData((byte)index, settingsEntityBool.GetTempValue());
	}

	public void OnRoomSettingsUpdate()
	{
		if (!PhotonManager.Lobby.InLobby)
		{
			return;
		}
		PFLog.Net.Log("Applying settings...");
		if (PhotonManager.Instance.GetRoomProperty<BaseSettingNetData[]>("st", out var obj))
		{
			int i = 0;
			for (int num = obj.Length; i < num; i++)
			{
				obj[i].ForceSet();
			}
			PFLog.Net.Log("All settings was applied!");
		}
		else
		{
			PFLog.Net.Error("Room settings not found!");
		}
	}
}
