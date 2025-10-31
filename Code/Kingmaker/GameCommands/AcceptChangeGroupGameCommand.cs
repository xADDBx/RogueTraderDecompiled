using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class AcceptChangeGroupGameCommand : GameCommand, IMemoryPackable<AcceptChangeGroupGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AcceptChangeGroupGameCommandFormatter : MemoryPackFormatter<AcceptChangeGroupGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AcceptChangeGroupGameCommand value)
		{
			AcceptChangeGroupGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AcceptChangeGroupGameCommand value)
		{
			AcceptChangeGroupGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private List<UnitReference> m_PartyCharacters;

	[JsonProperty]
	[MemoryPackInclude]
	private List<UnitReference> m_RemoteCharacters;

	[JsonProperty]
	[MemoryPackInclude]
	private List<BlueprintUnitReference> m_RequiredCharacters;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_IsCapital;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_ReInitPartyCharacters;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private AcceptChangeGroupGameCommand()
	{
	}

	[JsonConstructor]
	public AcceptChangeGroupGameCommand([NotNull] List<UnitReference> partyCharacters, [NotNull] List<UnitReference> remoteCharacters, [NotNull] List<BlueprintUnitReference> requiredCharacters, bool isCapital, bool reInitPartyCharacters)
	{
		m_PartyCharacters = partyCharacters;
		m_RemoteCharacters = remoteCharacters;
		m_RequiredCharacters = requiredCharacters;
		m_IsCapital = isCapital;
		m_ReInitPartyCharacters = reInitPartyCharacters;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.SelectionCharacter.ForceUpdateParty = true;
		if (!CanChangeGroup())
		{
			return;
		}
		if (m_ReInitPartyCharacters)
		{
			Game.Instance.Player.ReInitPartyCharacters(m_PartyCharacters.ToList());
			EventBus.RaiseEvent(delegate(IAcceptChangeGroupHandler h)
			{
				h.HandleAcceptChangeGroup();
			});
			return;
		}
		foreach (UnitReference item in m_PartyCharacters.Where((UnitReference unitRef) => Game.Instance.Player.PartyAndPetsDetached.Contains(unitRef.ToBaseUnitEntity())))
		{
			Game.Instance.Player.AttachPartyMember(item.ToBaseUnitEntity());
		}
		foreach (UnitReference remoteCharacter in m_RemoteCharacters)
		{
			if (Game.Instance.Player.PartyCharacters.Contains(remoteCharacter))
			{
				Game.Instance.Player.DetachPartyMember(remoteCharacter.ToBaseUnitEntity());
			}
		}
		EventBus.RaiseEvent(delegate(IAcceptChangeGroupHandler h)
		{
			h.HandleAcceptChangeGroup();
		});
	}

	private bool CanChangeGroup()
	{
		if (!m_ReInitPartyCharacters)
		{
			if (m_PartyCharacters.Count > 0)
			{
				return m_RemoteCharacters.Count > 0;
			}
			return false;
		}
		if (m_RemoteCharacters.Any((UnitReference v) => MustBeInParty(v.ToBaseUnitEntity())))
		{
			return false;
		}
		return true;
	}

	private bool MustBeInParty(BaseUnitEntity character)
	{
		if (character != Game.Instance.Player.MainCharacterEntity && character.Blueprint.GetComponent<LockedCompanionComponent>() == null && !m_RequiredCharacters.Any((BlueprintUnitReference x) => x.Get() == character.Blueprint))
		{
			return PartPartyLock.IsLocked(character);
		}
		return true;
	}

	static AcceptChangeGroupGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AcceptChangeGroupGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AcceptChangeGroupGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AcceptChangeGroupGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AcceptChangeGroupGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<UnitReference>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<UnitReference>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<List<BlueprintUnitReference>>())
		{
			MemoryPackFormatterProvider.Register(new ListFormatter<BlueprintUnitReference>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AcceptChangeGroupGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(5);
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.m_PartyCharacters));
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.m_RemoteCharacters));
		ListFormatter.SerializePackable(ref writer, ref Unsafe.AsRef(in value.m_RequiredCharacters));
		writer.WriteUnmanaged(in value.m_IsCapital, in value.m_ReInitPartyCharacters);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AcceptChangeGroupGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		List<UnitReference> value2;
		List<UnitReference> value3;
		List<BlueprintUnitReference> value4;
		bool value5;
		bool value6;
		if (memberCount == 5)
		{
			if (value != null)
			{
				value2 = value.m_PartyCharacters;
				value3 = value.m_RemoteCharacters;
				value4 = value.m_RequiredCharacters;
				value5 = value.m_IsCapital;
				value6 = value.m_ReInitPartyCharacters;
				ListFormatter.DeserializePackable(ref reader, ref value2);
				ListFormatter.DeserializePackable(ref reader, ref value3);
				ListFormatter.DeserializePackable(ref reader, ref value4);
				reader.ReadUnmanaged<bool>(out value5);
				reader.ReadUnmanaged<bool>(out value6);
				goto IL_0126;
			}
			value2 = ListFormatter.DeserializePackable<UnitReference>(ref reader);
			value3 = ListFormatter.DeserializePackable<UnitReference>(ref reader);
			value4 = ListFormatter.DeserializePackable<BlueprintUnitReference>(ref reader);
			reader.ReadUnmanaged<bool, bool>(out value5, out value6);
		}
		else
		{
			if (memberCount > 5)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AcceptChangeGroupGameCommand), 5, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = null;
				value5 = false;
				value6 = false;
			}
			else
			{
				value2 = value.m_PartyCharacters;
				value3 = value.m_RemoteCharacters;
				value4 = value.m_RequiredCharacters;
				value5 = value.m_IsCapital;
				value6 = value.m_ReInitPartyCharacters;
			}
			if (memberCount != 0)
			{
				ListFormatter.DeserializePackable(ref reader, ref value2);
				if (memberCount != 1)
				{
					ListFormatter.DeserializePackable(ref reader, ref value3);
					if (memberCount != 2)
					{
						ListFormatter.DeserializePackable(ref reader, ref value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<bool>(out value5);
							if (memberCount != 4)
							{
								reader.ReadUnmanaged<bool>(out value6);
								_ = 5;
							}
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_0126;
			}
		}
		value = new AcceptChangeGroupGameCommand
		{
			m_PartyCharacters = value2,
			m_RemoteCharacters = value3,
			m_RequiredCharacters = value4,
			m_IsCapital = value5,
			m_ReInitPartyCharacters = value6
		};
		return;
		IL_0126:
		value.m_PartyCharacters = value2;
		value.m_RemoteCharacters = value3;
		value.m_RequiredCharacters = value4;
		value.m_IsCapital = value5;
		value.m_ReInitPartyCharacters = value6;
	}
}
