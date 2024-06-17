using System;

namespace Kingmaker.UnitLogic.Abilities;

public class DamagePredictionData
{
	public int MinDamage;

	public int MaxDamage;

	public int Penetration;

	public static DamagePredictionData operator +(DamagePredictionData lhs, DamagePredictionData rhs)
	{
		if (lhs == null)
		{
			return rhs ?? new DamagePredictionData();
		}
		if (rhs == null)
		{
			return lhs;
		}
		return new DamagePredictionData
		{
			MinDamage = lhs.MinDamage + rhs.MinDamage,
			MaxDamage = lhs.MaxDamage + rhs.MaxDamage,
			Penetration = lhs.Penetration + rhs.Penetration
		};
	}

	public static DamagePredictionData Merge(DamagePredictionData lhs, DamagePredictionData rhs)
	{
		if (lhs == null)
		{
			return rhs ?? new DamagePredictionData();
		}
		if (rhs == null)
		{
			return lhs;
		}
		return new DamagePredictionData
		{
			MinDamage = Math.Min(lhs.MinDamage, rhs.MinDamage),
			MaxDamage = Math.Max(lhs.MaxDamage, rhs.MaxDamage),
			Penetration = Math.Min(lhs.Penetration, rhs.Penetration)
		};
	}
}
