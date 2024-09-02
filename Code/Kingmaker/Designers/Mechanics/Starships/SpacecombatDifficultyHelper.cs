using Kingmaker.EntitySystem.Entities;
using Kingmaker.Settings;

namespace Kingmaker.Designers.Mechanics.Starships;

public static class SpacecombatDifficultyHelper
{
	private static SpaceCombatDifficulty SpaceCombatDifficulty => SettingsRoot.Difficulty.SpaceCombatDifficulty.GetValue();

	public static float StarshipDamageMod(StarshipEntity initiator)
	{
		if (!initiator.IsPlayerEnemy)
		{
			return 1f;
		}
		return SpaceCombatDifficulty switch
		{
			SpaceCombatDifficulty.Easy => 0.1f, 
			SpaceCombatDifficulty.Normal => 0.6f, 
			SpaceCombatDifficulty.Hard => 1.25f, 
			_ => 1f, 
		};
	}

	public static float HullIntegrityMod(StarshipEntity initiator)
	{
		if (!initiator.IsPlayerEnemy || initiator.IsSoftUnit)
		{
			return 1f;
		}
		return SpaceCombatDifficulty switch
		{
			SpaceCombatDifficulty.Easy => 0.6f, 
			SpaceCombatDifficulty.Hard => 1.2f, 
			_ => 1f, 
		};
	}

	public static float RepairCostMod()
	{
		return SpaceCombatDifficulty switch
		{
			SpaceCombatDifficulty.Easy => 0.05f, 
			SpaceCombatDifficulty.Normal => 0.8f, 
			_ => 1f, 
		};
	}

	public static float StarshipAvoidanceMod(StarshipEntity initiator)
	{
		if (!initiator.IsPlayerEnemy)
		{
			return 1f;
		}
		if (SpaceCombatDifficulty == SpaceCombatDifficulty.Easy)
		{
			return 0.25f;
		}
		return 1f;
	}
}
