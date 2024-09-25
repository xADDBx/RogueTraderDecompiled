using System;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

public struct KeyValuePairComparable<K, V> : IComparable<KeyValuePairComparable<K, V>> where K : struct, IComparable<K> where V : struct
{
	public K Key { get; private set; }

	public V Value { get; private set; }

	public KeyValuePairComparable(K key, V value)
	{
		Key = key;
		Value = value;
	}

	public int CompareTo(KeyValuePairComparable<K, V> other)
	{
		return Key.CompareTo(other.Key);
	}

	public override string ToString()
	{
		return $"Key: {Key} Value: {Value}";
	}
}
