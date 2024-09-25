using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[JsonObject(IsReference = false)]
public struct EntityFactRef : IHashable
{
	private EntityServiceProxy m_Proxy;

	[JsonProperty]
	public readonly string EntityId;

	[JsonProperty]
	public readonly string FactId;

	private WeakReference<EntityFact> m_FactCache;

	public bool IsEmpty => string.IsNullOrEmpty(EntityId);

	[CanBeNull]
	public Entity Entity
	{
		get
		{
			if (!IsEmpty && (m_Proxy == null || m_Proxy.IsDisposed))
			{
				m_Proxy = EntityService.Instance?.GetProxy(EntityId);
			}
			return (Entity)(m_Proxy?.Entity);
		}
	}

	[CanBeNull]
	public EntityFact Fact
	{
		get
		{
			if (IsEmpty)
			{
				return null;
			}
			if (m_FactCache == null)
			{
				m_FactCache = new WeakReference<EntityFact>(null);
			}
			if (m_FactCache.TryGetTarget(out var target))
			{
				return target;
			}
			EntityFact entityFact = Entity?.Facts.FindById(FactId);
			m_FactCache.SetTarget(entityFact);
			return entityFact;
		}
	}

	public EntityFactRef([CanBeNull] string entityId, [CanBeNull] string factId)
	{
		EntityId = entityId;
		FactId = factId;
		m_Proxy = null;
		m_FactCache = new WeakReference<EntityFact>(null);
	}

	public EntityFactRef([CanBeNull] EntityFact fact)
	{
		if (fact?.Owner != null)
		{
			EntityId = fact.Owner.UniqueId;
			FactId = fact.UniqueId;
			m_Proxy = fact.Owner.Proxy;
			m_FactCache = new WeakReference<EntityFact>(null);
		}
		else
		{
			EntityId = null;
			FactId = null;
			m_Proxy = null;
			m_FactCache = new WeakReference<EntityFact>(null);
		}
	}

	public readonly bool Equals(EntityFactRef other)
	{
		if (string.Equals(EntityId, other.EntityId))
		{
			return string.Equals(FactId, other.FactId);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is EntityFactRef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (EntityId == null)
		{
			return 0;
		}
		return EntityId.GetHashCode();
	}

	public static implicit operator EntityFactRef([CanBeNull] EntityFact fact)
	{
		return new EntityFactRef(fact);
	}

	public static bool operator ==(EntityFactRef r, [CanBeNull] EntityFact fact)
	{
		if (r.EntityId == fact?.Owner?.UniqueId)
		{
			return r.FactId == fact?.UniqueId;
		}
		return false;
	}

	public static bool operator !=(EntityFactRef r, [CanBeNull] EntityFact fact)
	{
		if (!(r.EntityId != fact?.Owner?.UniqueId))
		{
			return r.FactId != fact?.UniqueId;
		}
		return true;
	}

	public static implicit operator EntityFact(EntityFactRef @ref)
	{
		return @ref.Fact;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(EntityId);
		result.Append(FactId);
		return result;
	}
}
[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public struct EntityFactRef<T> : IMemoryPackable<EntityFactRef<T>>, IMemoryPackFormatterRegister, IHashable where T : EntityFact
{
	[Preserve]
	private sealed class EntityFactRefFormatter : MemoryPackFormatter<EntityFactRef<T>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EntityFactRef<T> value)
		{
			EntityFactRef<T>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EntityFactRef<T> value)
		{
			EntityFactRef<T>.Deserialize(ref reader, ref value);
		}
	}

	private EntityServiceProxy m_Proxy;

	[JsonProperty]
	public readonly string EntityId;

	[JsonProperty]
	public readonly string FactId;

	private WeakReference<T> m_FactCache;

	[MemoryPackIgnore]
	public bool IsEmpty
	{
		get
		{
			if (!string.IsNullOrEmpty(EntityId))
			{
				return string.IsNullOrEmpty(FactId);
			}
			return true;
		}
	}

	[CanBeNull]
	[MemoryPackIgnore]
	public Entity Entity
	{
		get
		{
			if (!IsEmpty && (m_Proxy == null || m_Proxy.IsDisposed))
			{
				m_Proxy = EntityService.Instance?.GetProxy(EntityId);
			}
			return (Entity)(m_Proxy?.Entity);
		}
	}

	[MemoryPackIgnore]
	[CanBeNull]
	public T Fact
	{
		get
		{
			if (IsEmpty)
			{
				return null;
			}
			if (m_FactCache == null)
			{
				m_FactCache = new WeakReference<T>(null);
			}
			if (m_FactCache.TryGetTarget(out var target))
			{
				return target;
			}
			T val = Entity?.Facts.FindById(FactId) as T;
			m_FactCache.SetTarget(val);
			return val;
		}
	}

	[MemoryPackConstructor]
	public EntityFactRef([CanBeNull] string entityId, [CanBeNull] string factId)
	{
		EntityId = entityId;
		FactId = factId;
		m_Proxy = null;
		m_FactCache = new WeakReference<T>(null);
	}

	public EntityFactRef([CanBeNull] T fact)
	{
		if (fact?.Owner != null)
		{
			EntityId = fact.Owner.UniqueId;
			FactId = fact.UniqueId;
			m_Proxy = fact.Owner.Proxy;
			m_FactCache = new WeakReference<T>(null);
		}
		else
		{
			EntityId = null;
			FactId = null;
			m_Proxy = null;
			m_FactCache = new WeakReference<T>(null);
		}
	}

	public bool Equals(EntityFactRef<T> other)
	{
		if (string.Equals(EntityId, other.EntityId))
		{
			return string.Equals(FactId, other.FactId);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is EntityFactRef<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (EntityId == null)
		{
			return 0;
		}
		return EntityId.GetHashCode();
	}

	public static implicit operator EntityFactRef<T>([CanBeNull] T fact)
	{
		return new EntityFactRef<T>(fact);
	}

	public static bool operator ==(EntityFactRef<T> r, [CanBeNull] T fact)
	{
		if (r.EntityId == fact?.Owner?.UniqueId)
		{
			return r.FactId == fact?.UniqueId;
		}
		return false;
	}

	public static bool operator !=(EntityFactRef<T> r, [CanBeNull] T fact)
	{
		if (!(r.EntityId != fact?.Owner?.UniqueId))
		{
			return r.FactId != fact?.UniqueId;
		}
		return true;
	}

	public static implicit operator T(EntityFactRef<T> @ref)
	{
		return @ref.Fact;
	}

	static EntityFactRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EntityFactRef<T>>())
		{
			MemoryPackFormatterProvider.Register(new EntityFactRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityFactRef<T>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityFactRef<T>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EntityFactRef<T> value)
	{
		writer.WriteObjectHeader(2);
		writer.WriteString(value.EntityId);
		writer.WriteString(value.FactId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EntityFactRef<T> value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(EntityFactRef<T>);
			return;
		}
		string entityId;
		string factId;
		if (memberCount == 2)
		{
			entityId = reader.ReadString();
			factId = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EntityFactRef<T>), 2, memberCount);
				return;
			}
			entityId = null;
			factId = null;
			if (memberCount != 0)
			{
				entityId = reader.ReadString();
				if (memberCount != 1)
				{
					factId = reader.ReadString();
					_ = 2;
				}
			}
		}
		value = new EntityFactRef<T>(entityId, factId);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(EntityId);
		result.Append(FactId);
		return result;
	}
}
