using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class StartTradingGameCommand : GameCommandWithSynchronized, IMemoryPackable<StartTradingGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StartTradingGameCommandFormatter : MemoryPackFormatter<StartTradingGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StartTradingGameCommand value)
		{
			StartTradingGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StartTradingGameCommand value)
		{
			StartTradingGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<MechanicEntity> m_Vendor;

	[MemoryPackConstructor]
	private StartTradingGameCommand(EntityRef<MechanicEntity> m_vendor)
	{
		m_Vendor = m_vendor;
	}

	public StartTradingGameCommand(MechanicEntity vendor, bool isSynchronized)
		: this(vendor)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		MechanicEntity entity = m_Vendor.Entity;
		if (entity != null)
		{
			EventBus.RaiseEvent((IMechanicEntity)entity, (Action<IVendorUIHandler>)delegate(IVendorUIHandler h)
			{
				h.HandleTradeStarted();
			}, isCheckRuntime: true);
		}
	}

	static StartTradingGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StartTradingGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StartTradingGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StartTradingGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StartTradingGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StartTradingGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Vendor);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StartTradingGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<MechanicEntity> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<MechanicEntity>>();
			}
			else
			{
				value2 = value.m_Vendor;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StartTradingGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Vendor : default(EntityRef<MechanicEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new StartTradingGameCommand(value2);
	}
}
