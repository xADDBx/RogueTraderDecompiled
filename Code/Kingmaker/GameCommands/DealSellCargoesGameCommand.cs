using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class DealSellCargoesGameCommand : GameCommand, IMemoryPackable<DealSellCargoesGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DealSellCargoesGameCommandFormatter : MemoryPackFormatter<DealSellCargoesGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DealSellCargoesGameCommand value)
		{
			DealSellCargoesGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DealSellCargoesGameCommand value)
		{
			DealSellCargoesGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<MechanicEntity> m_VendorRef;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private DealSellCargoesGameCommand()
	{
	}

	[JsonConstructor]
	public DealSellCargoesGameCommand(EntityRef<MechanicEntity> vendorRef)
	{
		m_VendorRef = vendorRef;
	}

	protected override void ExecuteInternal()
	{
		MechanicEntity entity = m_VendorRef.Entity;
		if (entity != null)
		{
			VendorHelper.Vendor.DealSellCargoes(entity);
		}
	}

	static DealSellCargoesGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DealSellCargoesGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new DealSellCargoesGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DealSellCargoesGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DealSellCargoesGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DealSellCargoesGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_VendorRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref DealSellCargoesGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<MechanicEntity> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_VendorRef;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityRef<MechanicEntity>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DealSellCargoesGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_VendorRef : default(EntityRef<MechanicEntity>));
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
		value = new DealSellCargoesGameCommand
		{
			m_VendorRef = value2
		};
		return;
		IL_0070:
		value.m_VendorRef = value2;
	}
}
