using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class CreateCargoInternalGameCommand : GameCommand, IMemoryPackable<CreateCargoInternalGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CreateCargoInternalGameCommandFormatter : MemoryPackFormatter<CreateCargoInternalGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CreateCargoInternalGameCommand value)
		{
			CreateCargoInternalGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CreateCargoInternalGameCommand value)
		{
			CreateCargoInternalGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintCargoReference m_CargoBlueprint;

	[MemoryPackConstructor]
	private CreateCargoInternalGameCommand()
	{
	}

	[JsonConstructor]
	public CreateCargoInternalGameCommand(BlueprintCargo cargoBlueprint)
	{
		m_CargoBlueprint = cargoBlueprint.ToReference<BlueprintCargoReference>();
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.CargoState.Create(m_CargoBlueprint);
	}

	static CreateCargoInternalGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CreateCargoInternalGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CreateCargoInternalGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CreateCargoInternalGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CreateCargoInternalGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CreateCargoInternalGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_CargoBlueprint);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CreateCargoInternalGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintCargoReference value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_CargoBlueprint;
				reader.ReadPackable(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadPackable<BlueprintCargoReference>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CreateCargoInternalGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_CargoBlueprint : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new CreateCargoInternalGameCommand
		{
			m_CargoBlueprint = value2
		};
		return;
		IL_006a:
		value.m_CargoBlueprint = value2;
	}
}
