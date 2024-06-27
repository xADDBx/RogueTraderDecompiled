using System;
using System.Collections.Generic;

namespace Mono;

internal class MergeSort<T>
{
	private readonly T[] elements;

	private readonly T[] buffer;

	private readonly IComparer<T> comparer;

	private MergeSort(T[] elements, IComparer<T> comparer)
	{
		this.elements = elements;
		buffer = new T[elements.Length];
		Array.Copy(this.elements, buffer, elements.Length);
		this.comparer = comparer;
	}

	public static void Sort(T[] source, IComparer<T> comparer)
	{
		Sort(source, 0, source.Length, comparer);
	}

	public static void Sort(T[] source, int start, int length, IComparer<T> comparer)
	{
		new MergeSort<T>(source, comparer).Sort(start, length);
	}

	private void Sort(int start, int length)
	{
		TopDownSplitMerge(buffer, elements, start, length);
	}

	private void TopDownSplitMerge(T[] a, T[] b, int start, int end)
	{
		if (end - start >= 2)
		{
			int num = (end + start) / 2;
			TopDownSplitMerge(b, a, start, num);
			TopDownSplitMerge(b, a, num, end);
			TopDownMerge(a, b, start, num, end);
		}
	}

	private void TopDownMerge(T[] a, T[] b, int start, int middle, int end)
	{
		int num = start;
		int num2 = middle;
		for (int i = start; i < end; i++)
		{
			if (num < middle && (num2 >= end || comparer.Compare(a[num], a[num2]) <= 0))
			{
				b[i] = a[num++];
			}
			else
			{
				b[i] = a[num2++];
			}
		}
	}
}
