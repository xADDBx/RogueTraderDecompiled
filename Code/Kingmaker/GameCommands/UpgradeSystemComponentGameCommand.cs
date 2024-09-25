using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class UpgradeSystemComponentGameCommand : GameCommand, IMemoryPackable<UpgradeSystemComponentGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UpgradeSystemComponentGameCommandFormatter : MemoryPackFormatter<UpgradeSystemComponentGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UpgradeSystemComponentGameCommand value)
		{
			UpgradeSystemComponentGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UpgradeSystemComponentGameCommand value)
		{
			UpgradeSystemComponentGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private SystemComponent.SystemComponentType m_ComponentType;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private UpgradeSystemComponentGameCommand()
	{
	}

	[JsonConstructor]
	public UpgradeSystemComponentGameCommand(SystemComponent.SystemComponentType componentType)
	{
		m_ComponentType = componentType;
	}

	protected override void ExecuteInternal()
	{
		switch (m_ComponentType)
		{
		case SystemComponent.SystemComponentType.InternalStructure:
			Game.Instance.Player.PlayerShip.Hull.InternalStructure.Upgrade();
			break;
		case SystemComponent.SystemComponentType.ProwRam:
			Game.Instance.Player.PlayerShip.Hull.ProwRam.Upgrade();
			break;
		default:
			throw new ArgumentOutOfRangeException("Unknown Ship component type met");
		}
	}

	static UpgradeSystemComponentGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UpgradeSystemComponentGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new UpgradeSystemComponentGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UpgradeSystemComponentGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UpgradeSystemComponentGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SystemComponent.SystemComponentType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<SystemComponent.SystemComponentType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UpgradeSystemComponentGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_ComponentType);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UpgradeSystemComponentGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		SystemComponent.SystemComponentType value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_ComponentType;
				reader.ReadUnmanaged<SystemComponent.SystemComponentType>(out value2);
				goto IL_006b;
			}
			reader.ReadUnmanaged<SystemComponent.SystemComponentType>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UpgradeSystemComponentGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_ComponentType : SystemComponent.SystemComponentType.InternalStructure);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<SystemComponent.SystemComponentType>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new UpgradeSystemComponentGameCommand
		{
			m_ComponentType = value2
		};
		return;
		IL_006b:
		value.m_ComponentType = value2;
	}
}
