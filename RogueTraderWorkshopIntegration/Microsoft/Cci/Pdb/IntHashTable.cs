using System;

namespace Microsoft.Cci.Pdb;

internal class IntHashTable
{
	private struct bucket
	{
		internal int key;

		internal int hash_coll;

		internal object val;
	}

	private static readonly int[] primes = new int[72]
	{
		3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
		89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
		631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
		4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
		25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
		156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
		968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
		5999471, 7199369
	};

	private bucket[] buckets;

	private int count;

	private int occupancy;

	private int loadsize;

	private int loadFactorPerc;

	private int version;

	internal object this[int key]
	{
		get
		{
			if (key < 0)
			{
				throw new ArgumentException("Argument_KeyLessThanZero");
			}
			bucket[] array = buckets;
			uint seed;
			uint incr;
			uint num = InitHash(key, array.Length, out seed, out incr);
			int num2 = 0;
			bucket bucket;
			do
			{
				int num3 = (int)(seed % (uint)array.Length);
				bucket = array[num3];
				if (bucket.val == null)
				{
					return null;
				}
				if ((bucket.hash_coll & 0x7FFFFFFF) == num && key == bucket.key)
				{
					return bucket.val;
				}
				seed += incr;
			}
			while (bucket.hash_coll < 0 && ++num2 < array.Length);
			return null;
		}
	}

	private static int GetPrime(int minSize)
	{
		if (minSize < 0)
		{
			throw new ArgumentException("Arg_HTCapacityOverflow");
		}
		for (int i = 0; i < primes.Length; i++)
		{
			int num = primes[i];
			if (num >= minSize)
			{
				return num;
			}
		}
		throw new ArgumentException("Arg_HTCapacityOverflow");
	}

	internal IntHashTable()
		: this(0, 100)
	{
	}

	internal IntHashTable(int capacity, int loadFactorPerc)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", "ArgumentOutOfRange_NeedNonNegNum");
		}
		if (loadFactorPerc < 10 || loadFactorPerc > 100)
		{
			throw new ArgumentOutOfRangeException("loadFactorPerc", string.Format("ArgumentOutOfRange_IntHashTableLoadFactor", 10, 100));
		}
		this.loadFactorPerc = loadFactorPerc * 72 / 100;
		int prime = GetPrime(capacity / this.loadFactorPerc);
		buckets = new bucket[prime];
		loadsize = this.loadFactorPerc * prime / 100;
		if (loadsize >= prime)
		{
			loadsize = prime - 1;
		}
	}

	private static uint InitHash(int key, int hashsize, out uint seed, out uint incr)
	{
		uint result = (seed = (uint)key & 0x7FFFFFFFu);
		incr = 1 + ((seed >> 5) + 1) % (uint)(hashsize - 1);
		return result;
	}

	internal void Add(int key, object value)
	{
		Insert(key, value, add: true);
	}

	private void expand()
	{
		rehash(GetPrime(1 + buckets.Length * 2));
	}

	private void rehash()
	{
		rehash(buckets.Length);
	}

	private void rehash(int newsize)
	{
		occupancy = 0;
		bucket[] newBuckets = new bucket[newsize];
		for (int i = 0; i < buckets.Length; i++)
		{
			bucket bucket = buckets[i];
			if (bucket.val != null)
			{
				putEntry(newBuckets, bucket.key, bucket.val, bucket.hash_coll & 0x7FFFFFFF);
			}
		}
		version++;
		buckets = newBuckets;
		loadsize = loadFactorPerc * newsize / 100;
		if (loadsize >= newsize)
		{
			loadsize = newsize - 1;
		}
	}

	private void Insert(int key, object nvalue, bool add)
	{
		if (key < 0)
		{
			throw new ArgumentException("Argument_KeyLessThanZero");
		}
		if (nvalue == null)
		{
			throw new ArgumentNullException("nvalue", "ArgumentNull_Value");
		}
		if (count >= loadsize)
		{
			expand();
		}
		else if (occupancy > loadsize && count > 100)
		{
			rehash();
		}
		uint seed;
		uint incr;
		uint num = InitHash(key, buckets.Length, out seed, out incr);
		int num2 = 0;
		int num3 = -1;
		do
		{
			int num4 = (int)(seed % (uint)buckets.Length);
			if (buckets[num4].val == null)
			{
				if (num3 != -1)
				{
					num4 = num3;
				}
				buckets[num4].val = nvalue;
				buckets[num4].key = key;
				buckets[num4].hash_coll |= (int)num;
				count++;
				version++;
				return;
			}
			if ((buckets[num4].hash_coll & 0x7FFFFFFF) == num && key == buckets[num4].key)
			{
				if (add)
				{
					throw new ArgumentException("Argument_AddingDuplicate__" + buckets[num4].key);
				}
				buckets[num4].val = nvalue;
				version++;
				return;
			}
			if (num3 == -1 && buckets[num4].hash_coll >= 0)
			{
				buckets[num4].hash_coll |= int.MinValue;
				occupancy++;
			}
			seed += incr;
		}
		while (++num2 < buckets.Length);
		if (num3 != -1)
		{
			buckets[num3].val = nvalue;
			buckets[num3].key = key;
			buckets[num3].hash_coll |= (int)num;
			count++;
			version++;
			return;
		}
		throw new InvalidOperationException("InvalidOperation_HashInsertFailed");
	}

	private void putEntry(bucket[] newBuckets, int key, object nvalue, int hashcode)
	{
		uint num = (uint)hashcode;
		uint num2 = 1 + ((num >> 5) + 1) % (uint)(newBuckets.Length - 1);
		int num3;
		while (true)
		{
			num3 = (int)(num % (uint)newBuckets.Length);
			if (newBuckets[num3].val == null)
			{
				break;
			}
			if (newBuckets[num3].hash_coll >= 0)
			{
				newBuckets[num3].hash_coll |= int.MinValue;
				occupancy++;
			}
			num += num2;
		}
		newBuckets[num3].val = nvalue;
		newBuckets[num3].key = key;
		newBuckets[num3].hash_coll |= hashcode;
	}
}
