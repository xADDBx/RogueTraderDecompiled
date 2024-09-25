using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class ChangePlayerRoleGameCommand : GameCommand, IMemoryPackable<ChangePlayerRoleGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class ChangePlayerRoleGameCommandFormatter : MemoryPackFormatter<ChangePlayerRoleGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref ChangePlayerRoleGameCommand value)
		{
			ChangePlayerRoleGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref ChangePlayerRoleGameCommand value)
		{
			ChangePlayerRoleGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly NetPlayerSerializable m_Player;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly string m_EntityId;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	private ChangePlayerRoleGameCommand()
	{
	}

	[MemoryPackConstructor]
	private ChangePlayerRoleGameCommand(NetPlayerSerializable m_player, string m_entityId)
	{
		m_Player = m_player;
		m_EntityId = m_entityId;
	}

	public ChangePlayerRoleGameCommand(string entityId, NetPlayer player, bool enable)
		: this((NetPlayerSerializable)player, entityId)
	{
	}

	protected override void ExecuteInternal()
	{
		if (m_EntityId != null)
		{
			bool enable = true;
			Game.Instance.CoopData.PlayerRole.ForceSet(m_EntityId, (NetPlayer)m_Player, enable);
		}
	}

	static ChangePlayerRoleGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<ChangePlayerRoleGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new ChangePlayerRoleGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ChangePlayerRoleGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<ChangePlayerRoleGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref ChangePlayerRoleGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteUnmanagedWithObjectHeader(2, in value.m_Player);
		writer.WriteString(value.m_EntityId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref ChangePlayerRoleGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		NetPlayerSerializable value2;
		string entityId;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<NetPlayerSerializable>(out value2);
				entityId = reader.ReadString();
			}
			else
			{
				value2 = value.m_Player;
				entityId = value.m_EntityId;
				reader.ReadUnmanaged<NetPlayerSerializable>(out value2);
				entityId = reader.ReadString();
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(ChangePlayerRoleGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(NetPlayerSerializable);
				entityId = null;
			}
			else
			{
				value2 = value.m_Player;
				entityId = value.m_EntityId;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<NetPlayerSerializable>(out value2);
				if (memberCount != 1)
				{
					entityId = reader.ReadString();
					_ = 2;
				}
			}
			_ = value;
		}
		value = new ChangePlayerRoleGameCommand(value2, entityId);
	}
}
