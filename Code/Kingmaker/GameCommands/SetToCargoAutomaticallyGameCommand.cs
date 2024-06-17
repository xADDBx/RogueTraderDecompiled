using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class SetToCargoAutomaticallyGameCommand : GameCommandWithSynchronized, IMemoryPackable<SetToCargoAutomaticallyGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class SetToCargoAutomaticallyGameCommandFormatter : MemoryPackFormatter<SetToCargoAutomaticallyGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref SetToCargoAutomaticallyGameCommand value)
		{
			SetToCargoAutomaticallyGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref SetToCargoAutomaticallyGameCommand value)
		{
			SetToCargoAutomaticallyGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintItemReference m_BlueprintItemReference;

	[JsonProperty]
	[MemoryPackInclude]
	private bool m_ToCargo;

	[JsonConstructor]
	private SetToCargoAutomaticallyGameCommand()
	{
	}

	[MemoryPackConstructor]
	private SetToCargoAutomaticallyGameCommand([NotNull] BlueprintItemReference m_blueprintItemReference, bool m_toCargo)
	{
		m_BlueprintItemReference = m_blueprintItemReference;
		m_ToCargo = m_toCargo;
	}

	public SetToCargoAutomaticallyGameCommand([NotNull] BlueprintItem blueprintItem, bool toCargo, bool isSynchronized)
		: this(blueprintItem.ToReference<BlueprintItemReference>(), toCargo)
	{
		m_IsSynchronized = isSynchronized;
	}

	protected override void ExecuteInternal()
	{
		BlueprintItem blueprintItem = m_BlueprintItemReference.Get();
		if (blueprintItem == null)
		{
			PFLog.GameCommands.Error("[SetToCargoAutomaticallyGameCommand] BlueprintItem not found #" + m_BlueprintItemReference.Guid);
			return;
		}
		HashSet<BlueprintItem> itemsToCargo = Game.Instance.Player.ItemsToCargo;
		if ((!m_ToCargo) ? itemsToCargo.Remove(blueprintItem) : itemsToCargo.Add(blueprintItem))
		{
			EventBus.RaiseEvent(delegate(IToCargoAutomaticallyChangedHandler h)
			{
				h.HandleToCargoAutomaticallyChanged(blueprintItem);
			});
		}
	}

	static SetToCargoAutomaticallyGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<SetToCargoAutomaticallyGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new SetToCargoAutomaticallyGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<SetToCargoAutomaticallyGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<SetToCargoAutomaticallyGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref SetToCargoAutomaticallyGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_BlueprintItemReference);
		writer.WriteUnmanaged(in value.m_ToCargo);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref SetToCargoAutomaticallyGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintItemReference value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintItemReference>();
				reader.ReadUnmanaged<bool>(out value3);
			}
			else
			{
				value2 = value.m_BlueprintItemReference;
				value3 = value.m_ToCargo;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(SetToCargoAutomaticallyGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = false;
			}
			else
			{
				value2 = value.m_BlueprintItemReference;
				value3 = value.m_ToCargo;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new SetToCargoAutomaticallyGameCommand(value2, value3);
	}
}
