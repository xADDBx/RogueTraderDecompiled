using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

[HashRoot]
public class FeatureParam : IHashable
{
	[JsonProperty]
	[CanBeNull]
	public readonly BlueprintScriptableObject Blueprint;

	[JsonProperty]
	public readonly WeaponCategory? WeaponCategory;

	[JsonProperty]
	public readonly SpellSchool? SpellSchool;

	[JsonProperty]
	public readonly StatType? StatType;

	public FeatureParam Value => this;

	public FeatureParam()
	{
	}

	public FeatureParam(BlueprintScriptableObject blueprint)
		: this()
	{
		Blueprint = blueprint;
	}

	public FeatureParam(WeaponCategory? weaponCategory)
		: this()
	{
		WeaponCategory = weaponCategory;
	}

	public FeatureParam(SpellSchool? spellSchool)
		: this()
	{
		SpellSchool = spellSchool;
	}

	public FeatureParam(StatType? statType)
		: this()
	{
		StatType = statType;
	}

	public static implicit operator FeatureParam(BlueprintScriptableObject blueprint)
	{
		return new FeatureParam(blueprint);
	}

	public static implicit operator FeatureParam(WeaponCategory weaponCategory)
	{
		return new FeatureParam(weaponCategory);
	}

	public static implicit operator FeatureParam(SpellSchool spellSchool)
	{
		return new FeatureParam(spellSchool);
	}

	public static implicit operator FeatureParam(StatType statType)
	{
		return new FeatureParam(statType);
	}

	public bool Equals(FeatureParam other)
	{
		if (object.Equals(Blueprint, other?.Blueprint) && WeaponCategory == other?.WeaponCategory && SpellSchool == other?.SpellSchool)
		{
			return StatType == other?.StatType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is FeatureParam)
		{
			return Equals((FeatureParam)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((((Blueprint != null) ? Blueprint.GetHashCode() : 0) * 397) ^ WeaponCategory.GetHashCode()) * 397) ^ SpellSchool.GetHashCode()) * 397) ^ StatType.GetHashCode();
	}

	public static bool operator ==(FeatureParam f1, FeatureParam f2)
	{
		return f1?.Equals(f2) ?? ((object)f2 == null);
	}

	public static bool operator !=(FeatureParam f1, FeatureParam f2)
	{
		return !(f1?.Equals(f2) ?? ((object)f2 == null));
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		if (WeaponCategory.HasValue)
		{
			WeaponCategory val2 = WeaponCategory.Value;
			result.Append(ref val2);
		}
		if (SpellSchool.HasValue)
		{
			SpellSchool val3 = SpellSchool.Value;
			result.Append(ref val3);
		}
		if (StatType.HasValue)
		{
			StatType val4 = StatType.Value;
			result.Append(ref val4);
		}
		return result;
	}
}
