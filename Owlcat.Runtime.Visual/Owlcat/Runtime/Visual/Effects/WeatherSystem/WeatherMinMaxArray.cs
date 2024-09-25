using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class WeatherMinMaxArray
{
	public float[] MinValues;

	public float[] MaxValues;

	public float Sample(InclemencyType inclemency)
	{
		if (MinValues.Length != 0 && MaxValues.Length != 0)
		{
			return UnityEngine.Random.Range(MinValues[(int)inclemency], MaxValues[(int)inclemency]);
		}
		return 0f;
	}

	public InclemencyType GetCorrespondingInclemency(float value)
	{
		for (int i = 0; i < MinValues.Length; i++)
		{
			if (value >= MinValues[i] && value <= MaxValues[i])
			{
				return (InclemencyType)i;
			}
			if (i < MinValues.Length - 1 && value >= MaxValues[i] && value <= MinValues[i + 1])
			{
				return (InclemencyType)i;
			}
		}
		return InclemencyType.Clear;
	}

	public float GetMaxValue()
	{
		if (MaxValues.Length != 0)
		{
			return MaxValues[MaxValues.Length - 1];
		}
		return 0f;
	}

	public static float TransferValue(WeatherMinMaxArray from, WeatherMinMaxArray to, float value)
	{
		int num = 0;
		float t = 0f;
		bool flag = false;
		for (int i = 0; i < from.MinValues.Length; i++)
		{
			if (value >= from.MinValues[i] && value <= from.MaxValues[i])
			{
				num = i;
				t = Mathf.InverseLerp(from.MinValues[i], from.MaxValues[i], value);
				flag = false;
				break;
			}
			if (i < from.MaxValues.Length - 1 && value >= from.MaxValues[i] && value <= from.MinValues[i + 1])
			{
				num = i;
				t = Mathf.InverseLerp(from.MaxValues[i], from.MinValues[i + 1], value);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return Mathf.Lerp(to.MinValues[num], to.MaxValues[num], t);
		}
		return Mathf.Lerp(to.MaxValues[num], to.MinValues[num + 1], t);
	}
}
