using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.Code.EntitySystem.Entities;

[JsonObject(IsReference = false)]
[MemoryPackable(GenerateType.Object)]
public struct ViewBasedPartRef<TEntity, TPart> : IEquatable<ViewBasedPartRef<TEntity, TPart>>, IMemoryPackable<ViewBasedPartRef<TEntity, TPart>>, IMemoryPackFormatterRegister where TEntity : Entity where TPart : ViewBasedPart
{
	[Preserve]
	private sealed class ViewBasedPartRefFormatter : MemoryPackFormatter<ViewBasedPartRef<TEntity, TPart>>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ViewBasedPartRef<TEntity, TPart> value)
		{
			ViewBasedPartRef<TEntity, TPart>.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ViewBasedPartRef<TEntity, TPart> value)
		{
			ViewBasedPartRef<TEntity, TPart>.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<TEntity> m_Ref;

	[NotNull]
	[JsonProperty]
	[MemoryPackInclude]
	private readonly Type m_PartType;

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
			return entity.GetOptional<TPart>(m_PartType);
		}
	}

	[MemoryPackConstructor]
	private ViewBasedPartRef(EntityRef<TEntity> m_ref, [NotNull] Type m_partType)
	{
		m_Ref = m_ref;
		m_PartType = m_partType;
	}

	public ViewBasedPartRef([CanBeNull] string id, [CanBeNull] Type type)
	{
		m_Ref = new EntityRef<TEntity>(id);
		m_PartType = type ?? typeof(TPart);
		CheckType(m_PartType);
	}

	public ViewBasedPartRef([CanBeNull] TEntity entity, [CanBeNull] Type type)
	{
		m_Ref = new EntityRef<TEntity>(entity);
		m_PartType = type ?? typeof(TPart);
		CheckType(m_PartType);
	}

	public ViewBasedPartRef([CanBeNull] TPart part, [CanBeNull] Type type)
	{
		m_Ref = new EntityRef<TEntity>((TEntity)(part?.Owner));
		m_PartType = type ?? typeof(TPart);
		CheckType(m_PartType);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(m_Ref, m_PartType);
	}

	public static implicit operator ViewBasedPartRef<TEntity, TPart>([CanBeNull] TPart part)
	{
		return new ViewBasedPartRef<TEntity, TPart>(part, null);
	}

	public static implicit operator ViewBasedPartRef<TEntity, TPart>([CanBeNull] TEntity entity)
	{
		return new ViewBasedPartRef<TEntity, TPart>(entity, null);
	}

	public static implicit operator TEntity(ViewBasedPartRef<TEntity, TPart> @ref)
	{
		return @ref.Entity;
	}

	public static implicit operator TPart(ViewBasedPartRef<TEntity, TPart> @ref)
	{
		return @ref.EntityPart;
	}

	public bool Equals(ViewBasedPartRef<TEntity, TPart> other)
	{
		if (m_Ref.Equals(other.m_Ref))
		{
			return m_PartType == other.m_PartType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ViewBasedPartRef<TEntity, TPart> other)
		{
			return Equals(other);
		}
		return false;
	}

	private static void CheckType(Type type)
	{
	}

	static ViewBasedPartRef()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ViewBasedPartRef<TEntity, TPart>>())
		{
			MemoryPackFormatterProvider.Register(new ViewBasedPartRefFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ViewBasedPartRef<TEntity, TPart>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ViewBasedPartRef<TEntity, TPart>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ViewBasedPartRef<TEntity, TPart> value)
	{
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Ref);
		writer.WriteValue(in value.m_PartType);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ViewBasedPartRef<TEntity, TPart> value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = default(ViewBasedPartRef<TEntity, TPart>);
			return;
		}
		EntityRef<TEntity> value2;
		Type value3;
		if (memberCount == 2)
		{
			value2 = reader.ReadPackable<EntityRef<TEntity>>();
			value3 = reader.ReadValue<Type>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ViewBasedPartRef<TEntity, TPart>), 2, memberCount);
				return;
			}
			value2 = default(EntityRef<TEntity>);
			value3 = null;
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					_ = 2;
				}
			}
		}
		value = new ViewBasedPartRef<TEntity, TPart>(value2, value3);
	}
}
