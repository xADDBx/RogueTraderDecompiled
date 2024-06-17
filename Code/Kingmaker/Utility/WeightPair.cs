using System;
using Kingmaker.Blueprints;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.Attributes;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility;

[Serializable]
[MemoryPackable(GenerateType.Object)]
public class WeightPair<T> : IMemoryPackable<WeightPair<T>>, IMemoryPackFormatterRegister, IHashable where T : BlueprintReferenceBase
{
	[Preserve]
	private sealed class WeightPairFormatter : MemoryPackFormatter<WeightPair<T>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref WeightPair<T> value)
		{
			WeightPair<T>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref WeightPair<T> value)
		{
			WeightPair<T>.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[HasherCustom(Type = typeof(Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher))]
	public T Object;

	[JsonProperty]
	[Weights]
	public int Weight;

	[JsonConstructor]
	[MemoryPackConstructor]
	public WeightPair()
	{
	}

	public WeightPair(T obj, int weight)
	{
		Object = obj;
		Weight = weight;
	}

	static WeightPair()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<WeightPair<T>>())
		{
			MemoryPackFormatterProvider.Register(new WeightPairFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<WeightPair<T>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<WeightPair<T>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref WeightPair<T>? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteValue(in value.Object);
		writer.WriteUnmanaged(in value.Weight);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref WeightPair<T>? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		T value2;
		int value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.Object;
				value3 = value.Weight;
				reader.ReadValue(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				goto IL_00a1;
			}
			value2 = reader.ReadValue<T>();
			reader.ReadUnmanaged<int>(out value3);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(WeightPair<T>), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
			}
			else
			{
				value2 = value.Object;
				value3 = value.Weight;
			}
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a1;
			}
		}
		value = new WeightPair<T>
		{
			Object = value2,
			Weight = value3
		};
		return;
		IL_00a1:
		value.Object = value2;
		value.Weight = value3;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(Object);
		result.Append(ref val);
		result.Append(ref Weight);
		return result;
	}
}
