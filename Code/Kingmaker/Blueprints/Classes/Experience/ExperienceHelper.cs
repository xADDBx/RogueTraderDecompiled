using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Classes.Experience;

public static class ExperienceHelper
{
	private const int MaxEncounter = 6;

	private static readonly double[] s_EncounterModifiers;

	private static readonly double[] s_EnemyDifficultyModifiers;

	public static int MaxCR => Game.Instance.BlueprintRoot.Progression.CRTable.Bonuses.Length - 1;

	static ExperienceHelper()
	{
		s_EncounterModifiers = new double[7];
		s_EnemyDifficultyModifiers = new double[7];
		s_EncounterModifiers[0] = 1.4;
		s_EncounterModifiers[1] = 7.0;
		s_EncounterModifiers[2] = 28.0;
		s_EncounterModifiers[3] = 7.0;
		s_EncounterModifiers[4] = 2.8;
		s_EncounterModifiers[5] = 7.0;
		s_EncounterModifiers[6] = 0.7;
		s_EnemyDifficultyModifiers[0] = 0.21;
		s_EnemyDifficultyModifiers[1] = 0.42;
		s_EnemyDifficultyModifiers[2] = 1.4;
		s_EnemyDifficultyModifiers[3] = 2.8;
		s_EnemyDifficultyModifiers[4] = 8.4;
		s_EnemyDifficultyModifiers[5] = 21.0;
		s_EnemyDifficultyModifiers[6] = 52.5;
	}

	public static int GetXp(EncounterType type, int cr, float modifier = 1f, [CanBeNull] IntEvaluator count = null)
	{
		int bonus = BlueprintRoot.Instance.Progression.CRTable.GetBonus(cr);
		bonus = (int)(s_EncounterModifiers[(int)type] * (double)bonus);
		if ((double)Mathf.Abs(modifier - 1f) > 1E-06)
		{
			bonus = (int)((float)bonus * modifier);
		}
		if (count != null)
		{
			bonus *= count.GetValue();
		}
		return bonus;
	}

	public static int GetCRPoints(int cr, float modifier = 1f, [CanBeNull] IntEvaluator count = null)
	{
		int num = BlueprintRoot.Instance.Progression.XPToCRTable.GetBonus(cr);
		if ((double)Mathf.Abs(modifier - 1f) > 1E-06)
		{
			num = (int)((float)num * modifier);
		}
		if (count != null)
		{
			num *= count.GetValue();
		}
		return num;
	}

	public static int GetCR(int crPoints, BlueprintStatProgression progressionCrTable = null)
	{
		progressionCrTable = SimpleBlueprintExtendAsObject.Or(progressionCrTable, Game.Instance.BlueprintRoot.Progression.XPToCRTable);
		if (progressionCrTable == null)
		{
			return 0;
		}
		int[] bonuses = progressionCrTable.Bonuses;
		for (int i = 0; i < bonuses.Length; i++)
		{
			int num = bonuses[i];
			if (num == crPoints || (i == 0 && num >= crPoints) || i == bonuses.Length - 1)
			{
				return i;
			}
			int num2 = bonuses[i + 1];
			if (num < crPoints && crPoints < num2)
			{
				if (crPoints - num >= num2 - crPoints)
				{
					return i + 1;
				}
				return i;
			}
		}
		return 0;
	}

	public static int GetCheckExp(int difficultyMod, int areaCR)
	{
		BlueprintStatProgression dCToCRTable = BlueprintRoot.Instance.Progression.DCToCRTable;
		int bonus = dCToCRTable.Bonuses.FirstOrDefault((int p) => difficultyMod >= p);
		int level = dCToCRTable.GetLevel(bonus);
		if (areaCR > 0)
		{
			int num = dCToCRTable.GetBonus(areaCR) - dCToCRTable.GetBonus(level);
			double num2 = ((num >= 30) ? s_EncounterModifiers[5] : ((num >= 10) ? s_EncounterModifiers[4] : s_EncounterModifiers[6]));
			return (int)((double)BlueprintRoot.Instance.Progression.CRTable.GetBonus(areaCR) * num2) / 2;
		}
		return BlueprintRoot.Instance.Progression.CRTable.GetBonus(level);
	}

	public static int GetMobExp(UnitDifficultyType difficultyType, int areaCR)
	{
		return (int)((double)BlueprintRoot.Instance.Progression.CRTable.GetBonus(areaCR) * s_EnemyDifficultyModifiers[(int)difficultyType] + 0.5);
	}
}
