using Kingmaker.ElementsSystem.ContextData;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility.StatefulRandom;

[MemoryPackable(GenerateType.Object)]
public class Rand : IMemoryPackable<Rand>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class RandFormatter : MemoryPackFormatter<Rand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref Rand value)
		{
			Rand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref Rand value)
		{
			Rand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public RandState State;

	[MemoryPackIgnore]
	public uint Seed
	{
		get
		{
			return State.x;
		}
		set
		{
			State.x = value;
			State.y = State.x * 1812433253 + 1;
			State.z = State.y * 1812433253 + 1;
			State.w = State.z * 1812433253 + 1;
		}
	}

	[MemoryPackConstructor]
	public Rand()
		: this(0u)
	{
	}

	public Rand(uint seed)
	{
		Seed = seed;
	}

	public uint Get()
	{
		if ((bool)ContextData<DisableStatefulRandomContext>.Current)
		{
			return (uint)Random.Range(0, int.MaxValue);
		}
		uint num = State.x ^ (State.x << 11);
		State.x = State.y;
		State.y = State.z;
		State.z = State.w;
		return State.w = State.w ^ (State.w >> 19) ^ (num ^ (num >> 8));
	}

	public ulong Get64()
	{
		long num = Get();
		ulong num2 = Get();
		return (ulong)(num << 32) | num2;
	}

	public static float GetFloatFromInt(uint value)
	{
		return (float)(value & 0x7FFFFFu) * 1.192093E-07f;
	}

	public float RangedRandom(float inclusiveMin, float inclusiveMax)
	{
		float @float = GetFloat();
		return inclusiveMin * @float + (1f - @float) * inclusiveMax;
	}

	public int RangedRandom(int inclusiveMin, int exclusiveMax)
	{
		if (inclusiveMin < exclusiveMax)
		{
			int num = exclusiveMax - inclusiveMin;
			return (int)(Get() % num) + inclusiveMin;
		}
		if (exclusiveMax < inclusiveMin)
		{
			int num2 = inclusiveMin - exclusiveMax;
			int num3 = (int)(Get() % num2);
			return inclusiveMin - num3;
		}
		return inclusiveMin;
	}

	public float GetFloat()
	{
		return GetFloatFromInt(Get());
	}

	public float GetSignedFloat()
	{
		return GetFloat() * 2f - 1f;
	}

	static Rand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<Rand>())
		{
			MemoryPackFormatterProvider.Register(new RandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Rand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<Rand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref Rand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.State);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref Rand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		RandState value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.State;
				reader.ReadUnmanaged<RandState>(out value2);
				goto IL_0071;
			}
			reader.ReadUnmanaged<RandState>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(Rand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.State : default(RandState));
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<RandState>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0071;
			}
		}
		value = new Rand
		{
			State = value2
		};
		return;
		IL_0071:
		value.State = value2;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref State);
		return result;
	}
}
