using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Add ability resources")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("5991a77890cb6824eac58250da4b55c2")]
public class ReplaceStatForPrerequisites : UnitFactComponentDelegate, IHashable
{
	public enum StatReplacementPolicy
	{
		NewStat,
		ClassLevel,
		ClassLevelStacking,
		SpecificNumber,
		Summand
	}

	public StatType OldStat;

	public StatReplacementPolicy Policy;

	[ShowIf("IsNewStat")]
	public StatType NewStat;

	[ShowIf("IsClassLevel")]
	[SerializeField]
	[FormerlySerializedAs("CharacterClass")]
	private BlueprintCharacterClassReference m_CharacterClass;

	[ShowIf("IsSpecificNumber")]
	public int SpecificNumber;

	public BlueprintCharacterClass CharacterClass => m_CharacterClass?.Get();

	[UsedImplicitly]
	private bool IsNewStat => Policy == StatReplacementPolicy.NewStat;

	[UsedImplicitly]
	private bool IsClassLevel
	{
		get
		{
			if (Policy != StatReplacementPolicy.ClassLevel)
			{
				return Policy == StatReplacementPolicy.ClassLevelStacking;
			}
			return true;
		}
	}

	[UsedImplicitly]
	private bool IsSpecificNumber
	{
		get
		{
			if (Policy != StatReplacementPolicy.SpecificNumber)
			{
				return Policy == StatReplacementPolicy.Summand;
			}
			return true;
		}
	}

	public static int ResultStat(ReplaceStatForPrerequisites c, int oldStat, BaseUnitEntity unit, bool fullValue)
	{
		switch (c.Policy)
		{
		case StatReplacementPolicy.NewStat:
			if (!fullValue)
			{
				return Math.Max(oldStat, unit.Stats.GetStat(c.NewStat));
			}
			return Math.Max(oldStat, unit.Stats.GetStat(c.NewStat).PermanentValue);
		case StatReplacementPolicy.ClassLevel:
			return Math.Max(oldStat, unit.Progression.GetClassLevel(c.CharacterClass));
		case StatReplacementPolicy.ClassLevelStacking:
			return oldStat + unit.Progression.GetClassLevel(c.CharacterClass);
		case StatReplacementPolicy.SpecificNumber:
			return Math.Max(oldStat, c.SpecificNumber);
		case StatReplacementPolicy.Summand:
			return oldStat + c.SpecificNumber;
		default:
			return oldStat;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
