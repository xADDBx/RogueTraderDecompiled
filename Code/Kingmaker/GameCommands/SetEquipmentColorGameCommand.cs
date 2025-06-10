using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.Visual.CharacterSystem;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SetEquipmentColorGameCommand : GameCommand, IMemoryPackable<SetEquipmentColorGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetEquipmentColorGameCommandFormatter : MemoryPackFormatter<SetEquipmentColorGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetEquipmentColorGameCommand value)
		{
			SetEquipmentColorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetEquipmentColorGameCommand value)
		{
			SetEquipmentColorGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private UnitReference m_UnitRef;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_PrimaryIndex;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_SecondaryIndex;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SetEquipmentColorGameCommand()
	{
	}

	[MemoryPackConstructor]
	private SetEquipmentColorGameCommand(int m_primaryindex, int m_secondaryindex, UnitReference m_unitref)
	{
		m_PrimaryIndex = m_primaryindex;
		m_SecondaryIndex = m_secondaryindex;
		m_UnitRef = m_unitref;
	}

	public SetEquipmentColorGameCommand(RampColorPreset.IndexSet indexSet, BaseUnitEntity unit)
	{
		m_PrimaryIndex = indexSet.PrimaryIndex;
		m_SecondaryIndex = indexSet.SecondaryIndex;
		m_UnitRef = UnitReference.FromIAbstractUnitEntity(unit);
	}

	public SetEquipmentColorGameCommand(BaseUnitEntity indexSet, Texture2D texture)
	{
	}

	protected override void ExecuteInternal()
	{
		RampColorPreset.IndexSet indexSet = new RampColorPreset.IndexSet();
		indexSet.PrimaryIndex = m_PrimaryIndex;
		indexSet.SecondaryIndex = m_SecondaryIndex;
		m_UnitRef.Entity.ToBaseUnitEntity().SetUnitEquipmentColorRampIndex(indexSet);
		m_UnitRef.Entity.ToBaseUnitEntity().View.Or(null)?.UpdateEquipmentColorRampIndices();
	}

	static SetEquipmentColorGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetEquipmentColorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetEquipmentColorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetEquipmentColorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetEquipmentColorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetEquipmentColorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_UnitRef);
		writer.WriteUnmanaged(in value.m_PrimaryIndex, in value.m_SecondaryIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetEquipmentColorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		UnitReference value2;
		int value3;
		int value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<UnitReference>();
				reader.ReadUnmanaged<int, int>(out value3, out value4);
			}
			else
			{
				value2 = value.m_UnitRef;
				value3 = value.m_PrimaryIndex;
				value4 = value.m_SecondaryIndex;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<int>(out value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetEquipmentColorGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(UnitReference);
				value3 = 0;
				value4 = 0;
			}
			else
			{
				value2 = value.m_UnitRef;
				value3 = value.m_PrimaryIndex;
				value4 = value.m_SecondaryIndex;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new SetEquipmentColorGameCommand(value3, value4, value2);
	}
}
