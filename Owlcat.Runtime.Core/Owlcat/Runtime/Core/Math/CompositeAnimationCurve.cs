using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Math;

[Serializable]
public class CompositeAnimationCurve
{
	[Serializable]
	public struct Entry
	{
		public float Amplitude;

		public float Frequency;

		public float OffsetX;

		public float OffsetY;

		public float ScrollSpeed;
	}

	public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public List<Entry> Entries = new List<Entry>();

	public float EvaluateNormalized(float time)
	{
		time = math.saturate(time);
		float num = 0f;
		foreach (Entry entry2 in Entries)
		{
			Entry entry = entry2;
			num += EvaluateEntry(in entry, time);
		}
		float num2 = ScaleCurve.EvaluateNormalized(time);
		return num * num2;
	}

	private float EvaluateEntry(in Entry entry, float time)
	{
		return math.sin((entry.OffsetX + time + Time.time * entry.ScrollSpeed) * entry.Frequency) * 0.5f * entry.Amplitude + entry.OffsetY;
	}

	public float EvaluateNormalizedWithOffsets(float time, in Entry offsets)
	{
		time = math.saturate(time);
		float num = 0f;
		foreach (Entry entry2 in Entries)
		{
			Entry entry = entry2;
			num += EvaluateEntryWithOffsets(in entry, time, in offsets);
		}
		float num2 = ScaleCurve.EvaluateNormalized(time);
		return num * num2;
	}

	private float EvaluateEntryWithOffsets(in Entry entry, float time, in Entry offsets)
	{
		return math.sin((entry.OffsetX + offsets.OffsetX + time + Time.time * (entry.ScrollSpeed + offsets.ScrollSpeed)) * (entry.Frequency + offsets.Frequency)) * 0.5f * (entry.Amplitude + offsets.Amplitude) + entry.OffsetY + offsets.OffsetY;
	}
}
