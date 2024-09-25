using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SectorMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class CreateNewWarpRouteGameCommand : GameCommand, IMemoryPackable<CreateNewWarpRouteGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CreateNewWarpRouteGameCommandFormatter : MemoryPackFormatter<CreateNewWarpRouteGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CreateNewWarpRouteGameCommand value)
		{
			CreateNewWarpRouteGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CreateNewWarpRouteGameCommand value)
		{
			CreateNewWarpRouteGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<SectorMapObjectEntity> m_From;

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<SectorMapObjectEntity> m_To;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private CreateNewWarpRouteGameCommand()
	{
	}

	[JsonConstructor]
	public CreateNewWarpRouteGameCommand([NotNull] SectorMapObjectEntity from, [NotNull] SectorMapObjectEntity to)
	{
		m_From = from;
		m_To = to;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.SectorMapController.GenerateNewPassage(m_From.Entity, m_To.Entity);
	}

	static CreateNewWarpRouteGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CreateNewWarpRouteGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CreateNewWarpRouteGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CreateNewWarpRouteGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CreateNewWarpRouteGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CreateNewWarpRouteGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref CreateNewWarpRouteGameCommand? value)
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
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_00a6;
			}
			value2 = reader.ReadPackable<EntityRef<SectorMapObjectEntity>>();
			value3 = reader.ReadPackable<EntityRef<SectorMapObjectEntity>>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CreateNewWarpRouteGameCommand), 2, memberCount);
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
			if (value != null)
			{
				goto IL_00a6;
			}
		}
		value = new CreateNewWarpRouteGameCommand
		{
			m_From = value2,
			m_To = value3
		};
		return;
		IL_00a6:
		value.m_From = value2;
		value.m_To = value3;
	}
}
