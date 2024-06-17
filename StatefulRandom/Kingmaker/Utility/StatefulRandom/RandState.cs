using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility.StatefulRandom;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public struct RandState : IMemoryPackable<RandState>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class RandStateFormatter : MemoryPackFormatter<RandState>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RandState value)
		{
			RandState.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RandState value)
		{
			RandState.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public uint x;

	[JsonProperty]
	public uint y;

	[JsonProperty]
	public uint z;

	[JsonProperty]
	public uint w;

	public static readonly RandState Default;

	[MemoryPackIgnore]
	public bool IsReady
	{
		get
		{
			if (x == 0 && y == 0 && z == 0)
			{
				return w != 0;
			}
			return true;
		}
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
	}

	static RandState()
	{
		Default = new Rand(0u).State;
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RandState>())
		{
			MemoryPackFormatterProvider.Register(new RandStateFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RandState[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RandState>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RandState value)
	{
		writer.WriteUnmanaged(in value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref RandState value)
	{
		reader.ReadUnmanaged<RandState>(out value);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref x);
		result.Append(ref y);
		result.Append(ref z);
		result.Append(ref w);
		return result;
	}
}
