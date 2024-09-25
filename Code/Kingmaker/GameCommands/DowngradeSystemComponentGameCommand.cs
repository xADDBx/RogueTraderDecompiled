using System;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class DowngradeSystemComponentGameCommand : GameCommand, IMemoryPackable<DowngradeSystemComponentGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class DowngradeSystemComponentGameCommandFormatter : MemoryPackFormatter<DowngradeSystemComponentGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref DowngradeSystemComponentGameCommand value)
		{
			DowngradeSystemComponentGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref DowngradeSystemComponentGameCommand value)
		{
			DowngradeSystemComponentGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private SystemComponent.SystemComponentType m_ComponentType;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private DowngradeSystemComponentGameCommand()
	{
	}

	[JsonConstructor]
	public DowngradeSystemComponentGameCommand(SystemComponent.SystemComponentType componentType)
	{
		m_ComponentType = componentType;
	}

	protected override void ExecuteInternal()
	{
		switch (m_ComponentType)
		{
		case SystemComponent.SystemComponentType.InternalStructure:
			Game.Instance.Player.PlayerShip.Hull.InternalStructure.Downgrade();
			break;
		case SystemComponent.SystemComponentType.ProwRam:
			Game.Instance.Player.PlayerShip.Hull.ProwRam.Downgrade();
			break;
		default:
			throw new ArgumentOutOfRangeException("Unknown Ship component type met");
		}
	}

	static DowngradeSystemComponentGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<DowngradeSystemComponentGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new DowngradeSystemComponentGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<DowngradeSystemComponentGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<DowngradeSystemComponentGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SystemComponent.SystemComponentType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<SystemComponent.SystemComponentType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref DowngradeSystemComponentGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref DowngradeSystemComponentGameCommand? value)
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(DowngradeSystemComponentGameCommand), 1, memberCount);
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
		value = new DowngradeSystemComponentGameCommand
		{
			m_ComponentType = value2
		};
		return;
		IL_006b:
		value.m_ComponentType = value2;
	}
}
