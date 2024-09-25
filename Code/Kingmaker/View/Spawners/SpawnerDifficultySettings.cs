using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
public class SpawnerDifficultySettings : MonoBehaviour, IUnitSpawnRestriction
{
	public enum Type
	{
		Any,
		NormalAndHigher,
		CoreAndHigher,
		NormalAndLower
	}

	public Type Setting;

	public UnitSpawnRestrictionResult CanSpawn()
	{
		SettingsEntityEnum<CombatEncountersCapacity> combatEncountersCapacity = SettingsRoot.Difficulty.CombatEncountersCapacity;
		return CanSpawn(combatEncountersCapacity);
	}

	public UnitSpawnRestrictionResult CanSpawn(CombatEncountersCapacity capacity)
	{
		if (Setting != 0 && (Setting != Type.NormalAndHigher || capacity < CombatEncountersCapacity.Standard) && (Setting != Type.CoreAndHigher || capacity < CombatEncountersCapacity.Enlarged) && (Setting != Type.NormalAndLower || capacity >= CombatEncountersCapacity.Enlarged))
		{
			return UnitSpawnRestrictionResult.Disable;
		}
		return UnitSpawnRestrictionResult.CanSpawn;
	}
}
