using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SetUnitOnPostGameCommand : GameCommand, IMemoryPackable<SetUnitOnPostGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetUnitOnPostGameCommandFormatter : MemoryPackFormatter<SetUnitOnPostGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetUnitOnPostGameCommand value)
		{
			SetUnitOnPostGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetUnitOnPostGameCommand value)
		{
			SetUnitOnPostGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<BaseUnitEntity> m_Unit;

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<StarshipEntity> m_Starship;

	[JsonProperty]
	[MemoryPackInclude]
	private PostType m_PostType;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SetUnitOnPostGameCommand()
	{
	}

	public SetUnitOnPostGameCommand(JsonConstructorMark _)
	{
	}

	public SetUnitOnPostGameCommand(BaseUnitEntity unit, PostType post, StarshipEntity starshipEntity)
	{
		m_Unit = unit;
		m_PostType = post;
		m_Starship = starshipEntity;
	}

	protected override void ExecuteInternal()
	{
		if (m_Unit != null)
		{
			(m_Starship.Entity?.Hull.Posts.FirstOrDefault((Post p) => p.CurrentUnit == m_Unit))?.SetUnitOnPost(null);
		}
		(m_Starship.Entity?.Hull.Posts.FirstOrDefault((Post p) => p.PostType == m_PostType))?.SetUnitOnPost(m_Unit.Entity);
	}

	static SetUnitOnPostGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetUnitOnPostGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetUnitOnPostGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetUnitOnPostGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetUnitOnPostGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PostType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<PostType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetUnitOnPostGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_Unit);
		writer.WritePackable(in value.m_Starship);
		writer.WriteUnmanaged(in value.m_PostType);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetUnitOnPostGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		EntityRef<StarshipEntity> value3;
		PostType value4;
		if (memberCount == 3)
		{
			if (value != null)
			{
				value2 = value.m_Unit;
				value3 = value.m_Starship;
				value4 = value.m_PostType;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<PostType>(out value4);
				goto IL_00d4;
			}
			value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			value3 = reader.ReadPackable<EntityRef<StarshipEntity>>();
			reader.ReadUnmanaged<PostType>(out value4);
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetUnitOnPostGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<BaseUnitEntity>);
				value3 = default(EntityRef<StarshipEntity>);
				value4 = PostType.SupremeCommander;
			}
			else
			{
				value2 = value.m_Unit;
				value3 = value.m_Starship;
				value4 = value.m_PostType;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<PostType>(out value4);
						_ = 3;
					}
				}
			}
			if (value != null)
			{
				goto IL_00d4;
			}
		}
		value = new SetUnitOnPostGameCommand
		{
			m_Unit = value2,
			m_Starship = value3,
			m_PostType = value4
		};
		return;
		IL_00d4:
		value.m_Unit = value2;
		value.m_Starship = value3;
		value.m_PostType = value4;
	}
}
