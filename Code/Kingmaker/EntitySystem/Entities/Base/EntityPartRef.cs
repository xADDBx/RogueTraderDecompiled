using System;
using JetBrains.Annotations;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities.Base;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public struct EntityPartRef<TEntity, TPart> : IEquatable<EntityPartRef<TEntity, TPart>>, IMemoryPackable<EntityPartRef<TEntity, TPart>>, IMemoryPackFormatterRegister, IHashable where TEntity : Entity where TPart : EntityPart<TEntity>, new()
{
	[Preserve]
	private sealed class EntityPartRefFormatter : MemoryPackFormatter<EntityPartRef<TEntity, TPart>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EntityPartRef<TEntity, TPart> value)
		{
			EntityPartRef<TEntity, TPart>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EntityPartRef<TEntity, TPart> value)
		{
			EntityPartRef<TEntity, TPart>.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<TEntity> m_Ref;

	[MemoryPackIgnore]
	public bool IsNull => m_Ref.IsNull;

	[MemoryPackIgnore]
	public EntityRef<TEntity> EntityRef => m_Ref;

	[CanBeNull]
	[MemoryPackIgnore]
	public TEntity Entity => m_Ref.Entity;

	[CanBeNull]
	[MemoryPackIgnore]
	public TPart EntityPart
	{
		get
		{
			TEntity entity = Entity;
			if (entity == null)
			{
				return null;
			}
			return entity.GetOptional<TPart>();
		}
	}

	[MemoryPackConstructor]
	private EntityPartRef(EntityRef<TEntity> m_ref)
	{
		m_Ref = m_ref;
	}

	public EntityPartRef([CanBeNull] string id)
		: this(new EntityRef<TEntity>(id))
	{
	}

	public EntityPartRef([CanBeNull] TEntity entity)
		: this(new EntityRef<TEntity>(entity))
	{
	}

	public override int GetHashCode()
	{
		return m_Ref.GetHashCode();
	}

	public static implicit operator EntityPartRef<TEntity, TPart>([CanBeNull] TPart part)
	{
		return new EntityPartRef<TEntity, TPart>((part != null) ? part.Owner : null);
	}

	public static implicit operator EntityPartRef<TEntity, TPart>([CanBeNull] TEntity entity)
	{
		return new EntityPartRef<TEntity, TPart>(entity);
	}

	public static implicit operator TEntity(EntityPartRef<TEntity, TPart> @ref)
	{
		return @ref.Entity;
	}

	public static implicit operator TPart(EntityPartRef<TEntity, TPart> @ref)
	{
		return @ref.EntityPart;
	}

	public bool Equals(EntityPartRef<TEntity, TPart> other)
	{
		return m_Ref.Equals(other.m_Ref);
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityPartRef<TEntity, TPart> other)
		{
			return Equals(other);
		}
		return false;
	}

	static EntityPartRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EntityPartRef<TEntity, TPart>>())
		{
			MemoryPackFormatterProvider.Register(new EntityPartRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityPartRef<TEntity, TPart>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityPartRef<TEntity, TPart>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EntityPartRef<TEntity, TPart> value)
	{
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Ref);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EntityPartRef<TEntity, TPart> value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(EntityPartRef<TEntity, TPart>);
			return;
		}
		EntityRef<TEntity> value2;
		if (memberCount == 1)
		{
			value2 = reader.ReadPackable<EntityRef<TEntity>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EntityPartRef<TEntity, TPart>), 1, memberCount);
				return;
			}
			value2 = default(EntityRef<TEntity>);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
		}
		value = new EntityPartRef<TEntity, TPart>(value2);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<TEntity> obj = m_Ref;
		Hash128 val = StructHasher<EntityRef<TEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		return result;
	}
}
