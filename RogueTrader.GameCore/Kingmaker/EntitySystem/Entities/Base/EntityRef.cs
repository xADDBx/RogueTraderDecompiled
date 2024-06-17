using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[HashRoot]
[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public struct EntityRef : IEquatable<EntityRef>, IEntityRef, IMemoryPackable<EntityRef>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class EntityRefFormatter : MemoryPackFormatter<EntityRef>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EntityRef value)
		{
			EntityRef.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EntityRef value)
		{
			EntityRef.Deserialize(ref reader, ref value);
		}
	}

	private EntityServiceProxy m_Proxy;

	[MemoryPackInclude]
	public readonly string Id;

	[MemoryPackIgnore]
	string IEntityRef.Id => Id;

	[MemoryPackIgnore]
	public bool IsEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(Id))
			{
				return m_Proxy == null;
			}
			return false;
		}
	}

	[CanBeNull]
	[MemoryPackIgnore]
	public IEntity Entity
	{
		get
		{
			if ((m_Proxy == null || m_Proxy.IsDisposed) && !IsEmpty)
			{
				m_Proxy = EntityService.Instance?.GetProxy(Id);
			}
			return m_Proxy?.Entity;
		}
	}

	[MemoryPackConstructor]
	public EntityRef([CanBeNull] string id)
	{
		Id = id;
		m_Proxy = (string.IsNullOrEmpty(id) ? null : EntityService.Instance?.GetProxy(Id));
	}

	public EntityRef([CanBeNull] IEntity entity)
	{
		Id = entity?.UniqueId;
		m_Proxy = entity?.Proxy;
	}

	[CanBeNull]
	public T Get<T>() where T : class, IEntity
	{
		return Entity as T;
	}

	[CanBeNull]
	public IEntity Get()
	{
		return Entity;
	}

	public readonly bool Equals(EntityRef other)
	{
		return string.Equals(Id, other.Id, StringComparison.Ordinal);
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityRef other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Id == null)
		{
			return 0;
		}
		return Id.GetHashCode();
	}

	public static bool operator ==(EntityRef r, [CanBeNull] IEntity entity)
	{
		return string.Equals(r.Id, entity?.UniqueId, StringComparison.Ordinal);
	}

	public static bool operator !=(EntityRef r, [CanBeNull] IEntity entity)
	{
		return !string.Equals(r.Id, entity?.UniqueId, StringComparison.Ordinal);
	}

	public static bool operator ==(EntityRef r1, EntityRef r2)
	{
		return string.Equals(r1.Id, r2.Id, StringComparison.Ordinal);
	}

	public static bool operator !=(EntityRef r1, EntityRef r2)
	{
		return !string.Equals(r1.Id, r2.Id, StringComparison.Ordinal);
	}

	static EntityRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EntityRef>())
		{
			MemoryPackFormatterProvider.Register(new EntityRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityRef[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityRef>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EntityRef value)
	{
		writer.WriteObjectHeader(1);
		writer.WriteString(value.Id);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EntityRef value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(EntityRef);
			return;
		}
		string id;
		if (memberCount == 1)
		{
			id = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EntityRef), 1, memberCount);
				return;
			}
			id = null;
			if (memberCount != 0)
			{
				id = reader.ReadString();
				_ = 1;
			}
		}
		value = new EntityRef(id);
	}

	public Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
[HashRoot]
[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public struct EntityRef<T> : IEntityRef, ITypedEntityRef, IEquatable<EntityRef<T>>, IMemoryPackable<EntityRef<T>>, IMemoryPackFormatterRegister, IHashable where T : class, IEntity
{
	[Preserve]
	private sealed class EntityRefFormatter : MemoryPackFormatter<EntityRef<T>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EntityRef<T> value)
		{
			EntityRef<T>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EntityRef<T> value)
		{
			EntityRef<T>.Deserialize(ref reader, ref value);
		}
	}

	private EntityServiceProxy m_Proxy;

	[JsonProperty]
	public readonly string Id;

	[MemoryPackIgnore]
	string IEntityRef.Id => Id;

	[MemoryPackIgnore]
	public bool IsNull => string.IsNullOrEmpty(Id);

	[MemoryPackIgnore]
	[CanBeNull]
	public T Entity
	{
		get
		{
			if (!IsNull && (m_Proxy == null || m_Proxy.IsDisposed))
			{
				m_Proxy = EntityService.Instance?.GetProxy(Id);
			}
			return m_Proxy?.Entity as T;
		}
	}

	[MemoryPackConstructor]
	public EntityRef([CanBeNull] string id)
	{
		Id = id;
		m_Proxy = (string.IsNullOrEmpty(id) ? null : EntityService.Instance?.GetProxy(id));
	}

	public EntityRef([CanBeNull] T entity)
	{
		Id = entity?.UniqueId;
		m_Proxy = entity?.Proxy;
	}

	[CanBeNull]
	public TEntity Get<TEntity>() where TEntity : class, IEntity
	{
		return Entity as TEntity;
	}

	[CanBeNull]
	public IEntity Get()
	{
		return Entity;
	}

	public bool Equals(EntityRef<T> other)
	{
		return string.Equals(Id, other.Id, StringComparison.Ordinal);
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityRef<T> other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id?.GetHashCode() ?? 0;
	}

	public string GetId()
	{
		return Id;
	}

	public static implicit operator EntityRef(EntityRef<T> @ref)
	{
		return new EntityRef(@ref.Id);
	}

	public static implicit operator EntityRef<T>([CanBeNull] T entity)
	{
		return new EntityRef<T>(entity);
	}

	public static bool operator ==(EntityRef<T> r, [CanBeNull] T entity)
	{
		return string.Equals(r.Id, entity?.UniqueId, StringComparison.Ordinal);
	}

	public static bool operator !=(EntityRef<T> r, [CanBeNull] T entity)
	{
		return !string.Equals(r.Id, entity?.UniqueId, StringComparison.Ordinal);
	}

	public static implicit operator T(EntityRef<T> @ref)
	{
		return @ref.Entity;
	}

	static EntityRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EntityRef<T>>())
		{
			MemoryPackFormatterProvider.Register(new EntityRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityRef<T>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityRef<T>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EntityRef<T> value)
	{
		writer.WriteObjectHeader(1);
		writer.WriteString(value.Id);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EntityRef<T> value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(EntityRef<T>);
			return;
		}
		string id;
		if (memberCount == 1)
		{
			id = reader.ReadString();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EntityRef<T>), 1, memberCount);
				return;
			}
			id = null;
			if (memberCount != 0)
			{
				id = reader.ReadString();
				_ = 1;
			}
		}
		value = new EntityRef<T>(id);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(Id);
		return result;
	}
}
