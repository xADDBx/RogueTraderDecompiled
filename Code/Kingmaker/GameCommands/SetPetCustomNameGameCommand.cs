using Kingmaker.EntitySystem.Entities;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SetPetCustomNameGameCommand : GameCommand, IMemoryPackable<SetPetCustomNameGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetPetCustomNameGameCommandFormatter : MemoryPackFormatter<SetPetCustomNameGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetPetCustomNameGameCommand value)
		{
			SetPetCustomNameGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetPetCustomNameGameCommand value)
		{
			SetPetCustomNameGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private UnitReference m_Target;

	[JsonProperty]
	[MemoryPackInclude]
	private string m_PetName;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SetPetCustomNameGameCommand()
	{
	}

	[JsonConstructor]
	public SetPetCustomNameGameCommand(UnitReference target, string petName)
	{
		m_Target = target;
		m_PetName = petName;
	}

	protected override void ExecuteInternal()
	{
		if (!m_Target.IsNull() && m_Target.Entity is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.Description.CustomPetName = m_PetName;
			baseUnitEntity.Pet?.Description?.SetName(m_PetName);
		}
	}

	static SetPetCustomNameGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetPetCustomNameGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetPetCustomNameGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetPetCustomNameGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetPetCustomNameGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetPetCustomNameGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Target);
		writer.WriteString(value.m_PetName);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetPetCustomNameGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		UnitReference value2;
		string petName;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_Target;
				petName = value.m_PetName;
				reader.ReadPackable(ref value2);
				petName = reader.ReadString();
				goto IL_009e;
			}
			value2 = reader.ReadPackable<UnitReference>();
			petName = reader.ReadString();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetPetCustomNameGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(UnitReference);
				petName = null;
			}
			else
			{
				value2 = value.m_Target;
				petName = value.m_PetName;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					petName = reader.ReadString();
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009e;
			}
		}
		value = new SetPetCustomNameGameCommand
		{
			m_Target = value2,
			m_PetName = petName
		};
		return;
		IL_009e:
		value.m_Target = value2;
		value.m_PetName = petName;
	}
}
