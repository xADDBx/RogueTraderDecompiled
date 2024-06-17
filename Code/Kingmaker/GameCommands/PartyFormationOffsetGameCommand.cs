using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class PartyFormationOffsetGameCommand : GameCommand, IMemoryPackable<PartyFormationOffsetGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PartyFormationOffsetGameCommandFormatter : MemoryPackFormatter<PartyFormationOffsetGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PartyFormationOffsetGameCommand value)
		{
			PartyFormationOffsetGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PartyFormationOffsetGameCommand value)
		{
			PartyFormationOffsetGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_FormationIndex;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_Index;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly UnitReference m_Unit;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly Vector2 m_Vector;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private PartyFormationOffsetGameCommand()
	{
	}

	public PartyFormationOffsetGameCommand(int m_formationIndex, int m_index, BaseUnitEntity m_unit, Vector2 m_vector)
	{
		m_FormationIndex = m_formationIndex;
		m_Index = m_index;
		m_Unit = m_unit.FromBaseUnitEntity();
		m_Vector = m_vector;
	}

	[MemoryPackConstructor]
	public PartyFormationOffsetGameCommand(int m_formationIndex, int m_index, UnitReference m_unit, Vector2 m_vector)
	{
		m_FormationIndex = m_formationIndex;
		m_Index = m_index;
		m_Unit = m_unit;
		m_Vector = m_vector;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.FormationManager.SetOffset(m_FormationIndex, m_Index, m_Unit.ToBaseUnitEntity(), m_Vector);
	}

	static PartyFormationOffsetGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationOffsetGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PartyFormationOffsetGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PartyFormationOffsetGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PartyFormationOffsetGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PartyFormationOffsetGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(4, in value.m_FormationIndex, in value.m_Index);
		writer.WritePackable(in value.m_Unit);
		writer.WriteUnmanaged(in value.m_Vector);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PartyFormationOffsetGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		int value3;
		UnitReference value4;
		Vector2 value5;
		if (memberCount == 4)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int, int>(out value2, out value3);
				value4 = reader.ReadPackable<UnitReference>();
				reader.ReadUnmanaged<Vector2>(out value5);
			}
			else
			{
				value2 = value.m_FormationIndex;
				value3 = value.m_Index;
				value4 = value.m_Unit;
				value5 = value.m_Vector;
				reader.ReadUnmanaged<int>(out value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadPackable(ref value4);
				reader.ReadUnmanaged<Vector2>(out value5);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PartyFormationOffsetGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
				value4 = default(UnitReference);
				value5 = default(Vector2);
			}
			else
			{
				value2 = value.m_FormationIndex;
				value3 = value.m_Index;
				value4 = value.m_Unit;
				value5 = value.m_Vector;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<Vector2>(out value5);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new PartyFormationOffsetGameCommand(value2, value3, value4, value5);
	}
}
