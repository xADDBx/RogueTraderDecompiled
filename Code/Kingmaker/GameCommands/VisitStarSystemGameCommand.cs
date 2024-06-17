using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SectorMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class VisitStarSystemGameCommand : GameCommand, IMemoryPackable<VisitStarSystemGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class VisitStarSystemGameCommandFormatter : MemoryPackFormatter<VisitStarSystemGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref VisitStarSystemGameCommand value)
		{
			VisitStarSystemGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref VisitStarSystemGameCommand value)
		{
			VisitStarSystemGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<SectorMapObjectEntity> m_SectorMapObject;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private VisitStarSystemGameCommand(EntityRef<SectorMapObjectEntity> m_sectorMapObject)
	{
		m_SectorMapObject = m_sectorMapObject;
	}

	public VisitStarSystemGameCommand(SectorMapObjectEntity starSystem)
		: this((EntityRef<SectorMapObjectEntity>)starSystem)
	{
	}

	protected override void ExecuteInternal()
	{
		SectorMapObjectEntity sectorMapObjectEntity = m_SectorMapObject;
		if (sectorMapObjectEntity == null)
		{
			PFLog.Entity.Error("SectorMapObjectEntity " + m_SectorMapObject.Id + " not found!");
		}
		else
		{
			Game.Instance.SectorMapController.VisitStarSystemInternal(sectorMapObjectEntity);
		}
	}

	static VisitStarSystemGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<VisitStarSystemGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new VisitStarSystemGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<VisitStarSystemGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<VisitStarSystemGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref VisitStarSystemGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_SectorMapObject);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref VisitStarSystemGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<SectorMapObjectEntity> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<SectorMapObjectEntity>>();
			}
			else
			{
				value2 = value.m_SectorMapObject;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(VisitStarSystemGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_SectorMapObject : default(EntityRef<SectorMapObjectEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new VisitStarSystemGameCommand(value2);
	}
}
