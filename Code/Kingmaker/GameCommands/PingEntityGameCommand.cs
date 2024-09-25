using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class PingEntityGameCommand : GameCommand, IMemoryPackable<PingEntityGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PingEntityGameCommandFormatter : MemoryPackFormatter<PingEntityGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PingEntityGameCommand value)
		{
			PingEntityGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingEntityGameCommand value)
		{
			PingEntityGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef m_EntityRef;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingEntityGameCommand()
	{
	}

	[MemoryPackConstructor]
	private PingEntityGameCommand(EntityRef m_entityRef)
	{
		m_EntityRef = m_entityRef;
	}

	public PingEntityGameCommand([NotNull] Entity entity)
		: this(entity.Ref)
	{
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingEntityLocally(playerOrEmpty, m_EntityRef);
	}

	static PingEntityGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingEntityGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingEntityGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingEntityGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingEntityGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PingEntityGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_EntityRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingEntityGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef>();
			}
			else
			{
				value2 = value.m_EntityRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingEntityGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_EntityRef : default(EntityRef));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new PingEntityGameCommand(value2);
	}
}
