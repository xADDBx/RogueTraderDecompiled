using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class InteractWithStarSystemObjectGameCommand : GameCommand, IMemoryPackable<InteractWithStarSystemObjectGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class InteractWithStarSystemObjectGameCommandFormatter : MemoryPackFormatter<InteractWithStarSystemObjectGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref InteractWithStarSystemObjectGameCommand value)
		{
			InteractWithStarSystemObjectGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref InteractWithStarSystemObjectGameCommand value)
		{
			InteractWithStarSystemObjectGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<StarSystemObjectEntity> m_StarSystemObjectEntityRef;

	[JsonProperty]
	[MemoryPackInclude]
	private Vector3 m_Position;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private InteractWithStarSystemObjectGameCommand()
	{
	}

	[MemoryPackConstructor]
	private InteractWithStarSystemObjectGameCommand(EntityRef<StarSystemObjectEntity> m_starSystemObjectEntityRef, Vector3 m_position)
	{
		m_StarSystemObjectEntityRef = m_starSystemObjectEntityRef;
		m_Position = m_position;
	}

	public InteractWithStarSystemObjectGameCommand(StarSystemObjectEntity starSystemObjectEntity, Vector3 position)
		: this((EntityRef<StarSystemObjectEntity>)starSystemObjectEntity, position)
	{
	}

	protected override void ExecuteInternal()
	{
		StarSystemObjectView starSystemObjectView = m_StarSystemObjectEntityRef.Entity?.View;
		if (!(StarSystemMapClickObjectHandler.DestinationSso == starSystemObjectView) || !StarSystemMapMoveController.CheckLanding())
		{
			StarSystemMapClickObjectHandler.DestinationSso = null;
			if (starSystemObjectView is AnomalyView anomalyView && anomalyView.Data.CanInteract())
			{
				StarSystemMapClickObjectHandler.DestinationSso = starSystemObjectView;
				StarSystemMapClickObjectHandler.MovePlayerShipAndInteract(anomalyView.Data);
			}
			else if (starSystemObjectView != null)
			{
				StarSystemMapClickObjectHandler.MovePlayerShip(starSystemObjectView);
			}
			else
			{
				StarSystemMapMoveController.MovePlayerShip(m_Position);
			}
		}
	}

	static InteractWithStarSystemObjectGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<InteractWithStarSystemObjectGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new InteractWithStarSystemObjectGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<InteractWithStarSystemObjectGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<InteractWithStarSystemObjectGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref InteractWithStarSystemObjectGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_StarSystemObjectEntityRef);
		writer.WriteUnmanaged(in value.m_Position);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref InteractWithStarSystemObjectGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<StarSystemObjectEntity> value2;
		Vector3 value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<StarSystemObjectEntity>>();
				reader.ReadUnmanaged<Vector3>(out value3);
			}
			else
			{
				value2 = value.m_StarSystemObjectEntityRef;
				value3 = value.m_Position;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<Vector3>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(InteractWithStarSystemObjectGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<StarSystemObjectEntity>);
				value3 = default(Vector3);
			}
			else
			{
				value2 = value.m_StarSystemObjectEntityRef;
				value3 = value.m_Position;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<Vector3>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new InteractWithStarSystemObjectGameCommand(value2, value3);
	}
}
