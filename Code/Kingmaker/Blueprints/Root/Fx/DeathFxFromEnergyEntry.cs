using System;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Fx;

[Serializable]
public class DeathFxFromEnergyEntry
{
	public DamageType EnergyType;

	public bool PlayBloodPuddle;

	public bool PlayTint;

	[AssetPicker("")]
	public GameObject EnergyDeathPrefab;

	public static DeathFxFromEnergyEntry Default { get; } = new DeathFxFromEnergyEntry();


	public DeathFxFromEnergyEntry()
	{
		PlayBloodPuddle = true;
		PlayTint = true;
	}

	public static int GetMaxEntriesCount()
	{
		return EnumUtils.GetMaxValue<DamageType>() + 2;
	}

	public int GetEntryIndex()
	{
		return GetEnergyDamageEntryIndex(EnergyType);
	}

	public static int GetEnergyDamageEntryIndex(DamageType energyType)
	{
		return (int)energyType;
	}
}
