using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Utility;

[MemoryPackable(GenerateType.Object)]
public class RandomWeightsForSave<T> : IMemoryPackable<RandomWeightsForSave<T>>, IMemoryPackFormatterRegister, IHashable where T : BlueprintReferenceBase
{
	[Preserve]
	private sealed class RandomWeightsForSaveFormatter : MemoryPackFormatter<RandomWeightsForSave<T>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RandomWeightsForSave<T> value)
		{
			RandomWeightsForSave<T>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RandomWeightsForSave<T> value)
		{
			RandomWeightsForSave<T>.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	private List<WeightPair<T>> m_InitialWeights = new List<WeightPair<T>>();

	[JsonProperty]
	private List<WeightPair<T>> m_CurrentWeights = new List<WeightPair<T>>();

	public RandomWeightsForSave(RandomWeights<T> blueprint)
	{
		WeightPair<T>[] weights = blueprint.Weights;
		foreach (WeightPair<T> weightPair in weights)
		{
			m_InitialWeights.Add(new WeightPair<T>(weightPair.Object, weightPair.Weight));
			m_CurrentWeights.Add(new WeightPair<T>(weightPair.Object, weightPair.Weight));
		}
	}

	public RandomWeightsForSave(JsonConstructorMark _)
	{
	}

	[MemoryPackConstructor]
	public RandomWeightsForSave()
	{
	}

	private int WeightsSum(List<WeightPair<T>> weights)
	{
		int num = 0;
		foreach (WeightPair<T> weight in weights)
		{
			num += weight.Weight;
		}
		return num;
	}

	public int WeightsSum()
	{
		return WeightsSum(m_CurrentWeights);
	}

	[CanBeNull]
	public T GetRandomObject(PersistentRandom.Generator generator)
	{
		return GetRandomObject(generator, m_CurrentWeights);
	}

	[CanBeNull]
	private T GetRandomObject(PersistentRandom.Generator generator, List<WeightPair<T>> weights)
	{
		int num = generator.NextRange(1, WeightsSum(weights));
		int num2 = 0;
		foreach (WeightPair<T> weight in weights)
		{
			if (num2 <= num && num < num2 + weight.Weight)
			{
				T @object = weight.Object;
				RecalculateWeights(@object);
				return @object;
			}
			num2 += weight.Weight;
		}
		if (num2 == num)
		{
			RecalculateWeights(weights[weights.Count - 1].Object);
			return weights[weights.Count - 1].Object;
		}
		RecalculateAllWeights();
		return null;
	}

	[CanBeNull]
	public T GetRandomObjectExcept(PersistentRandom.Generator generator, List<T> except)
	{
		List<WeightPair<T>> weights = m_CurrentWeights.Where((WeightPair<T> obj) => !except.Contains(obj.Object)).ToList();
		return GetRandomObject(generator, weights);
	}

	private void RecalculateWeights(T chosenObject)
	{
		foreach (WeightPair<T> currentWeight in m_CurrentWeights)
		{
			if (currentWeight.Object != chosenObject)
			{
				currentWeight.Weight++;
			}
			else
			{
				currentWeight.Weight = 0;
			}
		}
	}

	private void RecalculateAllWeights()
	{
		foreach (WeightPair<T> currentWeight in m_CurrentWeights)
		{
			currentWeight.Weight++;
		}
	}

	public bool Empty()
	{
		return m_CurrentWeights.Empty();
	}

	public void RemoveObject(T obj)
	{
		WeightPair<T> weightPair = m_CurrentWeights.FirstOrDefault((WeightPair<T> pair) => pair.Object == obj);
		if (weightPair != null)
		{
			m_CurrentWeights.Remove(weightPair);
		}
	}

	public List<T> Keys()
	{
		return m_CurrentWeights.Select((WeightPair<T> pair) => pair.Object).ToList();
	}

	public void CopyCurrentWeights(RandomWeightsForSave<T> other)
	{
		foreach (WeightPair<T> obj in other.m_CurrentWeights)
		{
			m_CurrentWeights.First((WeightPair<T> o) => o.Object.Equals(obj.Object)).Weight = obj.Weight;
		}
	}

	static RandomWeightsForSave()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RandomWeightsForSave<T>>())
		{
			MemoryPackFormatterProvider.Register(new RandomWeightsForSaveFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RandomWeightsForSave<T>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RandomWeightsForSave<T>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RandomWeightsForSave<T>? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteObjectHeader(0);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RandomWeightsForSave<T>? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		if (memberCount == 0)
		{
			if (value != null)
			{
				return;
			}
		}
		else
		{
			if (memberCount > 0)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RandomWeightsForSave<T>), 0, memberCount);
				return;
			}
			_ = value;
			if (value != null)
			{
				return;
			}
		}
		value = new RandomWeightsForSave<T>();
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<WeightPair<T>> initialWeights = m_InitialWeights;
		if (initialWeights != null)
		{
			for (int i = 0; i < initialWeights.Count; i++)
			{
				Hash128 val = ClassHasher<WeightPair<T>>.GetHash128(initialWeights[i]);
				result.Append(ref val);
			}
		}
		List<WeightPair<T>> currentWeights = m_CurrentWeights;
		if (currentWeights != null)
		{
			for (int j = 0; j < currentWeights.Count; j++)
			{
				Hash128 val2 = ClassHasher<WeightPair<T>>.GetHash128(currentWeights[j]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
