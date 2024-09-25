using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.SectorMap;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class LowerWarpRouteDifficultyGameCommand : GameCommand, IMemoryPackable<LowerWarpRouteDifficultyGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class LowerWarpRouteDifficultyGameCommandFormatter : MemoryPackFormatter<LowerWarpRouteDifficultyGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref LowerWarpRouteDifficultyGameCommand value)
		{
			LowerWarpRouteDifficultyGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref LowerWarpRouteDifficultyGameCommand value)
		{
			LowerWarpRouteDifficultyGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<SectorMapObjectEntity> m_To;

	[JsonProperty]
	[MemoryPackInclude]
	private SectorMapPassageEntity.PassageDifficulty m_Difficulty;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private LowerWarpRouteDifficultyGameCommand()
	{
	}

	[JsonConstructor]
	public LowerWarpRouteDifficultyGameCommand([NotNull] SectorMapObjectEntity to, [NotNull] SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		m_To = to;
		m_Difficulty = difficulty;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.SectorMapController.LowerPassageDifficulty(m_To.Entity, m_Difficulty);
	}

	static LowerWarpRouteDifficultyGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<LowerWarpRouteDifficultyGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new LowerWarpRouteDifficultyGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<LowerWarpRouteDifficultyGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<LowerWarpRouteDifficultyGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SectorMapPassageEntity.PassageDifficulty>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<SectorMapPassageEntity.PassageDifficulty>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref LowerWarpRouteDifficultyGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_To);
		writer.WriteUnmanaged(in value.m_Difficulty);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref LowerWarpRouteDifficultyGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<SectorMapObjectEntity> value2;
		SectorMapPassageEntity.PassageDifficulty value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_To;
				value3 = value.m_Difficulty;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<SectorMapPassageEntity.PassageDifficulty>(out value3);
				goto IL_00a1;
			}
			value2 = reader.ReadPackable<EntityRef<SectorMapObjectEntity>>();
			reader.ReadUnmanaged<SectorMapPassageEntity.PassageDifficulty>(out value3);
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(LowerWarpRouteDifficultyGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<SectorMapObjectEntity>);
				value3 = SectorMapPassageEntity.PassageDifficulty.Safe;
			}
			else
			{
				value2 = value.m_To;
				value3 = value.m_Difficulty;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<SectorMapPassageEntity.PassageDifficulty>(out value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_00a1;
			}
		}
		value = new LowerWarpRouteDifficultyGameCommand
		{
			m_To = value2,
			m_Difficulty = value3
		};
		return;
		IL_00a1:
		value.m_To = value2;
		value.m_Difficulty = value3;
	}
}
