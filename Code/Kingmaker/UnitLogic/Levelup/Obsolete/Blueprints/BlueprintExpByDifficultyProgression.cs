using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View.MapObjects;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[TypeId("b68dc1e6fcfc4230aa5926c816922d6e")]
public class BlueprintExpByDifficultyProgression : BlueprintScriptableObject
{
	[Serializable]
	public struct ExpByDifficulty
	{
		public SkillCheckDifficulty Difficulty;

		public float Modifier;
	}

	public ExpByDifficulty[] Bonuses;

	public SkillCheckDifficulty GetDifficulty(float modifier)
	{
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < Bonuses.Length; i++)
		{
			if (MathF.Abs(Bonuses[i].Modifier - modifier) < num)
			{
				num2 = i;
				num = MathF.Abs(Bonuses[i].Modifier - modifier);
			}
		}
		return Bonuses[num2].Difficulty;
	}

	public float GetModifier(SkillCheckDifficulty dif)
	{
		for (int i = 0; i < Bonuses.Length; i++)
		{
			if (dif == Bonuses[i].Difficulty)
			{
				return Bonuses[i].Modifier;
			}
		}
		return 1f;
	}
}
