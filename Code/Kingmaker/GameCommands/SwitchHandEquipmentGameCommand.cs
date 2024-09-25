using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SwitchHandEquipmentGameCommand : GameCommand, IMemoryPackable<SwitchHandEquipmentGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SwitchHandEquipmentGameCommandFormatter : MemoryPackFormatter<SwitchHandEquipmentGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SwitchHandEquipmentGameCommand value)
		{
			SwitchHandEquipmentGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SwitchHandEquipmentGameCommand value)
		{
			SwitchHandEquipmentGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly sbyte m_HandEquipmentSetIndex;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SwitchHandEquipmentGameCommand()
	{
	}

	[MemoryPackConstructor]
	private SwitchHandEquipmentGameCommand(EntityRef<BaseUnitEntity> m_unit, sbyte m_handEquipmentSetIndex)
	{
		m_Unit = m_unit;
		m_HandEquipmentSetIndex = m_handEquipmentSetIndex;
	}

	public SwitchHandEquipmentGameCommand([NotNull] BaseUnitEntity unit, int handEquipmentSetIndex)
		: this((EntityRef<BaseUnitEntity>)unit, (sbyte)handEquipmentSetIndex)
	{
		if (unit == null)
		{
			throw new NullReferenceException("unit");
		}
		if (handEquipmentSetIndex < -128 || 127 < handEquipmentSetIndex)
		{
			throw new ArgumentOutOfRangeException($"handEquipmentSetIndex={handEquipmentSetIndex}");
		}
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity entity = m_Unit.Entity;
		if (entity == null)
		{
			PFLog.GameCommands.Error("Unit #" + m_Unit.Id + " not found!");
		}
		else
		{
			entity.Body.CurrentHandEquipmentSetIndex = m_HandEquipmentSetIndex;
		}
	}

	static SwitchHandEquipmentGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SwitchHandEquipmentGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SwitchHandEquipmentGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SwitchHandEquipmentGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SwitchHandEquipmentGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SwitchHandEquipmentGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Unit);
		writer.WriteUnmanaged(in value.m_HandEquipmentSetIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SwitchHandEquipmentGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		sbyte value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				reader.ReadUnmanaged<sbyte>(out value3);
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_HandEquipmentSetIndex;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<sbyte>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SwitchHandEquipmentGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<BaseUnitEntity>);
				value3 = 0;
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_HandEquipmentSetIndex;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<sbyte>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new SwitchHandEquipmentGameCommand(value2, value3);
	}
}
