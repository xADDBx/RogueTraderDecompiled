using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class AddRemoveItemAsFavoriteCommand : GameCommand, IMemoryPackable<AddRemoveItemAsFavoriteCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AddRemoveItemAsFavoriteCommandFormatter : MemoryPackFormatter<AddRemoveItemAsFavoriteCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AddRemoveItemAsFavoriteCommand value)
		{
			AddRemoveItemAsFavoriteCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AddRemoveItemAsFavoriteCommand value)
		{
			AddRemoveItemAsFavoriteCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private EntityRef<ItemEntity> m_Item;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private AddRemoveItemAsFavoriteCommand()
	{
	}

	[JsonConstructor]
	public AddRemoveItemAsFavoriteCommand(ItemEntity item)
	{
		m_Item = new EntityRef<ItemEntity>(item);
	}

	protected override void ExecuteInternal()
	{
		if (m_Item.Entity != null)
		{
			m_Item.Entity.IsFavorite = !m_Item.Entity.IsFavorite;
		}
	}

	static AddRemoveItemAsFavoriteCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AddRemoveItemAsFavoriteCommand>())
		{
			MemoryPackFormatterProvider.Register(new AddRemoveItemAsFavoriteCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AddRemoveItemAsFavoriteCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AddRemoveItemAsFavoriteCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AddRemoveItemAsFavoriteCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Item);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AddRemoveItemAsFavoriteCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_Item;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AddRemoveItemAsFavoriteCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Item : default(EntityRef<ItemEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_0070;
			}
		}
		value = new AddRemoveItemAsFavoriteCommand
		{
			m_Item = value2
		};
		return;
		IL_0070:
		value.m_Item = value2;
	}
}
