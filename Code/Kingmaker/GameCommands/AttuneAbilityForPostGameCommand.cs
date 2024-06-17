using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class AttuneAbilityForPostGameCommand : GameCommand, IMemoryPackable<AttuneAbilityForPostGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AttuneAbilityForPostGameCommandFormatter : MemoryPackFormatter<AttuneAbilityForPostGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AttuneAbilityForPostGameCommand value)
		{
			AttuneAbilityForPostGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AttuneAbilityForPostGameCommand value)
		{
			AttuneAbilityForPostGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<StarshipEntity> m_Starship;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly PostType m_PostType;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintAbilityReference m_BlueprintAbility;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private AttuneAbilityForPostGameCommand()
	{
	}

	[MemoryPackConstructor]
	private AttuneAbilityForPostGameCommand(EntityRef<StarshipEntity> m_starship, PostType m_postType, BlueprintAbilityReference m_blueprintAbility)
	{
		m_Starship = m_starship;
		m_PostType = m_postType;
		m_BlueprintAbility = m_blueprintAbility;
	}

	public AttuneAbilityForPostGameCommand(Post post, BlueprintAbility blueprintAbility)
		: this(post.ShipRef, post.PostType, blueprintAbility.ToReference<BlueprintAbilityReference>())
	{
	}

	protected override void ExecuteInternal()
	{
		Post post = m_Starship.Entity?.Hull.Posts.FirstOrDefault((Post p) => p.PostType == m_PostType);
		if (post == null)
		{
			PFLog.GameCommands.Error($"[AttuneAbilityForPostGameCommand] Post not found #{m_Starship.Id} {m_PostType}");
			return;
		}
		BlueprintAbility blueprintAbility = m_BlueprintAbility.Get();
		if (blueprintAbility == null)
		{
			PFLog.GameCommands.Error("[AttuneAbilityForPostGameCommand] Ability not found #" + m_BlueprintAbility.Guid);
		}
		else if (post.TryAttuneAbility(blueprintAbility))
		{
			EventBus.RaiseEvent(delegate(IOnPostAbilityChangeHandler h)
			{
				h.HandlePostAbilityChange((int)m_PostType);
			});
		}
	}

	static AttuneAbilityForPostGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AttuneAbilityForPostGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AttuneAbilityForPostGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AttuneAbilityForPostGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AttuneAbilityForPostGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PostType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<PostType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AttuneAbilityForPostGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_Starship);
		writer.WriteUnmanaged(in value.m_PostType);
		writer.WritePackable(in value.m_BlueprintAbility);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AttuneAbilityForPostGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<StarshipEntity> value2;
		PostType value3;
		BlueprintAbilityReference value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<StarshipEntity>>();
				reader.ReadUnmanaged<PostType>(out value3);
				value4 = reader.ReadPackable<BlueprintAbilityReference>();
			}
			else
			{
				value2 = value.m_Starship;
				value3 = value.m_PostType;
				value4 = value.m_BlueprintAbility;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<PostType>(out value3);
				reader.ReadPackable(ref value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AttuneAbilityForPostGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<StarshipEntity>);
				value3 = PostType.SupremeCommander;
				value4 = null;
			}
			else
			{
				value2 = value.m_Starship;
				value3 = value.m_PostType;
				value4 = value.m_BlueprintAbility;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<PostType>(out value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new AttuneAbilityForPostGameCommand(value2, value3, value4);
	}
}
