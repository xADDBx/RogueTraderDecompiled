using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class RemoveCargoFromSellGameCommand : GameCommand, IMemoryPackable<RemoveCargoFromSellGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class RemoveCargoFromSellGameCommandFormatter : MemoryPackFormatter<RemoveCargoFromSellGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref RemoveCargoFromSellGameCommand value)
		{
			RemoveCargoFromSellGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref RemoveCargoFromSellGameCommand value)
		{
			RemoveCargoFromSellGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<CargoEntity> m_CargoRef;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private RemoveCargoFromSellGameCommand()
	{
	}

	[JsonConstructor]
	public RemoveCargoFromSellGameCommand(EntityRef<CargoEntity> cargoRef)
	{
		m_CargoRef = cargoRef;
	}

	protected override void ExecuteInternal()
	{
		CargoEntity entity = m_CargoRef.Entity;
		if (entity != null)
		{
			VendorHelper.Vendor.RemoveFromSell(entity);
		}
	}

	static RemoveCargoFromSellGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveCargoFromSellGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new RemoveCargoFromSellGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<RemoveCargoFromSellGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<RemoveCargoFromSellGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref RemoveCargoFromSellGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref RemoveCargoFromSellGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(RemoveCargoFromSellGameCommand), 1, memberCount);
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
		value = new RemoveCargoFromSellGameCommand
		{
			m_CargoRef = value2
		};
		return;
		IL_0070:
		value.m_CargoRef = value2;
	}
}
