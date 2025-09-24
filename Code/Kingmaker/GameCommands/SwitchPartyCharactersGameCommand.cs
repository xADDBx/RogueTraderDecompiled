using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SwitchPartyCharactersGameCommand : GameCommand, IMemoryPackable<SwitchPartyCharactersGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SwitchPartyCharactersGameCommandFormatter : MemoryPackFormatter<SwitchPartyCharactersGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SwitchPartyCharactersGameCommand value)
		{
			SwitchPartyCharactersGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SwitchPartyCharactersGameCommand value)
		{
			SwitchPartyCharactersGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly sbyte m_Index1;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly sbyte m_Index2;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit1;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_Unit2;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private SwitchPartyCharactersGameCommand()
	{
	}

	[MemoryPackConstructor]
	private SwitchPartyCharactersGameCommand(EntityRef<BaseUnitEntity> m_unit1, EntityRef<BaseUnitEntity> m_unit2, sbyte m_index1, sbyte m_index2)
	{
		m_Unit1 = m_unit1;
		m_Unit2 = m_unit2;
		m_Index1 = m_index1;
		m_Index2 = m_index2;
	}

	public SwitchPartyCharactersGameCommand(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		m_Unit1 = unit1;
		m_Unit2 = unit2;
		m_Index1 = (sbyte)Game.Instance.Player.PartyAndPets.FindIndex((BaseUnitEntity u) => u == unit1);
		m_Index2 = (sbyte)Game.Instance.Player.PartyAndPets.FindIndex((BaseUnitEntity u) => u == unit2);
	}

	protected override void ExecuteInternal()
	{
		BaseUnitEntity unit1 = m_Unit1.Entity;
		BaseUnitEntity unit2 = m_Unit2.Entity;
		if (unit1 != null && unit2 != null)
		{
			int num = Game.Instance.Player.PartyAndPets.FindIndex((BaseUnitEntity u) => u == unit1);
			int num2 = Game.Instance.Player.PartyAndPets.FindIndex((BaseUnitEntity u) => u == unit2);
			if (num == m_Index1 && num2 == m_Index2)
			{
				Game.Instance.SelectionCharacter.SwitchCharacter(unit1, unit2);
			}
		}
	}

	static SwitchPartyCharactersGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SwitchPartyCharactersGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SwitchPartyCharactersGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SwitchPartyCharactersGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SwitchPartyCharactersGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SwitchPartyCharactersGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(4, in value.m_Index1, in value.m_Index2);
		writer.WritePackable(in value.m_Unit1);
		writer.WritePackable(in value.m_Unit2);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SwitchPartyCharactersGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		sbyte value2;
		sbyte value3;
		EntityRef<BaseUnitEntity> value4;
		EntityRef<BaseUnitEntity> value5;
		if (memberCount == 4)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<sbyte, sbyte>(out value2, out value3);
				value4 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				value5 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			}
			else
			{
				value2 = value.m_Index1;
				value3 = value.m_Index2;
				value4 = value.m_Unit1;
				value5 = value.m_Unit2;
				reader.ReadUnmanaged<sbyte>(out value2);
				reader.ReadUnmanaged<sbyte>(out value3);
				reader.ReadPackable(ref value4);
				reader.ReadPackable(ref value5);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SwitchPartyCharactersGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
				value4 = default(EntityRef<BaseUnitEntity>);
				value5 = default(EntityRef<BaseUnitEntity>);
			}
			else
			{
				value2 = value.m_Index1;
				value3 = value.m_Index2;
				value4 = value.m_Unit1;
				value5 = value.m_Unit2;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<sbyte>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<sbyte>(out value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						if (memberCount != 3)
						{
							reader.ReadPackable(ref value5);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new SwitchPartyCharactersGameCommand(value4, value5, value2, value3);
	}
}
