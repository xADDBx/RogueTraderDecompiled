using System;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.BaseGetter;

[Serializable]
public class PropertyGetterSettings
{
	public enum ProgressionType
	{
		AsIs,
		Div2,
		OnePlusDiv2,
		StartPlusDivStep,
		DivStep,
		OnePlusDivStep,
		MultiplyByModifier,
		HalfMore,
		BonusValue,
		DelayedStartPlusDivStep,
		StartPlusDoubleDivStep,
		Div2PlusStep,
		Custom,
		DoublePlusBonusValue,
		Sign,
		RandomUpTo
	}

	public enum LimitType
	{
		None,
		Min,
		Max,
		MinMax
	}

	[Serializable]
	private class CustomProgressionItem
	{
		public int BaseValue;

		public int ProgressionValue;
	}

	[SerializeField]
	public ProgressionType Progression;

	[SerializeField]
	[ShowIf("IsCustomProgression")]
	private CustomProgressionItem[] m_CustomProgression = new CustomProgressionItem[0];

	[SerializeField]
	[ShowIf("IsDivisionProgressionStart")]
	private int m_StartLevel;

	[SerializeField]
	[ShowIf("IsDivisionProgression")]
	private int m_StepLevel;

	[SerializeField]
	public bool Negate;

	[SerializeField]
	public LimitType Limit;

	[SerializeField]
	[ShowIf("UseMin")]
	public int Min;

	[SerializeField]
	[ShowIf("UseMax")]
	public int Max;

	public int StepLevel => m_StepLevel;

	private bool IsCustomProgression => Progression == ProgressionType.Custom;

	public bool IsDivisionProgression
	{
		get
		{
			if (Progression != ProgressionType.StartPlusDivStep && Progression != ProgressionType.DivStep && Progression != ProgressionType.OnePlusDivStep && Progression != ProgressionType.MultiplyByModifier && Progression != ProgressionType.BonusValue && Progression != ProgressionType.DelayedStartPlusDivStep && Progression != ProgressionType.StartPlusDoubleDivStep && Progression != ProgressionType.DoublePlusBonusValue)
			{
				return Progression == ProgressionType.Div2PlusStep;
			}
			return true;
		}
	}

	public bool IsDivisionProgressionStart
	{
		get
		{
			if (Progression != ProgressionType.StartPlusDivStep && Progression != ProgressionType.DelayedStartPlusDivStep)
			{
				return Progression == ProgressionType.StartPlusDoubleDivStep;
			}
			return true;
		}
	}

	private bool UseMin
	{
		get
		{
			if (Limit != LimitType.Min)
			{
				return Limit == LimitType.MinMax;
			}
			return true;
		}
	}

	private bool UseMax
	{
		get
		{
			if (Limit != LimitType.Max)
			{
				return Limit == LimitType.MinMax;
			}
			return true;
		}
	}

	public int Apply(int baseValue)
	{
		int value = baseValue;
		value = ApplyProgression(value);
		value = (Negate ? (-value) : value);
		if (UseMin)
		{
			value = Math.Max(Min, value);
		}
		if (UseMax)
		{
			value = Math.Min(Max, value);
		}
		return value;
	}

	private int ApplyProgression(int value)
	{
		switch (Progression)
		{
		case ProgressionType.AsIs:
			return value;
		case ProgressionType.Div2:
			return value / 2;
		case ProgressionType.OnePlusDiv2:
			return 1 + (value - 1) / 2;
		case ProgressionType.StartPlusDivStep:
			return 1 + Math.Max((value - m_StartLevel) / m_StepLevel, 0);
		case ProgressionType.DivStep:
			return value / m_StepLevel;
		case ProgressionType.OnePlusDivStep:
			return 1 + Math.Max(value / m_StepLevel, 0);
		case ProgressionType.MultiplyByModifier:
			return value * m_StepLevel;
		case ProgressionType.HalfMore:
			return value + value / 2;
		case ProgressionType.BonusValue:
			return value + m_StepLevel;
		case ProgressionType.DelayedStartPlusDivStep:
			if (value < m_StartLevel)
			{
				return 0;
			}
			return 1 + Math.Max((value - m_StartLevel) / m_StepLevel, 0);
		case ProgressionType.StartPlusDoubleDivStep:
			return 1 + 2 * Math.Max((value - m_StartLevel) / m_StepLevel, 0);
		case ProgressionType.Div2PlusStep:
			return value / 2 + m_StepLevel;
		case ProgressionType.Custom:
		{
			CustomProgressionItem[] customProgression = m_CustomProgression;
			foreach (CustomProgressionItem customProgressionItem in customProgression)
			{
				if (value <= customProgressionItem.BaseValue)
				{
					return customProgressionItem.ProgressionValue;
				}
			}
			return m_CustomProgression.LastItem()?.ProgressionValue ?? 0;
		}
		case ProgressionType.DoublePlusBonusValue:
			return value * 2 + m_StepLevel;
		case ProgressionType.Sign:
			return Math.Sign(value);
		case ProgressionType.RandomUpTo:
			return PFStatefulRandom.BaseGetter.Range(Math.Max(0, Min), Math.Min(value, Max));
		default:
			throw new ArgumentOutOfRangeException("Progression", Progression, null);
		}
	}

	public string ProgressionToString()
	{
		switch (Progression)
		{
		case ProgressionType.AsIs:
		case ProgressionType.Div2:
		case ProgressionType.OnePlusDiv2:
		case ProgressionType.HalfMore:
		case ProgressionType.Sign:
		case ProgressionType.RandomUpTo:
			return Progression.ToString();
		case ProgressionType.StartPlusDivStep:
			return $"StartPlusDivStep(Start={m_StartLevel}, Div={m_StepLevel})";
		case ProgressionType.DivStep:
			return $"DivStep(Div={m_StepLevel})";
		case ProgressionType.OnePlusDivStep:
			return $"OnePlusDivStep(Div={m_StepLevel})";
		case ProgressionType.MultiplyByModifier:
			return $"MultiplyByModifier(Mul={m_StepLevel})";
		case ProgressionType.BonusValue:
			return $"BonusValue(Bonus={m_StepLevel})";
		case ProgressionType.DelayedStartPlusDivStep:
			return $"DelayedStartPlusDivStep(Start={m_StartLevel}, Div={m_StepLevel})";
		case ProgressionType.StartPlusDoubleDivStep:
			return $"StartPlusDoubleDivStep(Start={m_StartLevel}, Div={m_StepLevel})";
		case ProgressionType.Div2PlusStep:
			return $"Div2PlusStep(Bonus={m_StepLevel})";
		case ProgressionType.Custom:
			return $"CustomProgression(Bonus={m_StepLevel})";
		case ProgressionType.DoublePlusBonusValue:
			return $"DoublePlusBonusValue(Bonus={m_StepLevel})";
		default:
			throw new ArgumentOutOfRangeException("Progression", Progression, null);
		}
	}
}
