using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("998cc31870f6467c86944e216e1d43a7")]
public class BlueprintAnomalyGroupSpawnSettings : BlueprintScriptableObject
{
	[Flags]
	public enum SpawnPlace
	{
		Orbital = 2,
		Planetary = 4,
		Star = 8,
		ArtificialObjects = 0x10,
		Asteroids = 0x20,
		RandomEmptyPlace = 0x40
	}

	[Serializable]
	public class Reference : BlueprintReference<BlueprintAnomalyGroupSpawnSettings>
	{
	}

	public SpawnPlace AnomalySpawnPlace;

	public int AnomalyMinCountInSystem;

	public int AnomalyMaxCountInSystem;

	public AnomalyWeights AnomalySpawnInGroupWeight;
}
