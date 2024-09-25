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
public sealed class CharGenSetHeadGameCommand : GameCommand, IMemoryPackable<CharGenSetHeadGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetHeadGameCommandFormatter : MemoryPackFormatter<CharGenSetHeadGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetHeadGameCommand value)
		{
			CharGenSetHeadGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetHeadGameCommand value)
		{
			CharGenSetHeadGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EquipmentEntityLink m_Head;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_Index;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetHeadGameCommand([NotNull] EquipmentEntityLink m_head, int m_index)
	{
		if (m_head == null)
		{
			throw new ArgumentNullException("m_head");
		}
		m_Head = m_head;
		m_Index = m_index;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetHead(m_Head, m_Index);
		});
	}

	static CharGenSetHeadGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetHeadGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetHeadGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetHeadGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetHeadGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetHeadGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Head);
		writer.WriteUnmanaged(in value.m_Index);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetHeadGameCommand? value)
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
				value2 = value.m_Head;
				value3 = value.m_Index;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetHeadGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
			}
			else
			{
				value2 = value.m_Head;
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
		value = new CharGenSetHeadGameCommand(value2, value3);
	}
}
