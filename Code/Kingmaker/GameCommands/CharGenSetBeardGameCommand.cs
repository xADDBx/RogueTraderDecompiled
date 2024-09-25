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
public sealed class CharGenSetBeardGameCommand : GameCommand, IMemoryPackable<CharGenSetBeardGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetBeardGameCommandFormatter : MemoryPackFormatter<CharGenSetBeardGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetBeardGameCommand value)
		{
			CharGenSetBeardGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetBeardGameCommand value)
		{
			CharGenSetBeardGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EquipmentEntityLink m_EquipmentEntityLink;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_Index;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetBeardGameCommand([NotNull] EquipmentEntityLink m_equipmentEntityLink, int m_index)
	{
		if (m_equipmentEntityLink == null)
		{
			throw new ArgumentNullException("m_equipmentEntityLink");
		}
		m_EquipmentEntityLink = m_equipmentEntityLink;
		m_Index = m_index;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetBeard(m_EquipmentEntityLink, m_Index);
		});
	}

	static CharGenSetBeardGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetBeardGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetBeardGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetBeardGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetBeardGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetBeardGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_EquipmentEntityLink);
		writer.WriteUnmanaged(in value.m_Index);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetBeardGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EquipmentEntityLink value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EquipmentEntityLink>();
				reader.ReadUnmanaged<int>(out value3);
			}
			else
			{
				value2 = value.m_EquipmentEntityLink;
				value3 = value.m_Index;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetBeardGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
			}
			else
			{
				value2 = value.m_EquipmentEntityLink;
				value3 = value.m_Index;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetBeardGameCommand(value2, value3);
	}
}
