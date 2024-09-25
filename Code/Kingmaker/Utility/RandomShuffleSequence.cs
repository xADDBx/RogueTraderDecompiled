using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.Utility;

public struct RandomShuffleSequence
{
	private List<int> m_Values;

	private int m_Length;

	private int m_Cursor;

	public int Next(int sequenceLength, [NotNull] Kingmaker.Utility.StatefulRandom.StatefulRandom random)
	{
		if (random == null)
		{
			throw new ArgumentNullException("random");
		}
		if (sequenceLength <= 0)
		{
			throw new ArgumentOutOfRangeException("sequenceLength");
		}
		if (m_Length != sequenceLength)
		{
			m_Length = sequenceLength;
			m_Cursor = 0;
			if (sequenceLength > 2)
			{
				if (m_Values != null)
				{
					m_Values.Clear();
				}
				else
				{
					m_Values = new List<int>(sequenceLength);
				}
				for (int i = 0; i < sequenceLength; i++)
				{
					m_Values.Add(i);
				}
				Shuffle(random);
			}
		}
		if (m_Length > 2)
		{
			if (m_Cursor >= m_Length)
			{
				Shuffle(random);
				m_Cursor = 0;
			}
			return m_Values[m_Cursor++];
		}
		if (m_Length > 1)
		{
			m_Cursor = (m_Cursor + 1) % 2;
			return m_Cursor;
		}
		return 0;
	}

	private void Shuffle([NotNull] Kingmaker.Utility.StatefulRandom.StatefulRandom random)
	{
		int count = m_Values.Count;
		int num = count - 1;
		int num2 = random.Range(0, num);
		if (num2 != 0)
		{
			List<int> values = m_Values;
			List<int> values2 = m_Values;
			int index = num2;
			int num3 = m_Values[num2];
			int num4 = m_Values[0];
			int num6 = (values[0] = num3);
			num6 = (values2[index] = num4);
		}
		for (int i = 1; i < num; i++)
		{
			int num8 = random.Range(i, count);
			if (num8 != i)
			{
				List<int> values3 = m_Values;
				int index = i;
				List<int> values2 = m_Values;
				int num4 = num8;
				int num3 = m_Values[num8];
				int num6 = m_Values[i];
				int num10 = (values3[index] = num3);
				num10 = (values2[num4] = num6);
			}
		}
	}
}
