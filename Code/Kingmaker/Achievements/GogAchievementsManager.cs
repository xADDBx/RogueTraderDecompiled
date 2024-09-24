using System;
using System.Collections.Generic;
using Galaxy.Api;
using Kingmaker.Stores;
using Kingmaker.Utility.DotNetExtensions;
using Plugins.GOG;
using UnityEngine;

namespace Kingmaker.Achievements;

public class GogAchievementsManager : MonoBehaviour
{
	private class UserStatsAndAchievementsRetrieveListener : GlobalUserStatsAndAchievementsRetrieveListener
	{
		public override void OnUserStatsAndAchievementsRetrieveSuccess(GalaxyID userId)
		{
			foreach (AchievementEntity achievement in s_Instance.Achievements)
			{
				try
				{
					bool unlocked = false;
					uint unlockTime = 0u;
					GalaxyInstance.Stats().GetAchievement(achievement.Data.GogId, ref unlocked, ref unlockTime);
					achievement.OnSynchronized(unlocked);
					if (!achievement.IsUnlocked && achievement.HasCounter)
					{
						int statInt = GalaxyInstance.Stats().GetStatInt(ProgressStat(achievement.Data.GogId));
						Debug.Log($"SteamUserStats.GetStats for {achievement.Data.SteamId}: {statInt}");
						achievement.SynchronizeCounter(statInt);
					}
				}
				catch (Exception ex)
				{
					PFLog.Default.Error("GOG: failed GetAchievement(...) or GetStatInt(...) for " + achievement.Data.name);
					PFLog.Default.Exception(ex);
				}
			}
			s_Instance.m_Synchronized = true;
			s_Instance.m_SynchronizingNow = false;
			PFLog.Default.Log("GOG: User " + userId?.ToString() + " stats and achievements retrieved");
		}

		public override void OnUserStatsAndAchievementsRetrieveFailure(GalaxyID userId, FailureReason failureReason)
		{
			s_Instance.m_SynchronizingNow = false;
			PFLog.Default.Error("GOG: User " + userId?.ToString() + " stats and achievements could not be retrieved, for reason " + failureReason);
		}
	}

	private class StatsAndAchievementsStoreListener : GlobalStatsAndAchievementsStoreListener
	{
		public override void OnUserStatsAndAchievementsStoreSuccess()
		{
			foreach (string item in s_Instance.m_AchievementsForStore)
			{
				foreach (AchievementEntity achievement in s_Instance.Achievements)
				{
					if (achievement.Data.GogId == item)
					{
						achievement.OnCommited();
					}
				}
			}
			s_Instance.m_AchievementsForStore.Clear();
			PFLog.Default.Log("GOG: OnUserStatsAndAchievementsStoreSuccess");
		}

		public override void OnUserStatsAndAchievementsStoreFailure(FailureReason failureReason)
		{
			s_Instance.m_AchievementsForStore.Clear();
			PFLog.Default.Error("GOG: OnUserStatsAndAchievementsStoreFailure: " + failureReason);
		}
	}

	private static GogAchievementsManager s_Instance;

	private readonly List<string> m_AchievementsForStore = new List<string>();

	private bool m_Synchronized;

	private bool m_SynchronizingNow;

	private UserStatsAndAchievementsRetrieveListener m_RetriveListener;

	private StatsAndAchievementsStoreListener m_StoreListener;

	public AchievementsManager Achievements { get; set; }

	public static GogAchievementsManager Instance { get; private set; }

	private static string ProgressStat(string GogId)
	{
		return GogId + "_Progress";
	}

	private void Awake()
	{
		Instance = this;
		s_Instance = this;
		if (StoreManager.Store == StoreType.GoG)
		{
			if (!GogGalaxyManager.IsInitializedAndSignedIn())
			{
				try
				{
					GogGalaxyManager.StartManager();
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void OnEnable()
	{
		m_RetriveListener = new UserStatsAndAchievementsRetrieveListener();
		m_StoreListener = new StatsAndAchievementsStoreListener();
	}

	private void OnDisable()
	{
		m_RetriveListener?.Dispose();
		m_StoreListener?.Dispose();
	}

	private void Update()
	{
		if (!GogGalaxyManager.IsInitializedAndSignedIn())
		{
			return;
		}
		GogGalaxyManager instance = GogGalaxyManager.Instance;
		if (instance == null)
		{
			return;
		}
		if (!m_Synchronized && !m_SynchronizingNow)
		{
			GalaxyInstance.Stats().RequestUserStatsAndAchievements(instance.UserId);
			m_SynchronizingNow = true;
		}
		if (!m_Synchronized || !m_AchievementsForStore.Empty())
		{
			return;
		}
		foreach (AchievementEntity achievement in Achievements)
		{
			if (achievement.NeedCommit)
			{
				if (achievement.IsUnlocked)
				{
					GalaxyInstance.Stats().SetAchievement(achievement.Data.GogId);
					m_AchievementsForStore.Add(achievement.Data.GogId);
				}
				else if (achievement.Data.EventsCountForUnlock > 1)
				{
					GalaxyInstance.Stats().SetStatInt(ProgressStat(achievement.Data.GogId), achievement.Counter);
					m_AchievementsForStore.Add(achievement.Data.GogId);
				}
			}
		}
		if (!m_AchievementsForStore.Empty())
		{
			GalaxyInstance.Stats().StoreStatsAndAchievements();
		}
	}

	public void SyncAchievements()
	{
		if (GogGalaxyManager.IsInitializedAndSignedIn() && !m_SynchronizingNow)
		{
			GogGalaxyManager instance = GogGalaxyManager.Instance;
			if (instance != null)
			{
				GalaxyInstance.Stats().RequestUserStatsAndAchievements(instance.UserId);
				m_Synchronized = false;
				m_SynchronizingNow = true;
			}
		}
	}
}
