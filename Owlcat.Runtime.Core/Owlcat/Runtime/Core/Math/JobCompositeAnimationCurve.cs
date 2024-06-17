using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Math;

public struct JobCompositeAnimationCurve
{
	public NativeArray<CompositeAnimationCurve.Entry> Entries;

	public JobAnimationCurve ScaleCurve;

	public JobCompositeAnimationCurve(CompositeAnimationCurve curve)
	{
		ScaleCurve = new JobAnimationCurve(curve.ScaleCurve);
		Entries = new NativeArray<CompositeAnimationCurve.Entry>(curve.Entries.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < curve.Entries.Count; i++)
		{
			Entries[i] = curve.Entries[i];
		}
	}

	public float EvaluateNormalized(float time, float globalTime)
	{
		time = math.saturate(time);
		float num = 0f;
		for (int i = 0; i < Entries.Length; i++)
		{
			float num2 = num;
			CompositeAnimationCurve.Entry entry = Entries[i];
			num = num2 + EvaluateEntry(in entry, time, globalTime);
		}
		float num3 = ScaleCurve.EvaluateNormalized(time);
		return num * num3;
	}

	private float EvaluateEntry(in CompositeAnimationCurve.Entry entry, float time, float globalTime)
	{
		return math.sin((entry.OffsetX + time + globalTime * entry.ScrollSpeed) * entry.Frequency) * 0.5f * entry.Amplitude + entry.OffsetY;
	}

	public void Dispose()
	{
		if (Entries.IsCreated)
		{
			Entries.Dispose();
		}
		ScaleCurve.Dispose();
	}
}
