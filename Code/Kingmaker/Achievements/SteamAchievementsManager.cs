using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Stores;
using Steamworks;
using UnityEngine;

namespace Kingmaker.Achievements;

public class SteamAchievementsManager : MonoBehaviour, IPlatformAchievementHandler, IDisposable
{
	private int m_CurrentVersion;

	private int m_UpdatedVersion;

	private AchievementsManager m_Achievements;

	private CGameID m_GameId;

	private bool m_RequestedStats;

	private bool m_StatsValid;

	private Callback<UserStatsReceived_t> m_UserStatsReceived;

	private Callback<UserStatsStored_t> m_UserStatsStored;

	private Callback<UserAchievementStored_t> m_UserAchievementStored;

	public static SteamAchievementsManager Instance { get; private set; }

	public AchievementsManager Achievements
	{
		get
		{
			return m_Achievements;
		}
		set
		{
			m_CurrentVersion = 0;
			m_UpdatedVersion = -1;
			m_Achievements = value;
		}
	}

	private static string ProgressStat(string SteamId)
	{
		return SteamId + "_Progress";
	}

	[UsedImplicitly]
	private void Awake()
	{
		Instance = this;
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		if (StoreManager.Store == StoreType.Steam && SteamManager.Initialized)
		{
			m_GameId = new CGameID(SteamUtils.GetAppID());
			m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			m_UserStatsStored = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			m_UserAchievementStored = Callback<UserAchievementStored_t>.Create(OnAchievementStored);
			m_RequestedStats = false;
			m_StatsValid = false;
		}
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		m_UserStatsReceived?.Dispose();
		m_UserStatsStored?.Dispose();
		m_UserAchievementStored?.Dispose();
	}

	[UsedImplicitly]
	private void Update()
	{
		if (StoreManager.Store != StoreType.Steam || !SteamManager.Initialized || m_CurrentVersion == m_UpdatedVersion)
		{
			return;
		}
		m_UpdatedVersion = m_CurrentVersion;
		if (!m_RequestedStats)
		{
			if (!SteamManager.Initialized)
			{
				m_RequestedStats = true;
				return;
			}
			m_RequestedStats = SteamUserStats.RequestCurrentStats();
		}
		if (!m_StatsValid)
		{
			return;
		}
		bool flag = false;
		foreach (AchievementEntity achievement in Achievements)
		{
			if (achievement.NeedCommit)
			{
				if (achievement.Data.EventsCountForUnlock > 1)
				{
					flag |= SteamUserStats.SetStat(ProgressStat(achievement.Data.SteamId), achievement.Counter);
				}
				if (achievement.IsUnlocked)
				{
					flag |= SteamUserStats.SetAchievement(achievement.Data.SteamId);
				}
			}
		}
		if (flag)
		{
			SteamUserStats.StoreStats();
		}
	}

	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (StoreManager.Store != StoreType.Steam || !SteamManager.Initialized || (ulong)m_GameId != pCallback.m_nGameID)
		{
			return;
		}
		if (EResult.k_EResultOK == pCallback.m_eResult)
		{
			Debug.Log("Received stats and achievements from Steam");
			m_StatsValid = true;
			{
				foreach (AchievementEntity achievement in Achievements)
				{
					if (SteamUserStats.GetAchievement(achievement.Data.SteamId, out var pbAchieved))
					{
						achievement.OnSynchronized(pbAchieved);
						if (!achievement.IsUnlocked && achievement.HasCounter && SteamUserStats.GetStat(ProgressStat(achievement.Data.SteamId), out int pData))
						{
							Debug.Log($"SteamUserStats.GetStats for {achievement.Data.SteamId}: {pData}");
							achievement.SynchronizeCounter(pData);
						}
					}
					else
					{
						Debug.LogWarning("SteamUserStats.GetAchievement failed for Achievement " + achievement.Data.SteamId);
					}
				}
				return;
			}
		}
		Debug.Log("RequestStats - failed, " + pCallback.m_eResult);
	}

	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)m_GameId == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - success");
			}
			else if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - some failed to validate");
				UserStatsReceived_t userStatsReceived_t = default(UserStatsReceived_t);
				userStatsReceived_t.m_eResult = EResult.k_EResultOK;
				userStatsReceived_t.m_nGameID = (ulong)m_GameId;
				UserStatsReceived_t pCallback2 = userStatsReceived_t;
				OnUserStatsReceived(pCallback2);
			}
			else
			{
				Debug.Log("StoreStats - failed, " + pCallback.m_eResult);
			}
		}
	}

	private void OnAchievementStored(UserAchievementStored_t pCallback)
	{
		if ((ulong)m_GameId == pCallback.m_nGameID)
		{
			Achievements.FirstOrDefault((AchievementEntity a) => a.Data.SteamId == pCallback.m_rgchAchievementName)?.OnCommited();
		}
	}

	public void SyncAchievements()
	{
		if (StoreManager.Store == StoreType.Steam && SteamManager.Initialized)
		{
			m_RequestedStats = SteamUserStats.RequestCurrentStats();
		}
	}

	public void Dispose()
	{
	}

	public void OnAchievementUnlocked(AchievementEntity achievementEntity)
	{
		m_CurrentVersion++;
	}

	public void OnAchievementProgressUpdated(AchievementEntity achievementEntity)
	{
		m_CurrentVersion++;
	}
}
