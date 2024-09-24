using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kingmaker.EOSSDK;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Achievements;

public class EGSAchievementsManager : IPlatformAchievementHandler, IDisposable
{
	private readonly IReadOnlyCollection<AchievementEntity> m_Achievements;

	private bool m_IsSynced;

	private LogChannel Logger => EpicGamesManager.Log;

	private EGSAchievementsHelper m_AchievementsHelper => EpicGamesManager.AchievementsHelper;

	private bool m_IsEGSAchievementsHelperReady => m_AchievementsHelper?.IsReady ?? false;

	public EGSAchievementsManager(IReadOnlyCollection<AchievementEntity> achievementEntities)
	{
		m_Achievements = achievementEntities;
	}

	private static string ProgressStat(string EgsId)
	{
		return EgsId + "_Progress";
	}

	public async void SyncAchievements()
	{
		Logger.Log("SyncAchievements: start");
		if (m_AchievementsHelper == null)
		{
			Logger.Log("SyncAchievements: canceled, reason: EGSAchievementHelper is not available");
			return;
		}
		while (!m_IsEGSAchievementsHelperReady)
		{
			await Task.Delay(TimeSpan.FromSeconds(1.0));
		}
		Logger.Log("SyncAchievements: EGSAchievementHelper started, sync started");
		foreach (AchievementEntity achievement in m_Achievements)
		{
			string eGSId = achievement.Data.EGSId;
			EGSAchievementsHelper.EOSPlayerAchievmentData eOSPlayerAchievmentData = m_AchievementsHelper.GetEOSPlayerAchievmentData(eGSId);
			if (eOSPlayerAchievmentData == null)
			{
				Logger.Warning("SyncAchievements: achievement " + eGSId + " not found");
				continue;
			}
			if (achievement.IsUnlocked && !eOSPlayerAchievmentData.Unlocked)
			{
				m_AchievementsHelper.AddAchievementToUnlockList(eGSId);
			}
			else if (achievement.HasCounter && achievement.Counter > eOSPlayerAchievmentData.StatCurrentValue)
			{
				m_AchievementsHelper.AddStatToUpdateList(ProgressStat(eGSId), achievement.Counter - eOSPlayerAchievmentData.StatCurrentValue);
			}
			if (!achievement.IsUnlocked && eOSPlayerAchievmentData.Unlocked)
			{
				achievement.OnSynchronized(eOSPlayerAchievmentData.Unlocked);
			}
			else if (achievement.HasCounter && achievement.Counter < eOSPlayerAchievmentData.StatCurrentValue)
			{
				achievement.SynchronizeCounter(eOSPlayerAchievmentData.StatCurrentValue);
			}
		}
		m_IsSynced = true;
		Logger.Log("SyncAchievements: sync finished");
	}

	public void Dispose()
	{
	}

	public void OnAchievementUnlocked(AchievementEntity achievementEntity)
	{
		if (m_IsSynced && m_IsEGSAchievementsHelperReady)
		{
			Logger.Log($"AddAchievementToUnlockList: {achievementEntity} ({achievementEntity.Data.EGSId})");
			m_AchievementsHelper.AddAchievementToUnlockList(achievementEntity.Data.EGSId);
		}
	}

	public void OnAchievementProgressUpdated(AchievementEntity achievementEntity)
	{
		if (m_IsSynced && m_IsEGSAchievementsHelperReady)
		{
			Logger.Log($"AddStatToUpdateList: {achievementEntity} ({ProgressStat(achievementEntity.Data.EGSId)})");
			m_AchievementsHelper.AddStatToUpdateList(ProgressStat(achievementEntity.Data.EGSId), 1);
		}
	}
}
