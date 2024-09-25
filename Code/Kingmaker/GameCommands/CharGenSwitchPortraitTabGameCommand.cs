using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSwitchPortraitTabGameCommand : GameCommand, IMemoryPackable<CharGenSwitchPortraitTabGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSwitchPortraitTabGameCommandFormatter : MemoryPackFormatter<CharGenSwitchPortraitTabGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSwitchPortraitTabGameCommand value)
		{
			CharGenSwitchPortraitTabGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSwitchPortraitTabGameCommand value)
		{
			CharGenSwitchPortraitTabGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly CharGenPortraitTab m_Tab;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSwitchPortraitTabGameCommand(CharGenPortraitTab m_tab)
	{
		m_Tab = m_tab;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhasePortraitHandler h)
		{
			h.HandlePortraitTabChange(m_Tab);
		});
	}

	static CharGenSwitchPortraitTabGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSwitchPortraitTabGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSwitchPortraitTabGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSwitchPortraitTabGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSwitchPortraitTabGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenPortraitTab>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CharGenPortraitTab>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSwitchPortraitTabGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_Tab);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSwitchPortraitTabGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CharGenPortraitTab value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CharGenPortraitTab>(out value2);
			}
			else
			{
				value2 = value.m_Tab;
				reader.ReadUnmanaged<CharGenPortraitTab>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSwitchPortraitTabGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Tab : CharGenPortraitTab.Default);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CharGenPortraitTab>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSwitchPortraitTabGameCommand(value2);
	}
}
