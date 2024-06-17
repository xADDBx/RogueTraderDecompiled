using System;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

public class DamageData
{
	public const int MaxAbsorptionPercentsWithPenetration = 90;

	public readonly CompositeModifiersManager Modifiers = new CompositeModifiersManager(0);

	public readonly ValueModifiersManager MinValueModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager MaxValueModifiers = new ValueModifiersManager();

	public readonly CompositeModifiersManager CriticalDamageModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager Absorption = new CompositeModifiersManager(0);

	public readonly CompositeModifiersManager Deflection = new CompositeModifiersManager(0);

	public readonly CompositeModifiersManager Penetration = new CompositeModifiersManager(0);

	private int? m_InitialRolledValue;

	public DamageType Type { get; }

	public int MinValueBase { get; }

	public int MaxValueBase { get; }

	public bool IsCalculated { get; private set; }

	public int OverpenetrationFactorPercents { get; set; }

	public bool Overpenetrating { get; set; }

	public bool UnreducedOverpenetration { get; set; }

	public int? CalculatedValue { get; set; }

	public bool CausedByCheckFail { get; set; }

	public bool IsCritical { get; set; }

	private float? Roll { get; set; }

	public int BaseRolledValue
	{
		get
		{
			if (!Roll.HasValue)
			{
				if (MinValueBaseWithMinModifiers != MaxValueBaseWithMaxModifiers)
				{
					return 0;
				}
				return MinValueBaseWithMinModifiers;
			}
			return MinValueBaseWithMinModifiers + Mathf.RoundToInt((float)(MaxValueBaseWithMaxModifiers - MinValueBaseWithMinModifiers) * Roll.Value);
		}
	}

	public int InitialRolledValue
	{
		get
		{
			if (!Roll.HasValue)
			{
				int? calculatedValue = CalculatedValue;
				if (!calculatedValue.HasValue)
				{
					if (MaxInitialValue != MinInitialValue)
					{
						return 0;
					}
					return MaxInitialValue;
				}
				return calculatedValue.GetValueOrDefault();
			}
			return MinInitialValue + Mathf.RoundToInt((float)(MaxInitialValue - MinInitialValue) * Roll.Value);
		}
	}

	public int CriticalRolledValue
	{
		get
		{
			if (!Roll.HasValue)
			{
				return 0;
			}
			return MinCriticalBonus + Mathf.RoundToInt((float)(MaxCriticalBonus - MinCriticalBonus) * Roll.Value);
		}
	}

	public int MinValueBaseWithMinModifiers => MinValueBase + MinValueModifiers.Value;

	public int MaxValueBaseWithMaxModifiers => MaxValueBase + MaxValueModifiers.Value;

	public int MinInitialValue => Mathf.Max(0, Modifiers.Apply(MinValueBaseWithMinModifiers));

	public int MaxInitialValue => Mathf.Max(0, Modifiers.Apply(MaxValueBaseWithMaxModifiers));

	public int MinCriticalBonus
	{
		get
		{
			if (!IsCritical)
			{
				return 0;
			}
			return Mathf.Max(0, Modifiers.ApplyPctMulExtra(CriticalDamageModifiers.Apply(MinValueBaseWithMinModifiers) - MinValueBaseWithMinModifiers));
		}
	}

	public int MaxCriticalBonus
	{
		get
		{
			if (!IsCritical)
			{
				return 0;
			}
			return Mathf.Max(0, Modifiers.ApplyPctMulExtra(CriticalDamageModifiers.Apply(MaxValueBaseWithMaxModifiers) - MaxValueBaseWithMaxModifiers));
		}
	}

	public int MinValue => GetMinValue(withArmorReduction: true);

	public int MaxValue => GetMaxValue(withArmorReduction: true);

	public int MinValueWithoutArmorReduction => GetMinValue(withArmorReduction: false);

	public int MaxValueWithoutArmorReduction => GetMaxValue(withArmorReduction: false);

	public int AverageValue => Mathf.RoundToInt((float)(MinValue + MaxValue) / 2f);

	public int AverageValueWithoutArmorReduction => Mathf.RoundToInt((float)(MinValueWithoutArmorReduction + MaxValueWithoutArmorReduction) / 2f);

	public int AbsorptionPercentsWithPenetration => Math.Min(Math.Max(0, Absorption.Value - Penetration.Value), 90);

	public float AbsorptionFactorWithPenetration => (float)(100 - AbsorptionPercentsWithPenetration) / 100f;

	public int AbsorptionPercentsWithoutPenetration => Absorption.Value;

	public float AbsorptionFactorWithoutPenetration => (float)(100 - AbsorptionPercentsWithoutPenetration) / 100f;

	public float EffectiveOverpenetrationFactor => Mathf.Clamp((float)OverpenetrationFactorPercents / 100f, 0f, 1f);

	public float OverpenetrationModifier
	{
		get
		{
			if (!Overpenetrating)
			{
				return 1f;
			}
			return EffectiveOverpenetrationFactor;
		}
	}

	public bool Immune => false;

	public void SetRoll(RuleRollD100 roll)
	{
		Roll = Math.Clamp((float)roll.Result / 100f, 0f, 1f);
	}

	public DamageData(DamageType type, int min, int max)
	{
		Type = type;
		MinValueBase = Math.Max(min, 0);
		MaxValueBase = Math.Max(max, MinValueBase);
		if (min > max)
		{
			PFLog.Default.ErrorWithReport($"Invalid damage range: min > max, [{min}..{max}]");
		}
		if (min < 0 || max < 0)
		{
			PFLog.Default.ErrorWithReport($"Invalid damage range: min < 0 || max < 0, [{min}..{max}]");
		}
	}

	public DamageData(DamageType type, int value)
		: this(type, value, value)
	{
	}

	public DamageData Copy(DamageType overrideDamageType)
	{
		return Copy(withModifiers: true, overrideDamageType);
	}

	public DamageData Copy()
	{
		return Copy(withModifiers: true);
	}

	public DamageData CopyWithoutModifiers()
	{
		return Copy(withModifiers: false);
	}

	private DamageData Copy(bool withModifiers, DamageType? overrideDamageType = null)
	{
		DamageData damageData = new DamageData(overrideDamageType ?? Type, MinValueBase, MaxValueBase)
		{
			CalculatedValue = CalculatedValue,
			CausedByCheckFail = CausedByCheckFail,
			Overpenetrating = Overpenetrating,
			OverpenetrationFactorPercents = OverpenetrationFactorPercents
		};
		if (withModifiers)
		{
			damageData.IsCalculated = IsCalculated;
			damageData.CopyModifiersFrom(this);
		}
		return damageData;
	}

	public void CopyModifiersFrom(DamageData source)
	{
		Modifiers.CopyFrom(source.Modifiers);
		MinValueModifiers.CopyFrom(source.MinValueModifiers);
		MaxValueModifiers.CopyFrom(source.MaxValueModifiers);
		CriticalDamageModifiers.CopyFrom(source.CriticalDamageModifiers);
		Absorption.CopyFrom(source.Absorption);
		Deflection.CopyFrom(source.Deflection);
		Penetration.CopyFrom(source.Penetration);
	}

	private int GetMinValue(bool withArmorReduction)
	{
		int num = Mathf.Max(0, MinInitialValue + MinCriticalBonus);
		num = Mathf.RoundToInt((float)num * OverpenetrationModifier);
		num = (withArmorReduction ? ((int)((float)(num - Deflection.Value) * AbsorptionFactorWithPenetration)) : num);
		return Mathf.Max(0, num);
	}

	private int GetMaxValue(bool withArmorReduction)
	{
		int num = Mathf.Max(0, MaxInitialValue + MaxCriticalBonus);
		num = Mathf.RoundToInt((float)num * OverpenetrationModifier);
		num = (withArmorReduction ? ((int)((float)(num - Deflection.Value) * AbsorptionFactorWithPenetration)) : num);
		return Mathf.Max(0, num);
	}

	public int GetMaxValueWithoutPenalties()
	{
		return Mathf.Max(0, Modifiers.Apply(MinValueBase + MinValueModifiers.GetValue(Modifier.IsPositive), Modifier.IsPositive));
	}

	public void MarkCalculated()
	{
		IsCalculated = true;
	}
}
