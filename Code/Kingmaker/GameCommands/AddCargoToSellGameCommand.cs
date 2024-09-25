using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class AddCargoToSellGameCommand : GameCommand, IMemoryPackable<AddCargoToSellGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AddCargoToSellGameCommandFormatter : MemoryPackFormatter<AddCargoToSellGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AddCargoToSellGameCommand value)
		{
			AddCargoToSellGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AddCargoToSellGameCommand value)
		{
			AddCargoToSellGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<CargoEntity> m_CargoRef;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private AddCargoToSellGameCommand()
	{
	}

	[JsonConstructor]
	public AddCargoToSellGameCommand(EntityRef<CargoEntity> cargoRef)
	{
		m_CargoRef = cargoRef;
	}

	protected override void ExecuteInternal()
	{
		CargoEntity entity = m_CargoRef.Entity;
		if (entity != null)
		{
			VendorHelper.Vendor.AddToSell(entity);
		}
	}

	static AddCargoToSellGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AddCargoToSellGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AddCargoToSellGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AddCargoToSellGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AddCargoToSellGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AddCargoToSellGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_CargoRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AddCargoToSellGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<CargoEntity> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_CargoRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityRef<CargoEntity>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AddCargoToSellGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_CargoRef : default(EntityRef<CargoEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0070;
			}
		}
		value = new AddCargoToSellGameCommand
		{
			m_CargoRef = value2
		};
		return;
		IL_0070:
		value.m_CargoRef = value2;
	}
}
