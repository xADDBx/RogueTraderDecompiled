using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.UnityExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
[HashRoot]
public struct UnitReference : IEntityRef, IEquatable<UnitReference>, IComparable<UnitReference>, IMemoryPackable<UnitReference>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class UnitReferenceFormatter : MemoryPackFormatter<UnitReference>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitReference value)
		{
			UnitReference.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitReference value)
		{
			UnitReference.Deserialize(ref reader, ref value);
		}
	}

	public static readonly UnitReference NullUnitReference;

	[MemoryPackInclude]
	private readonly string m_UniqueId;

	private EntityServiceProxy m_Proxy;

	[MemoryPackIgnore]
	public string Id => m_UniqueId;

	[MemoryPackIgnore]
	public IAbstractUnitEntity Entity
	{
		get
		{
			if ((m_Proxy == null || m_Proxy.IsDisposed) && !m_UniqueId.IsNullOrEmpty())
			{
				m_Proxy = EntityService.Instance?.GetProxy(m_UniqueId);
			}
			return m_Proxy?.Entity as IAbstractUnitEntity;
		}
	}

	[MemoryPackConstructor]
	public UnitReference([CanBeNull] string m_uniqueId)
	{
		m_UniqueId = m_uniqueId;
		m_Proxy = (string.IsNullOrEmpty(m_uniqueId) ? null : EntityService.Instance?.GetProxy(m_uniqueId));
	}

	private UnitReference([CanBeNull] IAbstractUnitEntity unit)
	{
		m_UniqueId = unit?.UniqueId;
		m_Proxy = unit?.Proxy;
	}

	[CanBeNull]
	public T Get<T>() where T : class, IEntity
	{
		return (T)Entity;
	}

	[CanBeNull]
	public IEntity Get()
	{
		return Entity;
	}

	public bool IsNull()
	{
		return Entity == null;
	}

	public bool Equals(UnitReference other)
	{
		return string.Equals(Id, other.Id, StringComparison.Ordinal);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is UnitReference other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (m_UniqueId == null)
		{
			return 0;
		}
		return m_UniqueId.GetHashCode();
	}

	public IAbstractUnitEntity ToIAbstractUnitEntity()
	{
		return Entity;
	}

	public static IAbstractUnitEntity ToIAbstractUnitEntity(UnitReference r)
	{
		if (!(r == null))
		{
			return r.Entity;
		}
		return null;
	}

	public static UnitReference FromIAbstractUnitEntity(IAbstractUnitEntity unit)
	{
		return new UnitReference(unit);
	}

	public static bool operator ==(UnitReference r, UnitReference l)
	{
		return string.Equals(r.Id, l.Id);
	}

	public static bool operator !=(UnitReference r, UnitReference l)
	{
		return !string.Equals(r.Id, l.Id);
	}

	public static bool operator ==(UnitReference r, [CanBeNull] IAbstractUnitEntity unit)
	{
		return string.Equals(r.Id, unit?.UniqueId, StringComparison.Ordinal);
	}

	public static bool operator !=(UnitReference r, [CanBeNull] IAbstractUnitEntity unit)
	{
		return !string.Equals(r.Id, unit?.UniqueId, StringComparison.Ordinal);
	}

	public override string ToString()
	{
		if (Entity == null)
		{
			if (m_UniqueId == null)
			{
				return "<null>";
			}
			return "[Not found] " + m_UniqueId;
		}
		return Entity.CharacterName;
	}

	public int CompareTo(UnitReference other)
	{
		return string.Compare(m_UniqueId, other.m_UniqueId, StringComparison.Ordinal);
	}

	static UnitReference()
	{
		NullUnitReference = new UnitReference((string)null);
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitReference>())
		{
			MemoryPackFormatterProvider.Register(new UnitReferenceFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitReference[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UnitReference value)
	{
		writer.WriteObjectHeader(1);
		writer.WriteString(value.m_UniqueId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UnitReference value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(UnitReference);
			return;
		}
		string uniqueId;
		if (memberCount == 1)
		{
			uniqueId = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UnitReference), 1, memberCount);
				return;
			}
			uniqueId = null;
			if (memberCount != 0)
			{
				uniqueId = reader.ReadString();
				_ = 1;
			}
		}
		value = new UnitReference(uniqueId);
	}

	public Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
