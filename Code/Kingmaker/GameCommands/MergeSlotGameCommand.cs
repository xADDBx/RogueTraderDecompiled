using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class MergeSlotGameCommand : GameCommand, IMemoryPackable<MergeSlotGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class MergeSlotGameCommandFormatter : MemoryPackFormatter<MergeSlotGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref MergeSlotGameCommand value)
		{
			MergeSlotGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref MergeSlotGameCommand value)
		{
			MergeSlotGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private ItemSlotRef m_From;

	[JsonProperty]
	[MemoryPackInclude]
	private ItemSlotRef m_To;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private MergeSlotGameCommand()
	{
	}

	[JsonConstructor]
	public MergeSlotGameCommand(ItemSlotRef from, ItemSlotRef to)
	{
		m_From = from;
		m_To = to;
	}

	protected override void ExecuteInternal()
	{
		GameCommandHelper.MergeSlot(m_From, m_To);
	}

	static MergeSlotGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<MergeSlotGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new MergeSlotGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<MergeSlotGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<MergeSlotGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref MergeSlotGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_From);
		writer.WritePackable(in value.m_To);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref MergeSlotGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemSlotRef value2;
		ItemSlotRef value3;
		if (memberCount == 2)
		{
			if (value != null)
			{
				value2 = value.m_From;
				value3 = value.m_To;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
				goto IL_009a;
			}
			value2 = reader.ReadPackable<ItemSlotRef>();
			value3 = reader.ReadPackable<ItemSlotRef>();
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(MergeSlotGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = null;
			}
			else
			{
				value2 = value.m_From;
				value3 = value.m_To;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			if (value != null)
			{
				goto IL_009a;
			}
		}
		value = new MergeSlotGameCommand
		{
			m_From = value2,
			m_To = value3
		};
		return;
		IL_009a:
		value.m_From = value2;
		value.m_To = value3;
	}
}
