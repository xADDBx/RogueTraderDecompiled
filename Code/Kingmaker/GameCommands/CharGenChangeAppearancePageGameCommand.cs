using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenChangeAppearancePageGameCommand : GameCommand, IMemoryPackable<CharGenChangeAppearancePageGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenChangeAppearancePageGameCommandFormatter : MemoryPackFormatter<CharGenChangeAppearancePageGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenChangeAppearancePageGameCommand value)
		{
			CharGenChangeAppearancePageGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenChangeAppearancePageGameCommand value)
		{
			CharGenChangeAppearancePageGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly CharGenAppearancePageType m_PageType;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenChangeAppearancePageGameCommand(CharGenAppearancePageType m_pageType)
	{
		m_PageType = m_pageType;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhaseHandler h)
		{
			h.HandleAppearancePageChange(m_PageType);
		});
	}

	static CharGenChangeAppearancePageGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangeAppearancePageGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenChangeAppearancePageGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangeAppearancePageGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenChangeAppearancePageGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenAppearancePageType>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<CharGenAppearancePageType>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenChangeAppearancePageGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_PageType);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenChangeAppearancePageGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		CharGenAppearancePageType value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<CharGenAppearancePageType>(out value2);
			}
			else
			{
				value2 = value.m_PageType;
				reader.ReadUnmanaged<CharGenAppearancePageType>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenChangeAppearancePageGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_PageType : CharGenAppearancePageType.Portrait);
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<CharGenAppearancePageType>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenChangeAppearancePageGameCommand(value2);
	}
}
