using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SetActiveWeaponIndexGameCommand : GameCommand, IMemoryPackable<SetActiveWeaponIndexGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetActiveWeaponIndexGameCommandFormatter : MemoryPackFormatter<SetActiveWeaponIndexGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetActiveWeaponIndexGameCommand value)
		{
			SetActiveWeaponIndexGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetActiveWeaponIndexGameCommand value)
		{
			SetActiveWeaponIndexGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef m_EntityRef;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_WeaponSlotIndex;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_ActiveAbilityId;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public SetActiveWeaponIndexGameCommand(EntityRef m_entityRef, int m_weaponSlotIndex, int m_activeAbilityId)
	{
		m_EntityRef = m_entityRef;
		m_WeaponSlotIndex = m_weaponSlotIndex;
		m_ActiveAbilityId = m_activeAbilityId;
	}

	public SetActiveWeaponIndexGameCommand([NotNull] WeaponSlot weaponSlot, int activeAbilityId)
	{
		if (weaponSlot == null)
		{
			throw new NullReferenceException("weaponSlot");
		}
		if (weaponSlot.Owner == null)
		{
			throw new NullReferenceException("Owner");
		}
		if (!(weaponSlot.Owner is AbstractUnitEntity abstractUnitEntity))
		{
			throw new NullReferenceException("unit");
		}
		int num = (abstractUnitEntity.GetHull()?.WeaponSlots ?? throw new NullReferenceException("weaponSlots")).IndexOf(weaponSlot);
		if (num == -1)
		{
			throw new Exception("Weapon slot was not found!");
		}
		m_EntityRef = abstractUnitEntity.Ref;
		m_WeaponSlotIndex = num;
		m_ActiveAbilityId = activeAbilityId;
	}

	protected override void ExecuteInternal()
	{
		if (!(m_EntityRef.Entity is AbstractUnitEntity unit))
		{
			PFLog.GameCommands.Error("[SetActiveWeaponIndexGameCommand] Unit was not found #" + m_EntityRef.Id);
			return;
		}
		List<WeaponSlot> list = unit.GetHull()?.WeaponSlots;
		WeaponSlot element;
		if (list == null)
		{
			PFLog.GameCommands.Error("[SetActiveWeaponIndexGameCommand] WeaponSlots is null!");
		}
		else if (!list.TryGet(m_WeaponSlotIndex, out element))
		{
			PFLog.GameCommands.Error($"[SetActiveWeaponIndexGameCommand] WeaponSlot with i={m_WeaponSlotIndex} was not found! Count={list.Count}");
		}
		else
		{
			element.ActiveWeaponIndex = m_ActiveAbilityId;
		}
	}

	static SetActiveWeaponIndexGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetActiveWeaponIndexGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetActiveWeaponIndexGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetActiveWeaponIndexGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetActiveWeaponIndexGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetActiveWeaponIndexGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_EntityRef);
		writer.WriteUnmanaged(in value.m_WeaponSlotIndex, in value.m_ActiveAbilityId);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetActiveWeaponIndexGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef value2;
		int value3;
		int value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef>();
				reader.ReadUnmanaged<int, int>(out value3, out value4);
			}
			else
			{
				value2 = value.m_EntityRef;
				value3 = value.m_WeaponSlotIndex;
				value4 = value.m_ActiveAbilityId;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<int>(out value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetActiveWeaponIndexGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef);
				value3 = 0;
				value4 = 0;
			}
			else
			{
				value2 = value.m_EntityRef;
				value3 = value.m_WeaponSlotIndex;
				value4 = value.m_ActiveAbilityId;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<int>(out value4);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new SetActiveWeaponIndexGameCommand(value2, value3, value4);
	}
}
