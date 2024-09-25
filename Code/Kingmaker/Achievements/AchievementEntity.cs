using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Achievements;

public class AchievementEntity : IHashable
{
	[JsonProperty]
	public readonly AchievementData Data;

	[JsonProperty]
	public bool IsUnlocked { get; private set; }

	[JsonProperty]
	public bool NeedCommit { get; private set; }

	[JsonProperty]
	public int Counter { get; private set; }

	public AchievementsManager Manager { get; set; }

	public bool IsDisabled
	{
		get
		{
			if (Data.OnlyMainCampaign && (bool)Game.Instance.Player.Campaign && !Game.Instance.Player.Campaign.IsMainGameContent)
			{
				return true;
			}
			BlueprintCampaign blueprintCampaign = Data.SpecificCampaign?.Get();
			if (!Data.OnlyMainCampaign && blueprintCampaign != null && Game.Instance.Player.Campaign != blueprintCampaign)
			{
				return true;
			}
			if (Game.Instance.Player.ModsUser)
			{
				return true;
			}
			return false;
		}
	}

	public bool HasCounter => Data.EventsCountForUnlock > 1;

	[JsonConstructor]
	public AchievementEntity(AchievementData data)
	{
		Data = data;
	}

	public void OnSynchronized(bool unlocked)
	{
		IsUnlocked |= unlocked;
		if (unlocked)
		{
			NeedCommit = false;
			Manager.OnAchievementUnlocked(this);
		}
	}

	public void SynchronizeCounter(int progressValue)
	{
		if (!IsDisabled && !IsUnlocked && HasCounter)
		{
			int counter = Counter;
			Counter = Math.Max(counter, progressValue);
			if (progressValue < counter)
			{
				NeedCommit = true;
				Manager.OnAchievementProgressUpdated(this);
			}
		}
	}

	public void OnCommited()
	{
		NeedCommit = false;
	}

	public void Unlock()
	{
		if (!IsDisabled && !IsUnlocked)
		{
			IsUnlocked = true;
			NeedCommit = true;
			Manager.OnAchievementUnlocked(this);
		}
	}

	public void IncrementCounter()
	{
		if (IsDisabled || IsUnlocked)
		{
			return;
		}
		if (!HasCounter)
		{
			PFLog.Default.Error("Can't increment counter for achievement with EventsCountForUnlock < 2 (use Unlock instead)");
			return;
		}
		Counter++;
		NeedCommit = true;
		Manager.OnAchievementProgressUpdated(this);
		if (Counter >= Data.EventsCountForUnlock)
		{
			Unlock();
		}
	}

	public override string ToString()
	{
		if (!HasCounter)
		{
			return Data.name;
		}
		return $"{Data.name} ({Counter}/{Data.EventsCountForUnlock})";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Data);
		result.Append(ref val);
		bool val2 = IsUnlocked;
		result.Append(ref val2);
		bool val3 = NeedCommit;
		result.Append(ref val3);
		int val4 = Counter;
		result.Append(ref val4);
		return result;
	}
}
