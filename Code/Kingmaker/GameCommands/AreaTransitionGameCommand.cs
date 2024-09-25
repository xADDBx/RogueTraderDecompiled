using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem.Core;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class AreaTransitionGameCommand : GameCommand, IMemoryPackable<AreaTransitionGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class AreaTransitionGameCommandFormatter : MemoryPackFormatter<AreaTransitionGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AreaTransitionGameCommand value)
		{
			AreaTransitionGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AreaTransitionGameCommand value)
		{
			AreaTransitionGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintMultiEntranceEntryReference m_MultiEntranceEntryRef;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private AreaTransitionGameCommand()
	{
	}

	public AreaTransitionGameCommand([NotNull] BlueprintMultiEntranceEntry multiEntrance)
	{
		m_MultiEntranceEntryRef = multiEntrance.ToReference<BlueprintMultiEntranceEntryReference>();
	}

	protected override void ExecuteInternal()
	{
		BlueprintMultiEntranceEntry blueprintMultiEntranceEntry = m_MultiEntranceEntryRef?.Get();
		if (blueprintMultiEntranceEntry != null)
		{
			blueprintMultiEntranceEntry.Enter();
			EventBus.RaiseEvent(delegate(IAreaTransitionHandler h)
			{
				h.HandleAreaTransition();
			});
		}
	}

	static AreaTransitionGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AreaTransitionGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AreaTransitionGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AreaTransitionGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AreaTransitionGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AreaTransitionGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_MultiEntranceEntryRef);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AreaTransitionGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintMultiEntranceEntryReference value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_MultiEntranceEntryRef;
				reader.ReadPackable(ref value2);
				goto IL_006a;
			}
			value2 = reader.ReadPackable<BlueprintMultiEntranceEntryReference>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AreaTransitionGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_MultiEntranceEntryRef : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			if (value != null)
			{
				goto IL_006a;
			}
		}
		value = new AreaTransitionGameCommand
		{
			m_MultiEntranceEntryRef = value2
		};
		return;
		IL_006a:
		value.m_MultiEntranceEntryRef = value2;
	}
}
