using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class HoldUnitsGameCommand : GameCommand, IMemoryPackable<HoldUnitsGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class HoldUnitsGameCommandFormatter : MemoryPackFormatter<HoldUnitsGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref HoldUnitsGameCommand value)
		{
			HoldUnitsGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref HoldUnitsGameCommand value)
		{
			HoldUnitsGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity>[] m_Units;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private HoldUnitsGameCommand(EntityRef<BaseUnitEntity>[] m_units)
	{
		m_Units = m_units;
	}

	public HoldUnitsGameCommand(IList<BaseUnitEntity> units)
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
				baseUnitEntity.HoldState = true;
				baseUnitEntity.Commands.InterruptMove();
			}
		}
	}

	static HoldUnitsGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<HoldUnitsGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new HoldUnitsGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<HoldUnitsGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<HoldUnitsGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EntityRef<BaseUnitEntity>[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EntityRef<BaseUnitEntity>>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref HoldUnitsGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref HoldUnitsGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(HoldUnitsGameCommand), 1, memberCount);
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
		value = new HoldUnitsGameCommand(value2);
	}
}
