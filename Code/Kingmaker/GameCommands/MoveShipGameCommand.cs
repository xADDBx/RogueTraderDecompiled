using System;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SystemMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class MoveShipGameCommand : GameCommand, IMemoryPackable<MoveShipGameCommand>, IMemoryPackFormatterRegister
{
	public enum VisitType
	{
		None,
		MovePlayerShip,
		HandlePlayerShipMove
	}

	[Preserve]
	private sealed class MoveShipGameCommandFormatter : MemoryPackFormatter<MoveShipGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref MoveShipGameCommand value)
		{
			MoveShipGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref MoveShipGameCommand value)
		{
			MoveShipGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<StarSystemObjectEntity> m_StarSystemObjectEntityRef;

	[JsonProperty]
	[MemoryPackInclude]
	private VisitType m_VisitType;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private MoveShipGameCommand()
	{
	}

	[MemoryPackConstructor]
	private MoveShipGameCommand(EntityRef<StarSystemObjectEntity> m_starSystemObjectEntityRef, VisitType m_visitType)
	{
		m_StarSystemObjectEntityRef = m_starSystemObjectEntityRef;
		m_VisitType = m_visitType;
	}

	public MoveShipGameCommand(StarSystemObjectEntity starSystemObjectEntity, VisitType visitType)
		: this((EntityRef<StarSystemObjectEntity>)starSystemObjectEntity, visitType)
	{
	}

	protected override void ExecuteInternal()
	{
		StarSystemObjectEntity entity = m_StarSystemObjectEntityRef.Entity;
		if (entity == null)
		{
			PFLog.Space.Error("StarSystemObjectEntity not found! ID=" + m_StarSystemObjectEntityRef.Id);
			return;
		}
		StarSystemObjectView view = entity.View;
		if (view == null)
		{
			PFLog.Space.Error("StarSystemObjectEntity has no view! ID=" + entity.UniqueId);
		}
		else if (!(StarSystemMapClickObjectHandler.DestinationSso == view) || !StarSystemMapMoveController.CheckLanding())
		{
			switch (m_VisitType)
			{
			case VisitType.MovePlayerShip:
				StarSystemMapClickObjectHandler.MovePlayerShip(view);
				break;
			case VisitType.HandlePlayerShipMove:
				StarSystemMapClickObjectHandler.HandlePlayerShipMove(view);
				break;
			default:
				throw new ArgumentOutOfRangeException(m_VisitType.ToString());
			}
		}
	}

	static MoveShipGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<MoveShipGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new MoveShipGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<MoveShipGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<MoveShipGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<VisitType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<VisitType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref MoveShipGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_StarSystemObjectEntityRef);
		writer.WriteUnmanaged(in value.m_VisitType);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref MoveShipGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<StarSystemObjectEntity> value2;
		VisitType value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<StarSystemObjectEntity>>();
				reader.ReadUnmanaged<VisitType>(out value3);
			}
			else
			{
				value2 = value.m_StarSystemObjectEntityRef;
				value3 = value.m_VisitType;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<VisitType>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(MoveShipGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<StarSystemObjectEntity>);
				value3 = VisitType.None;
			}
			else
			{
				value2 = value.m_StarSystemObjectEntityRef;
				value3 = value.m_VisitType;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<VisitType>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new MoveShipGameCommand(value2, value3);
	}
}
