using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class SetInventorySorterGameCommand : GameCommand, IMemoryPackable<SetInventorySorterGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetInventorySorterGameCommandFormatter : MemoryPackFormatter<SetInventorySorterGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetInventorySorterGameCommand value)
		{
			SetInventorySorterGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetInventorySorterGameCommand value)
		{
			SetInventorySorterGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private ItemsSorterType m_SorterType;

	[MemoryPackConstructor]
	private SetInventorySorterGameCommand()
	{
	}

	[JsonConstructor]
	public SetInventorySorterGameCommand(ItemsSorterType sorterType)
	{
		m_SorterType = sorterType;
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.Player.UISettings.InventorySorter = m_SorterType;
		EventBus.RaiseEvent(delegate(ISetInventorySorterHandler h)
		{
			h.HandleSetInventorySorter(m_SorterType);
		});
	}

	static SetInventorySorterGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetInventorySorterGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetInventorySorterGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetInventorySorterGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetInventorySorterGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<ItemsSorterType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<ItemsSorterType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetInventorySorterGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_SorterType);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetInventorySorterGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		ItemsSorterType value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_SorterType;
				reader.ReadUnmanaged<ItemsSorterType>(out value2);
				goto IL_006b;
			}
			reader.ReadUnmanaged<ItemsSorterType>(out value2);
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetInventorySorterGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_SorterType : ItemsSorterType.NotSorted);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<ItemsSorterType>(out value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006b;
			}
		}
		value = new SetInventorySorterGameCommand
		{
			m_SorterType = value2
		};
		return;
		IL_006b:
		value.m_SorterType = value2;
	}
}
