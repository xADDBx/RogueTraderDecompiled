using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public class CompositeModifiersManager : AbstractModifiersManager
{
	private readonly int m_Min;

	private readonly int m_Max;

	public IEnumerable<Modifier> ValueModifiersList => GetList(ModifierType.ValAdd);

	public IEnumerable<Modifier> PercentModifiersList => GetList(ModifierType.PctAdd);

	public IEnumerable<Modifier> PercentMultipliersList => GetList(ModifierType.PctMul);

	public IEnumerable<Modifier> ValueModifiersExtraList => GetList(ModifierType.ValAdd_Extra);

	public IEnumerable<Modifier> PercentMultipliersExtraList => GetList(ModifierType.PctMul_Extra);

	public IEnumerable<Modifier> AllModifiersList
	{
		get
		{
			foreach (Modifier valueModifiers in ValueModifiersList)
			{
				yield return valueModifiers;
			}
			foreach (Modifier percentModifiers in PercentModifiersList)
			{
				yield return percentModifiers;
			}
			foreach (Modifier percentMultipliers in PercentMultipliersList)
			{
				yield return percentMultipliers;
			}
			foreach (Modifier valueModifiersExtra in ValueModifiersExtraList)
			{
				yield return valueModifiersExtra;
			}
			foreach (Modifier percentMultipliersExtra in PercentMultipliersExtraList)
			{
				yield return percentMultipliersExtra;
			}
		}
	}

	public int Value => Apply(0);

	public CompositeModifiersManager(int min, int max)
	{
		m_Min = min;
		m_Max = max;
	}

	public CompositeModifiersManager(int min)
		: this(min, int.MaxValue)
	{
	}

	public CompositeModifiersManager()
		: this(int.MinValue, int.MaxValue)
	{
	}

	public int Apply(int value, [CanBeNull] Predicate<Modifier> filter = null)
	{
		GetValues(out var valAdd, out var pctAdd, out var pctMul, out var valAddExtra, out var pctMulExtra, filter);
		return Math.Clamp(Mathf.RoundToInt(((float)(value + valAdd) * (1f + pctAdd) * pctMul + (float)valAddExtra) * pctMulExtra), m_Min, m_Max);
	}

	public int ApplyPctMulExtra(int value)
	{
		GetValues(out var _, out var _, out var _, out var _, out var pctMulExtra);
		return Math.Clamp(Mathf.RoundToInt((float)value * pctMulExtra), m_Min, m_Max);
	}

	public void CopyFrom([CanBeNull] AbstractModifiersManager source)
	{
		CopyFrom(source, null);
	}
}
