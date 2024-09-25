using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class PointOfInterestInteractGameCommand : GameCommand, IMemoryPackable<PointOfInterestInteractGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PointOfInterestInteractGameCommandFormatter : MemoryPackFormatter<PointOfInterestInteractGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PointOfInterestInteractGameCommand value)
		{
			PointOfInterestInteractGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PointOfInterestInteractGameCommand value)
		{
			PointOfInterestInteractGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<StarSystemObjectEntity> m_StarSystemObjectRef;

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintPointOfInterest m_Poi;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private PointOfInterestInteractGameCommand()
	{
	}

	[JsonConstructor]
	public PointOfInterestInteractGameCommand(StarSystemObjectEntity starSystemObject, BlueprintPointOfInterest poi)
	{
		m_StarSystemObjectRef = starSystemObject;
		m_Poi = poi;
	}

	protected override void ExecuteInternal()
	{
		StarSystemObjectEntity entity = m_StarSystemObjectRef.Entity;
		if (entity != null && m_Poi != null)
		{
			entity.PointOfInterests.FirstOrDefault((BasePointOfInterest p) => p.Blueprint == m_Poi)?.Interact(entity);
		}
	}

	static PointOfInterestInteractGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PointOfInterestInteractGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PointOfInterestInteractGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PointOfInterestInteractGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PointOfInterestInteractGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PointOfInterestInteractGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_StarSystemObjectRef);
		writer.WriteValue(in value.m_Poi);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PointOfInterestInteractGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<StarSystemObjectEntity> value2;
		BlueprintPointOfInterest value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_StarSystemObjectRef;
				value3 = value.m_Poi;
				reader.ReadPackable(ref value2);
				reader.ReadValue(ref value3);
				goto IL_00a0;
			}
			value2 = reader.ReadPackable<EntityRef<StarSystemObjectEntity>>();
			value3 = reader.ReadValue<BlueprintPointOfInterest>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PointOfInterestInteractGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<StarSystemObjectEntity>);
				value3 = null;
			}
			else
			{
				value2 = value.m_StarSystemObjectRef;
				value3 = value.m_Poi;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadValue(ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a0;
			}
		}
		value = new PointOfInterestInteractGameCommand
		{
			m_StarSystemObjectRef = value2,
			m_Poi = value3
		};
		return;
		IL_00a0:
		value.m_StarSystemObjectRef = value2;
		value.m_Poi = value3;
	}
}
