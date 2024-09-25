using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual;

[Serializable]
public class CompositeAnimationCurve
{
	[Serializable]
	public class Entry
	{
		public float Amplitude = 1f;

		public float Frequency = 1f;

		public float OffsetX;

		public float OffsetY;

		public float ScrollSpeed;
	}

	public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public List<Entry> Entries = new List<Entry>();

	public float EvaluateNormalized(float time)
	{
		time = Mathf.Clamp01(time);
		float num = 0f;
		foreach (Entry entry in Entries)
		{
			num += EvaluateEntry(entry, time);
		}
		float num2 = ScaleCurve.EvaluateNormalized(time);
		return num * num2;
	}

	private float EvaluateEntry(Entry entry, float time)
	{
		return Mathf.Sin((entry.OffsetX + time + Time.time * entry.ScrollSpeed) * entry.Frequency) * 0.5f * entry.Amplitude + entry.OffsetY;
	}

	public float EvaluateNormalizedWithOffsets(float time, Entry offsets)
	{
		time = Mathf.Clamp01(time);
		float num = 0f;
		foreach (Entry entry in Entries)
		{
			num += EvaluateEntryWithOffsets(entry, time, offsets);
		}
		float num2 = ScaleCurve.EvaluateNormalized(time);
		return num * num2;
	}

	private float EvaluateEntryWithOffsets(Entry entry, float time, Entry offsets)
	{
		return Mathf.Sin((entry.OffsetX + offsets.OffsetX + time + Time.time * (entry.ScrollSpeed + offsets.ScrollSpeed)) * (entry.Frequency + offsets.Frequency)) * 0.5f * (entry.Amplitude + offsets.Amplitude) + entry.OffsetY + offsets.OffsetY;
	}
}
