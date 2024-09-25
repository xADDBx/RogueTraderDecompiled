using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetEquipmentColorGameCommand : GameCommand, IMemoryPackable<CharGenSetEquipmentColorGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetEquipmentColorGameCommandFormatter : MemoryPackFormatter<CharGenSetEquipmentColorGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetEquipmentColorGameCommand value)
		{
			CharGenSetEquipmentColorGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetEquipmentColorGameCommand value)
		{
			CharGenSetEquipmentColorGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_PrimaryIndex;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_SecondaryIndex;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetEquipmentColorGameCommand(int m_primaryIndex, int m_secondaryIndex)
	{
		m_PrimaryIndex = m_primaryIndex;
		m_SecondaryIndex = m_secondaryIndex;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetEquipmentColor(m_PrimaryIndex, m_SecondaryIndex);
		});
	}

	static CharGenSetEquipmentColorGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetEquipmentColorGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetEquipmentColorGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetEquipmentColorGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetEquipmentColorGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetEquipmentColorGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_PrimaryIndex, in value.m_SecondaryIndex);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetEquipmentColorGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		int value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<int, int>(out value2, out value3);
			}
			else
			{
				value2 = value.m_PrimaryIndex;
				value3 = value.m_SecondaryIndex;
				reader.ReadUnmanaged<int>(out value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetEquipmentColorGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = 0;
				value3 = 0;
			}
			else
			{
				value2 = value.m_PrimaryIndex;
				value3 = value.m_SecondaryIndex;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<int>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetEquipmentColorGameCommand(value2, value3);
	}
}
