using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("207c2f306c834e899ee94e7e8685518f")]
public class ReplaceMobTypeForStatCalculations : EntityFactComponentDelegate, IHashable
{
	public MobTypeForStatCalculations NewType;

	public static bool IsMobTypeForStatCalculationsMelee(MobTypeForStatCalculations? mobType)
	{
		if (mobType != MobTypeForStatCalculations.Melee && mobType != MobTypeForStatCalculations.RangedAndMelee && mobType != MobTypeForStatCalculations.MeleeLeader && mobType != MobTypeForStatCalculations.MeleePsyker && mobType != MobTypeForStatCalculations.MeleeRangedLeader)
		{
			return mobType == MobTypeForStatCalculations.MeleeRangedPsyker;
		}
		return true;
	}

	public static bool IsMobTypeForStatCalculationsNotMelee(MobTypeForStatCalculations? mobType)
	{
		if (mobType != MobTypeForStatCalculations.Ranged && mobType != MobTypeForStatCalculations.Leader && mobType != MobTypeForStatCalculations.Psyker && mobType != MobTypeForStatCalculations.RangedLeader)
		{
			return mobType == MobTypeForStatCalculations.RangedPsyker;
		}
		return true;
	}

	public static bool IsMobTypeForStatCalculationsRanged(MobTypeForStatCalculations? mobType)
	{
		if (mobType != MobTypeForStatCalculations.Ranged && mobType != MobTypeForStatCalculations.RangedAndMelee && mobType != MobTypeForStatCalculations.RangedLeader && mobType != MobTypeForStatCalculations.RangedPsyker && mobType != MobTypeForStatCalculations.MeleeRangedLeader)
		{
			return mobType == MobTypeForStatCalculations.MeleeRangedPsyker;
		}
		return true;
	}

	public static bool IsMobTypeForStatCalculationsNotRanged(MobTypeForStatCalculations? mobType)
	{
		if (mobType != MobTypeForStatCalculations.Melee && mobType != MobTypeForStatCalculations.Leader && mobType != MobTypeForStatCalculations.Psyker && mobType != MobTypeForStatCalculations.MeleeLeader)
		{
			return mobType == MobTypeForStatCalculations.MeleePsyker;
		}
		return true;
	}

	public static bool IsMobTypeForStatCalculationsLeader(MobTypeForStatCalculations? mobType)
	{
		if (mobType != MobTypeForStatCalculations.Leader && mobType != MobTypeForStatCalculations.MeleeLeader && mobType != MobTypeForStatCalculations.RangedLeader)
		{
			return mobType == MobTypeForStatCalculations.MeleeRangedLeader;
		}
		return true;
	}

	public static bool IsMobTypeForStatCalculationsPsyker(MobTypeForStatCalculations? mobType)
	{
		if (mobType != MobTypeForStatCalculations.Psyker && mobType != MobTypeForStatCalculations.MeleePsyker && mobType != MobTypeForStatCalculations.RangedPsyker)
		{
			return mobType == MobTypeForStatCalculations.MeleeRangedPsyker;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
