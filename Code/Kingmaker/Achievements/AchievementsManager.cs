using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints.Root;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Achievements;

[JsonObject]
public class AchievementsManager : IReadOnlyCollection<AchievementEntity>, IEnumerable<AchievementEntity>, IEnumerable, IHashable
{
	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("AchievementsManager");

	[JsonProperty]
	private readonly List<AchievementEntity> m_Achievements = new List<AchievementEntity>();

	private readonly List<AchievementLogic> m_Logics = new List<AchievementLogic>();

	private readonly List<IPlatformAchievementHandler> m_AchievementHandlers = new List<IPlatformAchievementHandler>();

	public int Count => m_Achievements.Count;

	[Cheat(Name = "list_achievements")]
	public static void ListAchievements(string param = "")
	{
		foreach (AchievementEntity achievement in Game.Instance.Player.Achievements.m_Achievements)
		{
			if (param != "locked" && achievement.IsUnlocked)
			{
				PFLog.SmartConsole.Log($"{achievement} [unlocked]");
			}
			if (param != "unlocked" && !achievement.IsUnlocked)
			{
				PFLog.SmartConsole.Log($"{achievement}");
			}
		}
	}

	[Cheat(Name = "add_achievement")]
	public static void AddAchievement(string param = "")
	{
		Game.Instance.Player.Achievements.m_Achievements.Where((AchievementEntity ach) => ach.Data.name == param).Single().Unlock();
	}

	public void Activate()
	{
		if (NetworkingManager.IsMultiplayer && !NetworkingManager.IsGameOwner)
		{
			m_Achievements.Clear();
		}
		foreach (AchievementData achData in BlueprintRoot.Instance.Achievements.List)
		{
			if (!m_Achievements.HasItem((AchievementEntity a) => a.Data == achData))
			{
				m_Achievements.Add(new AchievementEntity(achData));
			}
		}
		foreach (AchievementEntity ach in m_Achievements)
		{
			ach.Manager = this;
			if (!ach.IsUnlocked && ach.Data.IsFlagsUnlocked)
			{
				ach.Unlock();
			}
			if (!ach.IsUnlocked && !m_Logics.HasItem((AchievementLogic l) => l.Entity == ach))
			{
				AchievementLogic achievementLogic = AchievementLogicFactory.Create(ach);
				if (achievementLogic != null)
				{
					m_Logics.Add(achievementLogic);
				}
			}
		}
		m_Logics.ForEach(delegate(AchievementLogic l)
		{
			l.Activate();
		});
		if (Application.isPlaying)
		{
			SteamAchievementsManager steamAchievementsManager = SteamAchievementsManager.Instance;
			if (!steamAchievementsManager)
			{
				steamAchievementsManager = new GameObject().AddComponent<SteamAchievementsManager>();
				UnityEngine.Object.DontDestroyOnLoad(steamAchievementsManager);
			}
			steamAchievementsManager.Achievements = this;
			steamAchievementsManager.SyncAchievements();
			if (!m_AchievementHandlers.Contains(steamAchievementsManager))
			{
				m_AchievementHandlers.Add(steamAchievementsManager);
			}
			GogAchievementsManager gogAchievementsManager = GogAchievementsManager.Instance;
			if (!gogAchievementsManager)
			{
				gogAchievementsManager = new GameObject().AddComponent<GogAchievementsManager>();
				UnityEngine.Object.DontDestroyOnLoad(gogAchievementsManager);
			}
			gogAchievementsManager.Achievements = this;
			gogAchievementsManager.SyncAchievements();
			EGSAchievementsManager eGSAchievementsManager = new EGSAchievementsManager(this);
			eGSAchievementsManager.SyncAchievements();
			if (!m_AchievementHandlers.Contains(eGSAchievementsManager))
			{
				m_AchievementHandlers.Add(eGSAchievementsManager);
			}
		}
	}

	public void Deactivate()
	{
		m_Logics.ForEach(delegate(AchievementLogic l)
		{
			l.Deactivate();
		});
		foreach (IPlatformAchievementHandler achievementHandler in m_AchievementHandlers)
		{
			try
			{
				achievementHandler.Dispose();
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}
		m_AchievementHandlers.Clear();
	}

	public void OnAchievementUnlocked(AchievementEntity ach)
	{
		string message = $"Achievement unlocked: {ach}";
		Logger.Log(message);
		if (BuildModeUtility.IsDevelopment)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(message);
			});
		}
		m_Logics.RemoveAll((AchievementLogic l) => l.Entity == ach);
		foreach (IPlatformAchievementHandler achievementHandler in m_AchievementHandlers)
		{
			try
			{
				achievementHandler?.OnAchievementUnlocked(ach);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}
	}

	public void OnAchievementProgressUpdated(AchievementEntity ach)
	{
		string message = $"Achievement progress updated: {ach}";
		Logger.Log(message);
		if (BuildModeUtility.IsDevelopment)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(message);
			});
		}
		foreach (IPlatformAchievementHandler achievementHandler in m_AchievementHandlers)
		{
			try
			{
				achievementHandler?.OnAchievementProgressUpdated(ach);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}
	}

	public void Unlock(AchievementData achData)
	{
		if (achData.Type == AchievementType.Flags)
		{
			Logger.Error("Can't manually unlock achievement with Type == Flags");
			return;
		}
		m_Achievements.FirstItem((AchievementEntity a) => a.Data == achData)?.Unlock();
	}

	public void IncrementCounter(AchievementData achData)
	{
		if (achData.Type == AchievementType.Flags)
		{
			Logger.Error("Can't manually increment counter of achievement with Type == Flags");
			return;
		}
		m_Achievements.FirstItem((AchievementEntity a) => a.Data == achData)?.IncrementCounter();
	}

	public void Unlock(AchievementType type)
	{
		if (type == AchievementType.Flags)
		{
			Logger.Error("Can't manually unlock achievement with Type == Flags");
			return;
		}
		m_Achievements.FirstItem((AchievementEntity a) => a.Data.Type == type)?.Unlock();
	}

	public void IncrementCounter(AchievementType type)
	{
		if (type == AchievementType.Flags)
		{
			Logger.Error("Can't manually increment counter of achievement with Type == Flags");
			return;
		}
		m_Achievements.FirstItem((AchievementEntity a) => a.Data.Type == type)?.IncrementCounter();
	}

	public IEnumerator<AchievementEntity> GetEnumerator()
	{
		return m_Achievements.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool IsDifficultyDisableSomeAchievements(DifficultyPresetAsset newDifficulty)
	{
		DifficultyPreset minDifficulty = Game.Instance.Player.MinDifficultyController.MinDifficulty;
		if (minDifficulty == null || minDifficulty.CompareTo(newDifficulty.Preset) <= 0)
		{
			return false;
		}
		return m_Logics.HasItem((AchievementLogic l) => l.IsDifficultyDisableAchievement(newDifficulty));
	}

	public bool IsAchievementUnlocked(AchievementData achData)
	{
		if (achData != null)
		{
			return m_Achievements.FirstItem((AchievementEntity a) => a.Data == achData)?.IsUnlocked ?? false;
		}
		return false;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<AchievementEntity> achievements = m_Achievements;
		if (achievements != null)
		{
			for (int i = 0; i < achievements.Count; i++)
			{
				Hash128 val = ClassHasher<AchievementEntity>.GetHash128(achievements[i]);
				result.Append(ref val);
			}
		}
		return result;
	}
}
