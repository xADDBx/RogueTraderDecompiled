using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Utility.FlagCountable;

[MemoryPackable(GenerateType.Object)]
[HashRoot]
public class CountableFlag : IMemoryPackable<CountableFlag>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class CountableFlagFormatter : MemoryPackFormatter<CountableFlag>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CountableFlag value)
		{
			CountableFlag.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CountableFlag value)
		{
			CountableFlag.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private int m_Count;

	[MemoryPackIgnore]
	public int Count => m_Count;

	[MemoryPackIgnore]
	public bool Value => this;

	[MemoryPackConstructor]
	public CountableFlag()
	{
	}

	public void Retain()
	{
		m_Count++;
	}

	public void Release()
	{
		if (Application.isEditor && m_Count < 1)
		{
			PFLog.Default.Error("Can't release countable flag: no one retain it");
		}
		m_Count = Math.Max(0, m_Count - 1);
	}

	public void ReleaseAll()
	{
		m_Count = 0;
	}

	public static implicit operator bool(CountableFlag flag)
	{
		if (flag != null)
		{
			return flag.m_Count > 0;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{(bool)this}({m_Count})";
	}

	static CountableFlag()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CountableFlag>())
		{
			MemoryPackFormatterProvider.Register(new CountableFlagFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CountableFlag[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CountableFlag>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CountableFlag? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Count);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CountableFlag? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Count;
				reader.ReadUnmanaged<int>(out value2);
				goto IL_006b;
			}
			reader.ReadUnmanaged<int>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CountableFlag), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Count : 0);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new CountableFlag
		{
			m_Count = value2
		};
		return;
		IL_006b:
		value.m_Count = value2;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref m_Count);
		return result;
	}
}
