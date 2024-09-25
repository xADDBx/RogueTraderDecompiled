using System;
using JetBrains.Annotations;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[HashRoot]
[MemoryPackable(GenerateType.Object)]
public struct BlueprintComponentReference<T> : IEquatable<BlueprintComponentReference<T>>, IComparable<BlueprintComponentReference<T>>, IMemoryPackable<BlueprintComponentReference<T>>, IMemoryPackFormatterRegister, IHashable where T : BlueprintComponent
{
	[Preserve]
	private sealed class BlueprintComponentReferenceFormatter : MemoryPackFormatter<BlueprintComponentReference<T>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref BlueprintComponentReference<T> value)
		{
			BlueprintComponentReference<T>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref BlueprintComponentReference<T> value)
		{
			BlueprintComponentReference<T>.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	private BlueprintScriptableObject Blueprint;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	private string ComponentName;

	[MemoryPackConstructor]
	public BlueprintComponentReference(BlueprintScriptableObject blueprint, string componentName)
	{
		Blueprint = blueprint;
		ComponentName = componentName;
	}

	public BlueprintComponentReference([CanBeNull] T trigger)
	{
		Blueprint = trigger?.OwnerBlueprint;
		ComponentName = trigger?.name;
	}

	public T Get()
	{
		BlueprintComponentReference<T> d = this;
		if (d.Blueprint != null && !d.ComponentName.IsNullOrEmpty())
		{
			return d.Blueprint.ComponentsArray.FindOrDefault((BlueprintComponent c) => c.name == d.ComponentName) as T;
		}
		return null;
	}

	public bool Equals(BlueprintComponentReference<T> other)
	{
		if (Blueprint == other.Blueprint)
		{
			return string.Equals(ComponentName, other.ComponentName, StringComparison.Ordinal);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is BlueprintComponentReference<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(BlueprintComponentReference<T> lhs, [CanBeNull] BlueprintComponentReference<T> rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(BlueprintComponentReference<T> lhs, [CanBeNull] BlueprintComponentReference<T> rhs)
	{
		return !lhs.Equals(rhs);
	}

	public int CompareTo(BlueprintComponentReference<T> other)
	{
		throw new NotImplementedException();
	}

	public static implicit operator T(BlueprintComponentReference<T> reference)
	{
		if (!(reference == null))
		{
			return reference.Get();
		}
		return null;
	}

	public static implicit operator BlueprintComponentReference<T>(T component)
	{
		return new BlueprintComponentReference<T>(component);
	}

	public override string ToString()
	{
		if (!(this != null))
		{
			return "<null>";
		}
		return $"[{Blueprint}]{ComponentName}";
	}

	public override int GetHashCode()
	{
		if (!(this == null))
		{
			return ((ValueType)this).GetHashCode();
		}
		return 0;
	}

	static BlueprintComponentReference()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintComponentReference<T>>())
		{
			MemoryPackFormatterProvider.Register(new BlueprintComponentReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<BlueprintComponentReference<T>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<BlueprintComponentReference<T>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref BlueprintComponentReference<T> value)
	{
		writer.WriteObjectHeader(2);
		writer.WriteValue(in value.Blueprint);
		writer.WriteString(value.ComponentName);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref BlueprintComponentReference<T> value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(BlueprintComponentReference<T>);
			return;
		}
		BlueprintScriptableObject value2;
		string componentName;
		if (memberCount == 2)
		{
			value2 = reader.ReadValue<BlueprintScriptableObject>();
			componentName = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(BlueprintComponentReference<T>), 2, memberCount);
				return;
			}
			value2 = null;
			componentName = null;
			if (memberCount != 0)
			{
				reader.ReadValue(ref value2);
				if (memberCount != 1)
				{
					componentName = reader.ReadString();
					_ = 2;
				}
			}
		}
		value = new BlueprintComponentReference<T>(value2, componentName);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		result.Append(ComponentName);
		return result;
	}
}
