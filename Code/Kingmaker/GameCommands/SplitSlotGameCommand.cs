using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SplitSlotGameCommand : GameCommand, IMemoryPackable<SplitSlotGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SplitSlotGameCommandFormatter : MemoryPackFormatter<SplitSlotGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SplitSlotGameCommand value)
		{
			SplitSlotGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SplitSlotGameCommand value)
		{
			SplitSlotGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private ItemSlotRef m_From;

	[JsonProperty]
	[MemoryPackInclude]
	private ItemSlotRef m_To;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_IsLoot;

	[JsonProperty]
	[MemoryPackInclude]
	private int m_Count;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private SplitSlotGameCommand()
	{
	}

	[JsonConstructor]
	public SplitSlotGameCommand(ItemSlotRef from, ItemSlotRef to, bool isLoot, int count)
	{
		m_From = from;
		m_To = to;
		m_IsLoot = isLoot;
		m_Count = count;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.TrySplitSlot(m_From, m_To, m_IsLoot, m_Count);
	}

	static SplitSlotGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SplitSlotGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SplitSlotGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SplitSlotGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SplitSlotGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SplitSlotGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(4);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
		writer.WriteUnmanaged(in value.m_IsLoot, in value.m_Count);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SplitSlotGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemSlotRef value2;
		ItemSlotRef value3;
		bool value4;
		int value5;
		if (memberCount == 4)
		{
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_IsLoot;
				value5 = value.m_Count;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				reader.ReadUnmanaged<bool>(out value4);
				reader.ReadUnmanaged<int>(out value5);
				goto IL_00f3;
			}
			value2 = reader.ReadPackable<ItemSlotRef>();
			value3 = reader.ReadPackable<ItemSlotRef>();
			reader.ReadUnmanaged<bool, int>(out value4, out value5);
		}
		else
		{
			if (memberCount > 4)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SplitSlotGameCommand), 4, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
				value4 = false;
				value5 = 0;
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
				value4 = value.m_IsLoot;
				value5 = value.m_Count;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					if (memberCount != 2)
					{
						reader.ReadUnmanaged<bool>(out value4);
						if (memberCount != 3)
						{
							reader.ReadUnmanaged<int>(out value5);
							_ = 4;
						}
					}
				}
			}
			if (value != null)
			{
				goto IL_00f3;
			}
		}
		value = new SplitSlotGameCommand
		{
			m_From = value2,
			m_To = value3,
			m_IsLoot = value4,
			m_Count = value5
		};
		return;
		IL_00f3:
		value.m_From = value2;
		value.m_To = value3;
		value.m_IsLoot = value4;
		value.m_Count = value5;
	}
}
