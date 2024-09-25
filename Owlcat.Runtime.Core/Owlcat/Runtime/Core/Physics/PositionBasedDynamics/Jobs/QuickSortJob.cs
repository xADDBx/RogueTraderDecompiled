using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct QuickSortJob<T> : IJob where T : struct, IComparable<T>
{
	public NativeArray<T> Entries;

	public int StartIndex;

	public int EndIndex;

	public QuickSortJob(NativeArray<T> entries, int startIndex, int endIndex)
	{
		Entries = entries;
		StartIndex = startIndex;
		EndIndex = endIndex;
	}

	public void Execute()
	{
		EndIndex = math.clamp(EndIndex, 0, Entries.Length - 1);
		StartIndex = math.clamp(StartIndex, 0, EndIndex);
		if (EndIndex == 0)
		{
			EndIndex = Entries.Length - 1;
		}
		if (Entries.Length > 0)
		{
			Quicksort(StartIndex, EndIndex);
		}
	}

	private void Quicksort(int left, int right)
	{
		int i = left;
		int num = right;
		T other = Entries[(left + right) / 2];
		while (i <= num)
		{
			for (; Entries[i].CompareTo(other) < 0; i++)
			{
			}
			while (Entries[num].CompareTo(other) > 0)
			{
				num--;
			}
			if (i <= num)
			{
				T value = Entries[i];
				Entries[i] = Entries[num];
				Entries[num] = value;
				i++;
				num--;
			}
		}
		if (left < num)
		{
			Quicksort(left, num);
		}
		if (i < right)
		{
			Quicksort(i, right);
		}
	}
}
