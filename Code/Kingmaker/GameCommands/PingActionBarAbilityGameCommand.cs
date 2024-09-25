using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Warhammer.SpaceCombat.Blueprints.Slots;

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

	[JsonProperty]
	[MemoryPackInclude]
	private WeaponSlotType m_WeaponSlotType;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	private PingActionBarAbilityGameCommand()
	{
	}

	[MemoryPackConstructor]
	public PingActionBarAbilityGameCommand(string m_keyName, Entity m_characterEntityRef, int m_slotIndex, WeaponSlotType m_weaponSlotType)
		: this()
	{
		m_KeyName = m_keyName;
		m_CharacterEntityRef = m_characterEntityRef.Ref;
		m_SlotIndex = m_slotIndex;
		m_WeaponSlotType = m_weaponSlotType;
	}

	protected override void ExecuteInternal()
	{
		NetPlayer playerOrEmpty = GameCommandPlayer.GetPlayerOrEmpty();
		PhotonManager.Ping.PingActionBarAbilityLocally(playerOrEmpty, m_KeyName, m_CharacterEntityRef, m_SlotIndex, m_WeaponSlotType);
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
		if (!MemoryPackFormatterProvider.IsRegistered<WeaponSlotType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<WeaponSlotType>());
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
		writer.WriteObjectHeader(4);
		writer.WriteString(value.m_KeyName);
		writer.WritePackable(in value.m_CharacterEntityRef);
		writer.WriteUnmanaged(in value.m_SlotIndex, in value.m_WeaponSlotType);
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
		WeaponSlotType value4;
		string keyName;
		if (memberCount == 4)
		{
			if (value == null)
			{
				keyName = reader.ReadString();
				value2 = reader.ReadPackable<EntityRef>();
				reader.ReadUnmanaged<int, WeaponSlotType>(out value3, out value4);
			}
			else
			{
				keyName = value.m_KeyName;
				value2 = value.m_CharacterEntityRef;
				value3 = value.m_SlotIndex;
				value4 = value.m_WeaponSlotType;
				keyName = reader.ReadString();
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<WeaponSlotType>(out value4);
			}
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(PingActionBarAbilityGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				keyName = null;
				value2 = default(EntityRef);
				value3 = 0;
				value4 = WeaponSlotType.Dorsal;
			}
			else
			{
				keyName = value.m_KeyName;
				value2 = value.m_CharacterEntityRef;
				value3 = value.m_SlotIndex;
				value4 = value.m_WeaponSlotType;
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
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<WeaponSlotType>(out value4);
							_ = 4;
						}
					}
				}
			}
			_ = value;
		}
		value = new PingActionBarAbilityGameCommand(keyName, value2, value3, value4);
	}
}
