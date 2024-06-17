using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SectorMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class StartWarpTravelGameCommand : GameCommand, IMemoryPackable<StartWarpTravelGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class StartWarpTravelGameCommandFormatter : MemoryPackFormatter<StartWarpTravelGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref StartWarpTravelGameCommand value)
		{
			StartWarpTravelGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref StartWarpTravelGameCommand value)
		{
			StartWarpTravelGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<SectorMapObjectEntity> m_From;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<SectorMapObjectEntity> m_To;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private StartWarpTravelGameCommand()
	{
	}

	[MemoryPackConstructor]
	private StartWarpTravelGameCommand(EntityRef<SectorMapObjectEntity> m_from, EntityRef<SectorMapObjectEntity> m_to)
	{
		m_From = m_from;
		m_To = m_to;
	}

	public StartWarpTravelGameCommand([NotNull] SectorMapObjectEntity from, [NotNull] SectorMapObjectEntity to)
		: this((EntityRef<SectorMapObjectEntity>)from, (EntityRef<SectorMapObjectEntity>)to)
	{
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.SectorMapTravelController.StartWarpTravel(m_From.Entity, m_To.Entity);
	}

	static StartWarpTravelGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<StartWarpTravelGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new StartWarpTravelGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<StartWarpTravelGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<StartWarpTravelGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref StartWarpTravelGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref StartWarpTravelGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<SectorMapObjectEntity> value2;
		EntityRef<SectorMapObjectEntity> value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<SectorMapObjectEntity>>();
				value3 = reader.ReadPackable<EntityRef<SectorMapObjectEntity>>();
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(StartWarpTravelGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<SectorMapObjectEntity>);
				value3 = default(EntityRef<SectorMapObjectEntity>);
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new StartWarpTravelGameCommand(value2, value3);
	}
}
