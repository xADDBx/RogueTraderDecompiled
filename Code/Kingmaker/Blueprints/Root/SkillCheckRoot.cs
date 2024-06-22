using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("eb0f154cbe024b708d154713d974f2f0")]
public class SkillCheckRoot : BlueprintScriptableObject
{
	[Serializable]
	public class SkillCheckDifficultyEntry
	{
		public SkillCheckDifficulty Difficulty;

		public int[] CR2DC = new int[0];
	}

	[SerializeField]
	private DamageSkillCheckRootReference m_DamageSkillCheckRoot;

	[SerializeField]
	private DebuffSkillCheckRootReference m_DebuffSkillCheckRoot;

	public SkillCheckDifficultyEntry[] SkillCheckDifficulty = new SkillCheckDifficultyEntry[4]
	{
		new SkillCheckDifficultyEntry
		{
			Difficulty = Kingmaker.View.MapObjects.SkillCheckDifficulty.Easy
		},
		new SkillCheckDifficultyEntry
		{
			Difficulty = Kingmaker.View.MapObjects.SkillCheckDifficulty.Normal
		},
		new SkillCheckDifficultyEntry
		{
			Difficulty = Kingmaker.View.MapObjects.SkillCheckDifficulty.Hard
		},
		new SkillCheckDifficultyEntry
		{
			Difficulty = Kingmaker.View.MapObjects.SkillCheckDifficulty.Impossible
		}
	};

	public DamageSkillCheckRoot DamageSkillCheckRoot => m_DamageSkillCheckRoot;

	public DebuffSkillCheckRoot DebuffSkillCheckRoot => m_DebuffSkillCheckRoot;

	public int GetSkillCheckDC(SkillCheckDifficulty difficulty, int cr)
	{
		if (difficulty == Kingmaker.View.MapObjects.SkillCheckDifficulty.Custom)
		{
			PFLog.Default.Error("Difficulty is Custom");
			return 0;
		}
		SkillCheckDifficultyEntry skillCheckDifficultyEntry = SkillCheckDifficulty.FirstItem((SkillCheckDifficultyEntry i) => i.Difficulty == difficulty);
		if (skillCheckDifficultyEntry == null || skillCheckDifficultyEntry.CR2DC.Empty())
		{
			PFLog.Default.Error($"Settings is missing dor Difficulty == {difficulty}");
			return 0;
		}
		cr = Math.Clamp(cr, 0, skillCheckDifficultyEntry.CR2DC.Length - 1);
		return skillCheckDifficultyEntry.CR2DC[cr];
	}
}
