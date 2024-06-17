using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class StopUnitsGameCommand : GameCommand, IMemoryPackable<StopUnitsGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StopUnitsGameCommandFormatter : MemoryPackFormatter<StopUnitsGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StopUnitsGameCommand value)
		{
			StopUnitsGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StopUnitsGameCommand value)
		{
			StopUnitsGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity>[] m_Units;

	private static readonly Func<AbstractUnitCommand, bool> NotStarted;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[MemoryPackConstructor]
	private StopUnitsGameCommand(EntityRef<BaseUnitEntity>[] m_units)
	{
		m_Units = m_units;
	}

	public StopUnitsGameCommand(IList<BaseUnitEntity> units)
	{
		int count = units.Count;
		m_Units = new EntityRef<BaseUnitEntity>[count];
		for (int i = 0; i < count; i++)
		{
			m_Units[i] = units[i];
		}
	}

	protected override void ExecuteInternal()
	{
		EntityRef<BaseUnitEntity>[] units = m_Units;
		foreach (BaseUnitEntity baseUnitEntity in units)
		{
			if (baseUnitEntity != null)
			{
				baseUnitEntity.HoldState = false;
				baseUnitEntity.Commands.InterruptAll(NotStarted);
				baseUnitEntity.CombatState.LastTarget = null;
				baseUnitEntity.CombatState.ManualTarget = null;
			}
		}
	}

	static StopUnitsGameCommand()
	{
		NotStarted = (AbstractUnitCommand unitCommand) => !unitCommand.IsStarted;
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StopUnitsGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StopUnitsGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StopUnitsGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StopUnitsGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityRef<BaseUnitEntity>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityRef<BaseUnitEntity>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StopUnitsGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackableArray(value.m_Units);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StopUnitsGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity>[] value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackableArray<EntityRef<BaseUnitEntity>>();
			}
			else
			{
				value2 = value.m_Units;
				reader.ReadPackableArray(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StopUnitsGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Units : null);
			if (memberCount != 0)
			{
				reader.ReadPackableArray(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new StopUnitsGameCommand(value2);
	}
}
