using System;
using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetPortGameCommand : GameCommand, IMemoryPackable<CharGenSetPortGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetPortGameCommandFormatter : MemoryPackFormatter<CharGenSetPortGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetPortGameCommand value)
		{
			CharGenSetPortGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortGameCommand value)
		{
			CharGenSetPortGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EquipmentEntityLink m_EquipmentEntityLink;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_Index;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_PortNumber;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetPortGameCommand([NotNull] EquipmentEntityLink m_equipmentEntityLink, int m_index, int m_portNumber)
	{
		if (m_equipmentEntityLink == null)
		{
			throw new ArgumentNullException("m_equipmentEntityLink");
		}
		m_EquipmentEntityLink = m_equipmentEntityLink;
		m_Index = m_index;
		m_PortNumber = m_portNumber;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetPort(m_EquipmentEntityLink, m_Index, m_PortNumber);
		});
	}

	static CharGenSetPortGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetPortGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetPortGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetPortGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_EquipmentEntityLink);
		writer.WriteUnmanaged(in value.m_Index, in value.m_PortNumber);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EquipmentEntityLink value2;
		int value3;
		int value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EquipmentEntityLink>();
				reader.ReadUnmanaged<int, int>(out value3, out value4);
			}
			else
			{
				value2 = value.m_EquipmentEntityLink;
				value3 = value.m_Index;
				value4 = value.m_PortNumber;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
				reader.ReadUnmanaged<int>(out value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetPortGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
				value4 = 0;
			}
			else
			{
				value2 = value.m_EquipmentEntityLink;
				value3 = value.m_Index;
				value4 = value.m_PortNumber;
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
		value = new CharGenSetPortGameCommand(value2, value3, value4);
	}
}
