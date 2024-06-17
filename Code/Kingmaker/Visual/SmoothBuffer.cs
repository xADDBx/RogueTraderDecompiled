using System;
using System.Collections.Generic;

namespace Kingmaker.Visual;

public class SmoothBuffer<T> where T : struct
{
	private Queue<T> m_Buffer = new Queue<T>();

	private float[] m_Weights;

	private Func<T, T, T> m_Add;

	private Func<T, float, T> m_Scale;

	public T Value { get; private set; }

	public SmoothBuffer(int capacity, Func<T, T, T> add, Func<T, float, T> scale)
	{
		m_Add = add;
		m_Scale = scale;
		m_Weights = new float[capacity];
		float num = 0f;
		for (int i = 0; i < capacity; i++)
		{
			m_Weights[i] = i + 1;
			num += (float)(i + 1);
		}
		for (int j = 0; j < capacity; j++)
		{
			m_Weights[j] /= num;
		}
		Value = default(T);
		for (int k = 0; k < capacity; k++)
		{
			m_Buffer.Enqueue(default(T));
		}
	}

	public void Update(T value)
	{
		m_Buffer.Dequeue();
		m_Buffer.Enqueue(value);
		T arg = default(T);
		int num = 0;
		foreach (T item in m_Buffer)
		{
			T arg2 = m_Scale(item, m_Weights[num]);
			arg = m_Add(arg, arg2);
			num++;
		}
		float arg3 = 1f / (float)m_Buffer.Count;
		Value = m_Scale(arg, arg3);
	}
}
