using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.View.MapObjects;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class TriggerLootGameCommand : GameCommandWithSynchronized, IMemoryPackable<TriggerLootGameCommand>, IMemoryPackFormatterRegister
{
	public enum TriggerType : byte
	{
		None,
		Put,
		Take,
		Close
	}

	[Preserve]
	private sealed class TriggerLootGameCommandFormatter : MemoryPackFormatter<TriggerLootGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref TriggerLootGameCommand value)
		{
			TriggerLootGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref TriggerLootGameCommand value)
		{
			TriggerLootGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityPartRef<Entity, InteractionLootPart> m_InteractionLootPartRef;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly byte m_Type;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<ItemEntity> m_Item;

	[JsonConstructor]
	private TriggerLootGameCommand()
	{
	}

	[MemoryPackConstructor]
	private TriggerLootGameCommand(EntityPartRef<Entity, InteractionLootPart> m_interactionLootPartRef, byte m_type, EntityRef<ItemEntity> m_item)
	{
		m_InteractionLootPartRef = m_interactionLootPartRef;
		m_Type = m_type;
		m_Item = m_item;
	}

	public TriggerLootGameCommand(InteractionLootPart interactionLootPart, TriggerType type, ItemEntity item)
		: this(interactionLootPart, (byte)type, item)
	{
		m_IsSynchronized = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
	}

	protected override void ExecuteInternal()
	{
		InteractionLootPart entityPart = m_InteractionLootPartRef.EntityPart;
		if (entityPart != null)
		{
			switch ((TriggerType)m_Type)
			{
			case TriggerType.Put:
				entityPart.HandleItemsAddedImplementation(m_Item.Entity);
				break;
			case TriggerType.Take:
				entityPart.HandleItemsRemovedImplementation(m_Item.Entity);
				break;
			case TriggerType.Close:
				entityPart.OnLootClosedImplementation();
				break;
			default:
				throw new ArgumentOutOfRangeException(string.Format("{0}={1}", "m_Type", m_Type));
			}
		}
	}

	static TriggerLootGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<TriggerLootGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new TriggerLootGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<TriggerLootGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<TriggerLootGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref TriggerLootGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(3);
		writer.WritePackable(in value.m_InteractionLootPartRef);
		writer.WriteUnmanaged(in value.m_Type);
		writer.WritePackable(in value.m_Item);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref TriggerLootGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityPartRef<Entity, InteractionLootPart> value2;
		byte value3;
		EntityRef<ItemEntity> value4;
		if (memberCount == 3)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityPartRef<Entity, InteractionLootPart>>();
				reader.ReadUnmanaged<byte>(out value3);
				value4 = reader.ReadPackable<EntityRef<ItemEntity>>();
			}
			else
			{
				value2 = value.m_InteractionLootPartRef;
				value3 = value.m_Type;
				value4 = value.m_Item;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<byte>(out value3);
				reader.ReadPackable(ref value4);
			}
		}
		else
		{
			if (memberCount > 3)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(TriggerLootGameCommand), 3, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityPartRef<Entity, InteractionLootPart>);
				value3 = 0;
				value4 = default(EntityRef<ItemEntity>);
			}
			else
			{
				value2 = value.m_InteractionLootPartRef;
				value3 = value.m_Type;
				value4 = value.m_Item;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<byte>(out value3);
					if (memberCount != 2)
					{
						reader.ReadPackable(ref value4);
						_ = 3;
					}
				}
			}
			_ = value;
		}
		value = new TriggerLootGameCommand(value2, value3, value4);
	}
}
