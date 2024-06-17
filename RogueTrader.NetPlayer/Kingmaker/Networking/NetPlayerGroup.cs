using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public readonly struct NetPlayerGroup : IEquatable<NetPlayerGroup>, IMemoryPackable<NetPlayerGroup>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class NetPlayerGroupFormatter : MemoryPackFormatter<NetPlayerGroup>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref NetPlayerGroup value)
		{
			NetPlayerGroup.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref NetPlayerGroup value)
		{
			NetPlayerGroup.Deserialize(ref reader, ref value);
		}
	}

	public const byte MinPlayerIndex = 1;

	public const byte MaxPlayerIndex = 6;

	private const byte AllOnes = byte.MaxValue;

	public static readonly NetPlayerGroup Empty;

	public static readonly NetPlayerGroup All;

	public static readonly NetPlayerGroup Offline;

	[JsonProperty(PropertyName = "v")]
	[MemoryPackInclude]
	private readonly byte m_Value;

	[MemoryPackIgnore]
	public bool IsEmpty => m_Value == 0;

	private NetPlayerGroup(byte value)
	{
		m_Value = value;
	}

	public NetPlayerGroup(NetPlayer player)
	{
		m_Value = (byte)player.Mask;
	}

	public NetPlayerGroup Add(NetPlayer player)
	{
		return new NetPlayerGroup((byte)(m_Value | player.Mask));
	}

	public NetPlayerGroup Del(NetPlayer player)
	{
		return new NetPlayerGroup((byte)(m_Value & ~player.Mask));
	}

	public NetPlayerGroup Del(NetPlayerGroup playerGroup)
	{
		return new NetPlayerGroup((byte)(m_Value & ~playerGroup.m_Value));
	}

	public NetPlayerGroup Intersection(NetPlayerGroup playerGroup)
	{
		return new NetPlayerGroup((byte)(m_Value & playerGroup.m_Value));
	}

	public bool Contains(NetPlayer player)
	{
		if (!player.IsEmpty)
		{
			return (m_Value & player.Mask) == player.Mask;
		}
		return false;
	}

	public bool Contains(NetPlayerGroup playerGroup)
	{
		return (m_Value & playerGroup.m_Value) == playerGroup.m_Value;
	}

	public override string ToString()
	{
		return Convert.ToString(m_Value, 2);
	}

	public bool Equals(NetPlayerGroup other)
	{
		return m_Value == other.m_Value;
	}

	public override bool Equals(object obj)
	{
		if (obj is NetPlayerGroup other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		byte value = m_Value;
		return value.GetHashCode();
	}

	public int Count()
	{
		int num = 0;
		for (int num2 = Del(NetPlayer.Offline).m_Value; num2 != 0; num2 &= num2 - 1)
		{
			num++;
		}
		return num;
	}

	static NetPlayerGroup()
	{
		Empty = default(NetPlayerGroup);
		All = new NetPlayerGroup(byte.MaxValue);
		Offline = new NetPlayerGroup(NetPlayer.Offline);
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<NetPlayerGroup>())
		{
			MemoryPackFormatterProvider.Register(new NetPlayerGroupFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<NetPlayerGroup[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<NetPlayerGroup>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref NetPlayerGroup value)
	{
		writer.WriteUnmanaged(in value);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref NetPlayerGroup value)
	{
		reader.ReadUnmanaged<NetPlayerGroup>(out value);
	}
}
