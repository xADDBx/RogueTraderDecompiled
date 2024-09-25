using System;
using System.Diagnostics;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Utility.StatefulRandom;

[MemoryPackable(GenerateType.Object)]
[HashRoot]
public class StatefulRandom : IMemoryPackable<StatefulRandom>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class StatefulRandomFormatter : MemoryPackFormatter<StatefulRandom>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StatefulRandom value)
		{
			StatefulRandom.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StatefulRandom value)
		{
			StatefulRandom.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public readonly string Name;

	[JsonProperty(IsReference = false)]
	public readonly Rand Rand = new Rand();

	[MemoryPackIgnore]
	public RandState State
	{
		get
		{
			return Rand.State;
		}
		set
		{
			Rand.State = value;
		}
	}

	[MemoryPackIgnore]
	public int Sign
	{
		get
		{
			if (Range(0, 2) <= 0)
			{
				return -1;
			}
			return 1;
		}
	}

	[MemoryPackIgnore]
	public bool YesOrNo => Range(0, 2) > 0;

	[MemoryPackIgnore]
	public float value => Rand.GetFloat();

	[MemoryPackIgnore]
	public Vector3 insideUnitSphere => RandomUnitVector(Rand) * Mathf.Pow(Rand.GetFloat(), 1f / 3f);

	[MemoryPackIgnore]
	public Vector2 insideUnitCircle => RandomUnitVector2(Rand) * Mathf.Sqrt(Rand.RangedRandom(0f, 1f));

	[MemoryPackIgnore]
	public Vector3 onUnitSphere => RandomUnitVector(Rand);

	[MemoryPackIgnore]
	public uint uintValue => Rand.Get();

	[MemoryPackConstructor]
	private StatefulRandom(string name, Rand rand)
	{
		Name = name;
		Rand = rand;
	}

	public StatefulRandom(string name, uint seed = 0u)
	{
		Name = name;
		Seed(seed);
	}

	public void Seed(uint seed)
	{
		Rand.Seed = seed;
	}

	[Conditional("ALWAYS_FALSE")]
	private void DebugCollect()
	{
		CheckInit();
	}

	private void CheckInit()
	{
		if (!State.IsReady)
		{
			throw new Exception("Trying to use RND '" + Name + "' that has not been initialized!");
		}
	}

	public float Range(float minInclusive, float maxInclusive)
	{
		return Rand.RangedRandom(minInclusive, maxInclusive);
	}

	public int Range(int minInclusive, int maxExclusive)
	{
		return Rand.RangedRandom(minInclusive, maxExclusive);
	}

	private static Vector3 RandomUnitVector(Rand rand)
	{
		float num = rand.RangedRandom(-1f, 1f);
		float f = rand.RangedRandom(0f, MathF.PI * 2f);
		float num2 = Mathf.Sqrt(1f - num * num);
		float x = num2 * Mathf.Cos(f);
		float y = num2 * Mathf.Sin(f);
		return new Vector3(x, y, num);
	}

	private static Vector2 RandomUnitVector2(Rand rand)
	{
		float f = rand.RangedRandom(0f, MathF.PI * 2f);
		float x = Mathf.Cos(f);
		float y = Mathf.Sin(f);
		return new Vector2(x, y);
	}

	static StatefulRandom()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StatefulRandom>())
		{
			MemoryPackFormatterProvider.Register(new StatefulRandomFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StatefulRandom[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StatefulRandom>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StatefulRandom? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WriteString(value.Name);
		writer.WritePackable(in value.Rand);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StatefulRandom? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Rand rand;
		string name;
		if (memberCount == 2)
		{
			if (value == null)
			{
				name = reader.ReadString();
				rand = reader.ReadPackable<Rand>();
			}
			else
			{
				name = value.Name;
				rand = value.Rand;
				name = reader.ReadString();
				reader.ReadPackable(ref rand);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StatefulRandom), 2, memberCount);
				return;
			}
			if (value == null)
			{
				name = null;
				rand = null;
			}
			else
			{
				name = value.Name;
				rand = value.Rand;
			}
			if (memberCount != 0)
			{
				name = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadPackable(ref rand);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new StatefulRandom(name, rand);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(Name);
		Hash128 val = ClassHasher<Rand>.GetHash128(Rand);
		result.Append(ref val);
		return result;
	}
}
