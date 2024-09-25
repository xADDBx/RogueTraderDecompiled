using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Settings.Difficulty;

public class MinDifficultyController : IHashable
{
	[JsonProperty]
	public DifficultyPreset MinDifficulty { get; private set; }

	public void UpdateMinDifficulty(bool force = false)
	{
		DifficultyPreset difficultyPreset = SettingsController.Instance.DifficultySettingsController.ExtractFromSettings();
		if (force || MinDifficulty == null || difficultyPreset.CompareTo(MinDifficulty) < 0)
		{
			MinDifficulty = difficultyPreset;
		}
	}

	public void PostLoad()
	{
		UpdateMinDifficulty();
	}

	public void PreSave()
	{
		UpdateMinDifficulty();
	}

	public void ResetMinDifficulty()
	{
		MinDifficulty = SettingsController.Instance.DifficultySettingsController.ExtractFromSettings();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<DifficultyPreset>.GetHash128(MinDifficulty);
		result.Append(ref val);
		return result;
	}
}
