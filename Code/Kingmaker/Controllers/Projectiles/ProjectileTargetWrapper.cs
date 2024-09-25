using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.Projectiles;

[MemoryPackable(GenerateType.Object)]
public class ProjectileTargetWrapper : TargetWrapper, IMemoryPackable<ProjectileTargetWrapper>, IMemoryPackFormatterRegister, IHashable
{
	[Preserve]
	private sealed class ProjectileTargetWrapperFormatter : MemoryPackFormatter<ProjectileTargetWrapper>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ProjectileTargetWrapper value)
		{
			ProjectileTargetWrapper.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ProjectileTargetWrapper value)
		{
			ProjectileTargetWrapper.Deserialize(ref reader, ref value);
		}
	}

	private readonly Transform m_Transform;

	public override Vector3 Point => m_Transform.Or(null)?.position ?? base.Point;

	public override float Orientation => m_Transform.Or(null)?.rotation.y ?? base.Orientation;

	public override bool IsOrientationSpecified => m_Transform != null;

	public override string ToString()
	{
		if ((bool)m_Transform)
		{
			return "[Target: point '" + m_Transform.name + "']";
		}
		return base.ToString();
	}

	[MemoryPackConstructor]
	private ProjectileTargetWrapper(EntityRef<MechanicEntity> entityRef, Vector3? m_point, float? m_orientation)
		: base(entityRef, m_point, m_orientation)
	{
	}

	public ProjectileTargetWrapper([NotNull] Transform t)
	{
		m_Transform = t.Or(null) ?? throw new ArgumentException("TargetWrapper: 'transform' is null");
	}

	public ProjectileTargetWrapper([NotNull] BaseUnitEntity unit)
		: base(unit)
	{
	}

	public ProjectileTargetWrapper(Vector3 point, float orientation)
		: base(point, orientation)
	{
	}

	public static implicit operator ProjectileTargetWrapper([NotNull] Transform t)
	{
		return new ProjectileTargetWrapper(t);
	}

	public static implicit operator ProjectileTargetWrapper([NotNull] BaseUnitEntity unit)
	{
		return new ProjectileTargetWrapper(unit);
	}

	public static implicit operator ProjectileTargetWrapper(Vector3 point)
	{
		return new ProjectileTargetWrapper(point, 0f);
	}

	public override bool Equals(object obj)
	{
		return Equals(this, obj as ProjectileTargetWrapper);
	}

	public override bool Equals(TargetWrapper other)
	{
		return Equals(this, other as ProjectileTargetWrapper);
	}

	public bool Equals(ProjectileTargetWrapper other)
	{
		return Equals(this, other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), m_Transform.GetHashCode());
	}

	private static bool Equals(ProjectileTargetWrapper x, ProjectileTargetWrapper y)
	{
		if (!TargetWrapper.Equals(x, y))
		{
			return false;
		}
		return x.m_Transform.Equals(y.m_Transform);
	}

	static ProjectileTargetWrapper()
	{
		RegisterFormatter();
	}

	[Preserve]
	public new static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ProjectileTargetWrapper>())
		{
			MemoryPackFormatterProvider.Register(new ProjectileTargetWrapperFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ProjectileTargetWrapper[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ProjectileTargetWrapper>());
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
	public static void Serialize(ref MemoryPackWriter writer, ref ProjectileTargetWrapper? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref ProjectileTargetWrapper? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ProjectileTargetWrapper), 3, memberCount);
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
		value = new ProjectileTargetWrapper(value2, value3, value4);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
