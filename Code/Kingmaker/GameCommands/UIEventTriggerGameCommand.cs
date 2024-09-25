using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public class UIEventTriggerGameCommand : GameCommand, IMemoryPackable<UIEventTriggerGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class UIEventTriggerGameCommandFormatter : MemoryPackFormatter<UIEventTriggerGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref UIEventTriggerGameCommand value)
		{
			UIEventTriggerGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref UIEventTriggerGameCommand value)
		{
			UIEventTriggerGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private BlueprintComponentReference<UIEventTrigger> m_UIEventTrigger;

	public override bool IsSynchronized => true;

	[MemoryPackConstructor]
	private UIEventTriggerGameCommand()
	{
	}

	[JsonConstructor]
	public UIEventTriggerGameCommand([NotNull] UIEventTrigger uiEventTrigger)
	{
		m_UIEventTrigger = uiEventTrigger;
	}

	protected override void ExecuteInternal()
	{
		UIEventTrigger uIEventTrigger = m_UIEventTrigger.Get();
		if (uIEventTrigger.Conditions.Check())
		{
			uIEventTrigger.Actions.Run();
		}
	}

	static UIEventTriggerGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<UIEventTriggerGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new UIEventTriggerGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<UIEventTriggerGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<UIEventTriggerGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref UIEventTriggerGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_UIEventTrigger);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref UIEventTriggerGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintComponentReference<UIEventTrigger> value2;
		if (memberCount == 1)
		{
			if (value != null)
			{
				value2 = value.m_UIEventTrigger;
				reader.ReadPackable(ref value2);
				goto IL_0070;
			}
			value2 = reader.ReadPackable<BlueprintComponentReference<UIEventTrigger>>();
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(UIEventTriggerGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_UIEventTrigger : default(BlueprintComponentReference<UIEventTrigger>));
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
		value = new UIEventTriggerGameCommand
		{
			m_UIEventTrigger = value2
		};
		return;
		IL_0070:
		value.m_UIEventTrigger = value2;
	}
}
