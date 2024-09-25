using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Controllers;

public static class ReputationHelper
{
	public static readonly IReadOnlyCollection<FactionType> Factions = Enum.GetValues(typeof(FactionType)).Cast<FactionType>().ToList();

	private static Dictionary<FactionType, BlueprintVendorFaction> s_FactionsBlueprints;

	private static Dictionary<FactionType, int> FractionsReputation => Game.Instance.Player.FractionsReputation;

	public static Dictionary<FactionType, BlueprintVendorFaction> FactionsBlueprints => s_FactionsBlueprints ?? (s_FactionsBlueprints = Game.Instance.BlueprintRoot.SystemMechanics.VendorFactionsRoot.VendorFactions.ToDictionary((BlueprintVendorFaction.Reference x) => x.Get().FactionType, (BlueprintVendorFaction.Reference x) => x.Get()));

	private static IReadOnlyList<FactionReputationSettings.FactionReputationLevel> GetReputationLevelThresholds(FactionType factionType)
	{
		if (factionType == FactionType.None || !FactionsBlueprints.TryGetValue(factionType, out var value) || !value.OverrideReputation)
		{
			return Game.Instance.BlueprintRoot.SystemMechanics.VendorFactionsRoot.DefaultFactionReputation.ReputationLevelThresholds;
		}
		return value.ReputationLevelThresholds;
	}

	public static bool ReputationPointsReached(FactionType factionType, int points)
	{
		return GetCurrentReputationPoints(factionType) >= points;
	}

	public static bool FactionReputationLevelReached(FactionType factionType, int reputationLevel)
	{
		return GetCurrentReputationLevel(factionType) >= reputationLevel;
	}

	public static int GetLastLvl(FactionType factionType)
	{
		return GetReputationLevelThresholds(factionType).Count - 1;
	}

	public static int? GetNextLvl(FactionType factionType)
	{
		if (IsLastLvl(factionType))
		{
			return null;
		}
		return GetCurrentReputationLevel(factionType) + 1;
	}

	public static bool IsLastLvl(FactionType factionType)
	{
		int currentReputationLevel = GetCurrentReputationLevel(factionType);
		int lastLvl = GetLastLvl(factionType);
		return currentReputationLevel == lastLvl;
	}

	public static FactionReputationSettings.FactionReputationLevel GetNextReputation(FactionType factionType)
	{
		int? nextLvl = GetNextLvl(factionType);
		if (nextLvl.HasValue)
		{
			return GetReputationLevelThresholds(factionType)[nextLvl.Value];
		}
		return null;
	}

	public static int GetCurrentReputationLevel(FactionType factionType)
	{
		IReadOnlyList<FactionReputationSettings.FactionReputationLevel> reputationLevelThresholds = GetReputationLevelThresholds(factionType);
		int result = 0;
		int currentReputationPoints = GetCurrentReputationPoints(factionType);
		for (int i = 0; i < reputationLevelThresholds.Count && reputationLevelThresholds[i].ReputationPointsToLevel <= currentReputationPoints; i++)
		{
			result = i;
		}
		return result;
	}

	public static bool IsMaxReputation(FactionType factionType)
	{
		return IsLastLvl(factionType);
	}

	public static int GetCountReputationLevels(FactionType factionType)
	{
		return GetReputationLevelThresholds(factionType).Count;
	}

	public static float GetProgressToNextLevel(FactionType factionType)
	{
		int currentReputationPoints = GetCurrentReputationPoints(factionType);
		int? nextLevelReputationPoints = GetNextLevelReputationPoints(factionType);
		if (nextLevelReputationPoints.HasValue)
		{
			return Math.Abs((float)currentReputationPoints - (float)nextLevelReputationPoints.Value);
		}
		return 1f;
	}

	public static int? GetNextLevelReputationPoints(FactionType factionType)
	{
		return GetNextReputation(factionType)?.ReputationPointsToLevel;
	}

	public static int GetCurrentReputationPoints(FactionType faction)
	{
		if (!FractionsReputation.TryGetValue(faction, out var value))
		{
			return 0;
		}
		return value;
	}

	public static void GainFactionReputation(FactionType faction, int reputation)
	{
		FractionsReputation[faction] += reputation;
		EventBus.RaiseEvent(delegate(IGainFactionReputationHandler l)
		{
			l.HandleGainFactionReputation(faction, reputation);
		});
	}

	[Cheat(Name = "add_reputation_to_next_level", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddReputationToNextLevel(FactionType faction)
	{
		int? nextLevelReputationPoints = GetNextLevelReputationPoints(faction);
		if (nextLevelReputationPoints.HasValue)
		{
			int currentReputationPoints = GetCurrentReputationPoints(faction);
			GainFactionReputation(faction, nextLevelReputationPoints.Value - currentReputationPoints);
		}
	}

	[Cheat(Name = "unlock_all_vendors", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void UnlockAllVendors()
	{
		UnlockVendor(FactionType.Drusians);
		UnlockVendor(FactionType.Explorators);
		UnlockVendor(FactionType.Kasballica);
		UnlockVendor(FactionType.Pirates);
		UnlockVendor(FactionType.ShipVendor);
	}

	[Cheat(Name = "unlock_vendor", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void UnlockVendor(FactionType faction)
	{
		while (!IsMaxReputation(faction))
		{
			AddReputationToNextLevel(faction);
		}
	}

	public static int GetReputationLevelByPoints(FactionType factionType, int points)
	{
		IReadOnlyList<FactionReputationSettings.FactionReputationLevel> reputationLevelThresholds = GetReputationLevelThresholds(factionType);
		for (int i = 0; i < reputationLevelThresholds.Count; i++)
		{
			if (reputationLevelThresholds[i].ReputationPointsToLevel > points)
			{
				return i - 1;
			}
		}
		return reputationLevelThresholds.Count - 1;
	}

	public static int GetReputationPointsByLevel(FactionType factionType, int level)
	{
		IReadOnlyList<FactionReputationSettings.FactionReputationLevel> reputationLevelThresholds = GetReputationLevelThresholds(factionType);
		level = Mathf.Clamp(level, 0, reputationLevelThresholds.Count - 1);
		return reputationLevelThresholds[level].ReputationPointsToLevel;
	}
}
