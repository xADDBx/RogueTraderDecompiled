using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap;

public class GlobalMapRandomGenerationState : IHashable
{
	internal class AnomalyGroupWeightsData : IHashable
	{
		[JsonProperty]
		public BlueprintAnomalyGroupSpawnSettings Group;

		[JsonProperty]
		public RandomWeightsForSave<BlueprintAnomaly.Reference> Weights;

		public override string ToString()
		{
			return $"{Group}: {Weights}";
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Group);
			result.Append(ref val);
			Hash128 val2 = ClassHasher<RandomWeightsForSave<BlueprintAnomaly.Reference>>.GetHash128(Weights);
			result.Append(ref val2);
			return result;
		}
	}

	internal class PassageDifficultyDialogsData : IHashable
	{
		[JsonProperty]
		public SectorMapPassageEntity.PassageDifficulty Difficulty;

		[JsonProperty]
		public RandomWeightsForSave<BlueprintDialogReference> Dialogs;

		public override string ToString()
		{
			return $"{Difficulty}: {Dialogs}";
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref Difficulty);
			Hash128 val = ClassHasher<RandomWeightsForSave<BlueprintDialogReference>>.GetHash128(Dialogs);
			result.Append(ref val);
			return result;
		}
	}

	internal class AnomalyFactForSkillWeightData : IHashable
	{
		[JsonProperty]
		public StatType Skill;

		[JsonProperty]
		public RandomWeightsForSave<BlueprintUnitFactReference> Weights;

		public override string ToString()
		{
			return $"{Skill}: {Weights}";
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			result.Append(ref Skill);
			Hash128 val = ClassHasher<RandomWeightsForSave<BlueprintUnitFactReference>>.GetHash128(Weights);
			result.Append(ref val);
			return result;
		}
	}

	[JsonProperty]
	private bool m_IsInitialize;

	[JsonProperty]
	private List<AnomalyGroupWeightsData> AnomaliesInGroupWeights = new List<AnomalyGroupWeightsData>();

	[JsonProperty]
	private RandomWeightsForSave<BlueprintAnomalyGroupSpawnSettings.Reference> AnomalyGroupWeights = new RandomWeightsForSave<BlueprintAnomalyGroupSpawnSettings.Reference>();

	[JsonProperty]
	private List<PassageDifficultyDialogsData> RandomEncounterDialogWeights = new List<PassageDifficultyDialogsData>();

	[JsonProperty]
	private RandomWeightsForSave<BlueprintUnitFactReference> AnomalyFactWeights = new RandomWeightsForSave<BlueprintUnitFactReference>();

	[JsonProperty]
	private List<AnomalyFactForSkillWeightData> AnomalyFactForSkillWeights = new List<AnomalyFactForSkillWeightData>();

	public void Initialize()
	{
		if (m_IsInitialize)
		{
			return;
		}
		AnomalyFactWeights = new RandomWeightsForSave<BlueprintUnitFactReference>(BlueprintWarhammerRoot.Instance.AnomaliesRoot.FactsPool);
		foreach (BlueprintSystemAnomaliesRoot.SkillToFacts skillToFacts in BlueprintWarhammerRoot.Instance.AnomaliesRoot.SkillToFactsList)
		{
			AnomalyFactForSkillWeights.Add(new AnomalyFactForSkillWeightData
			{
				Skill = skillToFacts.Skill,
				Weights = new RandomWeightsForSave<BlueprintUnitFactReference>(skillToFacts.Facts)
			});
		}
		BlueprintWarpRoutesSettings.DifficultySettings[] difficultySettingsList = BlueprintWarhammerRoot.Instance.WarpRoutesSettings.DifficultySettingsList;
		foreach (BlueprintWarpRoutesSettings.DifficultySettings difficultySettings in difficultySettingsList)
		{
			RandomEncounterDialogWeights.Add(new PassageDifficultyDialogsData
			{
				Difficulty = difficultySettings.Difficulty,
				Dialogs = new RandomWeightsForSave<BlueprintDialogReference>(difficultySettings.RandomEncounters)
			});
		}
		AnomalyGroupWeights = new RandomWeightsForSave<BlueprintAnomalyGroupSpawnSettings.Reference>(BlueprintWarhammerRoot.Instance.AnomaliesRoot.AnomalyGroupSpawns);
		foreach (BlueprintAnomalyGroupSpawnSettings.Reference item in AnomalyGroupWeights.Keys())
		{
			AnomaliesInGroupWeights.Add(new AnomalyGroupWeightsData
			{
				Group = item,
				Weights = new RandomWeightsForSave<BlueprintAnomaly.Reference>(item.Get().AnomalySpawnInGroupWeight)
			});
		}
		m_IsInitialize = true;
	}

	public RandomWeightsForSave<BlueprintUnitFactReference> GetBuffsForSkill(StatType skill)
	{
		return AnomalyFactForSkillWeights.FirstOrDefault((AnomalyFactForSkillWeightData s) => s.Skill == skill).Weights ?? AnomalyFactWeights;
	}

	public RandomWeightsForSave<BlueprintDialogReference> GetRandomEncountersDialogs(SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		return RandomEncounterDialogWeights.FirstOrDefault((PassageDifficultyDialogsData re) => re.Difficulty == difficulty).Dialogs;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_IsInitialize);
		List<AnomalyGroupWeightsData> anomaliesInGroupWeights = AnomaliesInGroupWeights;
		if (anomaliesInGroupWeights != null)
		{
			for (int i = 0; i < anomaliesInGroupWeights.Count; i++)
			{
				Hash128 val = ClassHasher<AnomalyGroupWeightsData>.GetHash128(anomaliesInGroupWeights[i]);
				result.Append(ref val);
			}
		}
		Hash128 val2 = ClassHasher<RandomWeightsForSave<BlueprintAnomalyGroupSpawnSettings.Reference>>.GetHash128(AnomalyGroupWeights);
		result.Append(ref val2);
		List<PassageDifficultyDialogsData> randomEncounterDialogWeights = RandomEncounterDialogWeights;
		if (randomEncounterDialogWeights != null)
		{
			for (int j = 0; j < randomEncounterDialogWeights.Count; j++)
			{
				Hash128 val3 = ClassHasher<PassageDifficultyDialogsData>.GetHash128(randomEncounterDialogWeights[j]);
				result.Append(ref val3);
			}
		}
		Hash128 val4 = ClassHasher<RandomWeightsForSave<BlueprintUnitFactReference>>.GetHash128(AnomalyFactWeights);
		result.Append(ref val4);
		List<AnomalyFactForSkillWeightData> anomalyFactForSkillWeights = AnomalyFactForSkillWeights;
		if (anomalyFactForSkillWeights != null)
		{
			for (int k = 0; k < anomalyFactForSkillWeights.Count; k++)
			{
				Hash128 val5 = ClassHasher<AnomalyFactForSkillWeightData>.GetHash128(anomalyFactForSkillWeights[k]);
				result.Append(ref val5);
			}
		}
		return result;
	}
}
