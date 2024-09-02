using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class OpenItemDescriptionGameCommand : GameCommand, IMemoryPackable<OpenItemDescriptionGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class OpenItemDescriptionGameCommandFormatter : MemoryPackFormatter<OpenItemDescriptionGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref OpenItemDescriptionGameCommand value)
		{
			OpenItemDescriptionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref OpenItemDescriptionGameCommand value)
		{
			OpenItemDescriptionGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly EntityRef<ItemEntity> ItemRef;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private OpenItemDescriptionGameCommand(EntityRef<ItemEntity> itemRef)
	{
		ItemRef = itemRef;
	}

	public OpenItemDescriptionGameCommand([NotNull] ItemEntity item)
		: this((EntityRef<ItemEntity>)item)
	{
	}

	protected override void ExecuteInternal()
	{
		ItemEntity entity = ItemRef.Entity;
		if (ItemRef == null)
		{
			PFLog.GameCommands.Error("[OpenItemDescriptionGameCommand] Item was not found " + ItemRef.Id);
			return;
		}
		EventBus.RaiseEvent((IItemEntity)entity, (Action<IPlayerOpenItemDescriptionHandler>)delegate(IPlayerOpenItemDescriptionHandler h)
		{
			h.HandlePlayerOpenItemDescription();
		}, isCheckRuntime: true);
	}

	static OpenItemDescriptionGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<OpenItemDescriptionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new OpenItemDescriptionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<OpenItemDescriptionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<OpenItemDescriptionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref OpenItemDescriptionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.ItemRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref OpenItemDescriptionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<ItemEntity> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<ItemEntity>>();
			}
			else
			{
				value2 = value.ItemRef;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(OpenItemDescriptionGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.ItemRef : default(EntityRef<ItemEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new OpenItemDescriptionGameCommand(value2);
	}
}
