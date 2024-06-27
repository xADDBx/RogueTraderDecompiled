using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Kingmaker.Utility.DotNetExtensions;

public struct PersistentRandom
{
	public struct Generator
	{
		private int m_Seed;

		public PersistentRandom Next => Seed(m_Seed = GetNext(m_Seed));

		public Generator Branch(string salt, int additionalSalt = 0)
		{
			return Next.MakeGenerator(salt, additionalSalt);
		}

		public Generator(int seed)
		{
			m_Seed = seed;
		}

		public int NextRange(int min, int maxExcluded)
		{
			return Next.Range(min, maxExcluded);
		}

		public float NextRangeFloat(float min, float maxIncluded, int precision)
		{
			if (precision < 0)
			{
				precision = 0;
			}
			float num = (float)Math.Pow(10.0, precision);
			int minIncluded = (int)(min * num);
			int maxExcluded = (int)(maxIncluded * num) + 1;
			return (float)Next.Range(minIncluded, maxExcluded) / num;
		}
	}

	private const int a = 1222003;

	private const int b = 1199461;

	private const int c = 15497;

	public readonly int Value;

	public int NextValue => GetNext(Value);

	public int Chance => Range(0, 100);

	public static int GetNext(int seed)
	{
		int num = 1222003 * seed + 1199461 * seed + 15497;
		int num2 = num ^ (num << 13);
		int num3 = num2 ^ (num2 >> 7);
		return num3 ^ (num3 << 17);
	}

	public static PersistentRandom Seed(int seed)
	{
		return new PersistentRandom(seed);
	}

	public PersistentRandom(int seed)
	{
		Value = seed;
	}

	public PersistentRandom Next()
	{
		return new PersistentRandom(NextValue);
	}

	public PersistentRandom Next(int salt)
	{
		return new PersistentRandom(GetNext(NextValue + GetNext(salt) + GetNext(~salt)));
	}

	public PersistentRandom Next(string salt, int additionalSalt = 0)
	{
		return Next((salt != null) ? (salt.GetHashCode() + additionalSalt) : additionalSalt);
	}

	public int Range(int minIncluded, int maxExcluded)
	{
		if (minIncluded >= maxExcluded)
		{
			return minIncluded;
		}
		long num = (long)maxExcluded - (long)minIncluded;
		return minIncluded + (int)(Math.Abs(Value) % num);
	}

	[CanBeNull]
	public T GetRandomItem<T>(IEnumerable<T> list)
	{
		if (list == null)
		{
			return default(T);
		}
		int num = 0;
		T result = default(T);
		Generator generator = MakeGenerator();
		foreach (T item in list)
		{
			if (generator.Next.Range(0, num + 1) >= num)
			{
				result = item;
			}
			num++;
		}
		return result;
	}

	public void ShuffleList<T>(IList<T> list)
	{
		Generator generator = MakeGenerator();
		for (int i = 0; i < list.Count; i++)
		{
			int index = generator.Next.Range(0, list.Count);
			list.SwapItemsAt(index, i);
		}
	}

	public Generator MakeGenerator(string salt, int additionalSalt = 0)
	{
		return new Generator(Next(salt, additionalSalt).Value);
	}

	public Generator MakeGenerator()
	{
		return new Generator(Value);
	}
}
