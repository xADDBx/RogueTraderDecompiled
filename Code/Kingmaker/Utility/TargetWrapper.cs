using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Pathfinding;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Utility;

[MemoryPackable(GenerateType.Object)]
public class TargetWrapper : ITargetWrapper, IMemoryPackable<TargetWrapper>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class TargetWrapperFormatter : MemoryPackFormatter<TargetWrapper>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref TargetWrapper value)
		{
			TargetWrapper.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TargetWrapper value)
		{
			TargetWrapper.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	public readonly EntityRef<MechanicEntity> EntityRef;

	[JsonProperty]
	[MemoryPackInclude]
	protected readonly Vector3? m_Point;

	[JsonProperty]
	[MemoryPackInclude]
	protected readonly float? m_Orientation;

	[CanBeNull]
	[MemoryPackIgnore]
	public MechanicEntity Entity => EntityRef.Entity;

	[CanBeNull]
	[MemoryPackIgnore]
	public IMechanicEntity IEntity => EntityRef.Entity;

	[MemoryPackIgnore]
	public virtual Vector3 Point => m_Point ?? Entity?.Position ?? default(Vector3);

	[MemoryPackIgnore]
	public virtual float Orientation
	{
		get
		{
			float? orientation = m_Orientation;
			if (!orientation.HasValue)
			{
				if (!IsPoint)
				{
					return Entity?.Orientation ?? 0f;
				}
				return 0f;
			}
			return orientation.GetValueOrDefault();
		}
	}

	[MemoryPackIgnore]
	public bool HasEntity => !EntityRef.IsNull;

	[MemoryPackIgnore]
	public bool IsPoint => m_Point.HasValue;

	[MemoryPackIgnore]
	public virtual bool IsOrientationSpecified => m_Orientation.HasValue;

	[MemoryPackIgnore]
	public IntRect SizeRect
	{
		get
		{
			if (!IsPoint)
			{
				return Entity?.SizeRect ?? default(IntRect);
			}
			return default(IntRect);
		}
	}

	[MemoryPackIgnore]
	public Vector3 Forward
	{
		get
		{
			if (!IsPoint)
			{
				return Entity?.Forward ?? Vector3.forward;
			}
			return Vector3.forward;
		}
	}

	[MemoryPackIgnore]
	public CustomGridNodeBase NearestNode
	{
		get
		{
			if (!(Entity?.Features.DisableSnapToGrid))
			{
				return Point.GetNearestNodeXZUnwalkable();
			}
			return Point.GetNearestNodeXZ();
		}
	}

	public TargetWrapper([NotNull] MechanicEntity unit)
	{
		EntityRef = unit ?? throw new ArgumentException("TargetWrapper: 'unit' is null");
	}

	public TargetWrapper(Vector3 point)
		: this(point, null, null)
	{
	}

	public TargetWrapper(Vector3 point, float? orientation)
		: this(point, orientation, null)
	{
	}

	public TargetWrapper(Vector3 point, float? orientation, MechanicEntity entity)
	{
		m_Point = point;
		m_Orientation = orientation;
		EntityRef = entity;
	}

	protected TargetWrapper([NotNull] TargetWrapper other)
	{
		m_Point = other.m_Point;
		m_Orientation = other.m_Orientation;
		EntityRef = other.EntityRef;
	}

	[JsonConstructor]
	protected TargetWrapper()
	{
	}

	[MemoryPackConstructor]
	protected TargetWrapper(EntityRef<MechanicEntity> entityRef, Vector3? m_point, float? m_orientation)
	{
		EntityRef = entityRef;
		m_Point = m_point;
		m_Orientation = m_orientation;
	}

	public static implicit operator TargetWrapper(MechanicEntity unit)
	{
		if (unit == null)
		{
			return null;
		}
		return new TargetWrapper(unit);
	}

	public static implicit operator TargetWrapper(Vector3 point)
	{
		return new TargetWrapper(point);
	}

	public static implicit operator TargetWrapper(Vector3? point)
	{
		if (!point.HasValue)
		{
			return null;
		}
		return new TargetWrapper(point.Value);
	}

	public override string ToString()
	{
		if (!HasEntity)
		{
			return $"[Target: point '{Point}']";
		}
		return $"[Target: unit '{Entity}' {Point}]";
	}

	public override bool Equals(object obj)
	{
		return Equals(this, obj as TargetWrapper);
	}

	public virtual bool Equals(TargetWrapper other)
	{
		return Equals(this, other);
	}

	public static bool operator ==(TargetWrapper t1, TargetWrapper t2)
	{
		return Equals(t1, t2);
	}

	public static bool operator !=(TargetWrapper t1, TargetWrapper t2)
	{
		return !Equals(t1, t2);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(EntityRef, m_Point, m_Orientation);
	}

	protected static bool Equals(TargetWrapper x, TargetWrapper y)
	{
		if ((object)x == y)
		{
			return true;
		}
		if ((object)x == null)
		{
			return false;
		}
		if ((object)y == null)
		{
			return false;
		}
		if (x.GetType() != y.GetType())
		{
			return false;
		}
		if (x.EntityRef.Equals(y.EntityRef) && Nullable.Equals(x.m_Point, y.m_Point))
		{
			return Nullable.Equals(x.m_Orientation, y.m_Orientation);
		}
		return false;
	}

	static TargetWrapper()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TargetWrapper>())
		{
			MemoryPackFormatterProvider.Register(new TargetWrapperFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TargetWrapper[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TargetWrapper>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Vector3?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<Vector3>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<float?>())
		{
			MemoryPackFormatterProvider.Register(new NullableFormatter<float>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref TargetWrapper? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.EntityRef);
		writer.DangerousWriteUnmanaged(in value.m_Point, in value.m_Orientation);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TargetWrapper? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<MechanicEntity> value2;
		Vector3? value3;
		float? value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<MechanicEntity>>();
				reader.DangerousReadUnmanaged<Vector3?, float?>(out value3, out value4);
			}
			else
			{
				value2 = value.EntityRef;
				value3 = value.m_Point;
				value4 = value.m_Orientation;
				reader.ReadPackable(ref value2);
				reader.DangerousReadUnmanaged<Vector3?>(out value3);
				reader.DangerousReadUnmanaged<float?>(out value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TargetWrapper), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<MechanicEntity>);
				value3 = null;
				value4 = null;
			}
			else
			{
				value2 = value.EntityRef;
				value3 = value.m_Point;
				value4 = value.m_Orientation;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.DangerousReadUnmanaged<Vector3?>(out value3);
					if (memberCount != 2)
					{
						reader.DangerousReadUnmanaged<float?>(out value4);
						_ = 3;
					}
				}
			}
			_ = value == null;
		}
		value = new TargetWrapper(value2, value3, value4);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<MechanicEntity> obj = EntityRef;
		Hash128 val = StructHasher<EntityRef<MechanicEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		if (m_Point.HasValue)
		{
			Vector3 val2 = m_Point.Value;
			result.Append(ref val2);
		}
		if (m_Orientation.HasValue)
		{
			float val3 = m_Orientation.Value;
			result.Append(ref val3);
		}
		return result;
	}
}
