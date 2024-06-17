using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class PingActionBarAbilityGameCommand : GameCommand, IMemoryPackable<PingActionBarAbilityGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class PingActionBarAbilityGameCommandFormatter : MemoryPackFormatter<PingActionBarAbilityGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref PingActionBarAbilityGameCommand value)
		{
			PingActionBarAbilityGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref PingActionBarAbilityGameCommand value)
		{
			PingActionBarAbilityGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private string m_KeyName;

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef m_CharacterEntityRef;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_SlotIndex;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingActionBarAbilityGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PingActionBarAbilityGameCommand(string m_keyName, Entity m_characterEntityRef, int m_slotIndex)
		: this()
	{
		m_KeyName = m_keyName;
		m_CharacterEntityRef = m_characterEntityRef.Ref;
		m_SlotIndex = m_slotIndex;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingActionBarAbilityLocally(playerOrEmpty, m_KeyName, m_CharacterEntityRef, m_SlotIndex);
	}

	static PingActionBarAbilityGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<PingActionBarAbilityGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new PingActionBarAbilityGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<PingActionBarAbilityGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<PingActionBarAbilityGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref PingActionBarAbilityGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WriteString(value.m_KeyName);
		writer.WritePackable(in value.m_CharacterEntityRef);
		writer.WriteUnmanaged(in value.m_SlotIndex);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref PingActionBarAbilityGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef value2;
		int value3;
		string keyName;
		if (memberCount == 3)
		{
			if (value == null)
			{
				keyName = reader.ReadString();
				value2 = reader.ReadPackable<EntityRef>();
				reader.ReadUnmanaged<int>(out value3);
			}
			else
			{
				keyName = value.m_KeyName;
				value2 = value.m_CharacterEntityRef;
				value3 = value.m_SlotIndex;
				keyName = reader.ReadString();
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingActionBarAbilityGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				keyName = null;
				value2 = default(EntityRef);
				value3 = 0;
			}
			else
			{
				keyName = value.m_KeyName;
				value2 = value.m_CharacterEntityRef;
				value3 = value.m_SlotIndex;
			}
			if (memberCount != 0)
			{
				keyName = reader.ReadString();
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value2);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value3);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new PingActionBarAbilityGameCommand(keyName, value2, value3);
	}
}
