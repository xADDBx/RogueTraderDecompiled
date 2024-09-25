using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic.Commands;

[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackUnion(0, typeof(UnitAreaTransitionParams))]
public abstract class UnitGroupCommandParams : UnitCommandParams, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UnitGroupCommandParamsFormatter : MemoryPackFormatter<UnitGroupCommandParams>
	{
		private static readonly Dictionary<Type, ushort> __typeToTag = new Dictionary<Type, ushort>(1) { 
		{
			typeof(UnitAreaTransitionParams),
			0
		} };

		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UnitGroupCommandParams? value)
		{
			ushort value2;
			if (value == null)
			{
				writer.WriteNullUnionHeader();
			}
			else if (__typeToTag.TryGetValue(value.GetType(), out value2))
			{
				writer.WriteUnionHeader(value2);
				if (value2 == 0)
				{
					writer.WritePackable(in Unsafe.As<UnitGroupCommandParams, UnitAreaTransitionParams>(ref value));
				}
			}
			else
			{
				MemoryPackSerializationException.ThrowNotFoundInUnionType(value.GetType(), typeof(UnitGroupCommandParams));
			}
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UnitGroupCommandParams? value)
		{
			if (!reader.TryReadUnionHeader(out var tag))
			{
				value = null;
			}
			else if (tag == 0)
			{
				if (value is UnitAreaTransitionParams)
				{
					reader.ReadPackable(ref Unsafe.As<UnitGroupCommandParams, UnitAreaTransitionParams>(ref value));
				}
				else
				{
					value = reader.ReadPackable<UnitAreaTransitionParams>();
				}
			}
			else
			{
				MemoryPackSerializationException.ThrowInvalidTag(tag, typeof(UnitGroupCommandParams));
			}
		}
	}

	[JsonProperty]
	public readonly Guid GroupGuid;

	[JsonProperty]
	[NotNull]
	public readonly List<EntityRef<BaseUnitEntity>> Units;

	[JsonConstructor]
	protected UnitGroupCommandParams(JsonConstructorMark _)
		: base(_)
	{
	}

	protected UnitGroupCommandParams(Guid groupGuid, [NotNull] List<EntityRef<BaseUnitEntity>> units, [CanBeNull] TargetWrapper target)
		: base(target)
	{
		GroupGuid = groupGuid;
		Units = units;
	}

	static UnitGroupCommandParams()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UnitGroupCommandParams>())
		{
			MemoryPackFormatterProvider.Register(new UnitGroupCommandParamsFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UnitGroupCommandParams[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UnitGroupCommandParams>());
		}
	}
}
public abstract class UnitGroupCommandParams<T> : UnitGroupCommandParams where T : UnitGroupCommand
{
	[JsonConstructor]
	protected UnitGroupCommandParams(JsonConstructorMark _)
		: base(_)
	{
	}

	protected UnitGroupCommandParams(Guid groupGuid, [NotNull] List<EntityRef<BaseUnitEntity>> units, [CanBeNull] TargetWrapper target)
		: base(groupGuid, units, target)
	{
	}

	protected override AbstractUnitCommand CreateCommandInternal()
	{
		return (AbstractUnitCommand)Activator.CreateInstance(typeof(T), this);
	}

	public new T CreateCommand()
	{
		return (T)CreateCommandInternal();
	}
}
