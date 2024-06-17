using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.View.Spawners;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Customization;

[Serializable]
public class UnitCustomizationVariation : IEquatable<UnitCustomizationVariation>
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Race")]
	private BlueprintRaceReference m_Race;

	public Gender Gender;

	[NotNull]
	public UnitViewLink Prefab = new UnitViewLink();

	public static UnitCustomizationVariation Default => new UnitCustomizationVariation(BlueprintRoot.Instance.Progression.CharacterRaces.FirstOrDefault(), Gender.Male);

	public BlueprintRace Race => m_Race?.Get();

	[UsedImplicitly]
	private UnitCustomizationVariation()
	{
	}

	public UnitCustomizationVariation([NotNull] BlueprintRace race, Gender gender)
	{
		m_Race = race.ToReference<BlueprintRaceReference>();
		Gender = gender;
	}

	public UnitCustomizationVariation(UnitCustomizationVariation other)
		: this(other.Race, other.Gender)
	{
	}

	public SpawningData CreateSpawningData(BlueprintUnitAsksList voice, bool leftHanded)
	{
		return ContextData<SpawningData>.Request().Setup(Prefab.AssetId, Race, Gender, voice, leftHanded);
	}

	public bool Equals(UnitCustomizationVariation other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Race.Equals(other.Race))
		{
			return Gender == other.Gender;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((UnitCustomizationVariation)obj);
	}

	public override int GetHashCode()
	{
		return (Race.GetHashCode() * 397) ^ (int)Gender;
	}
}
