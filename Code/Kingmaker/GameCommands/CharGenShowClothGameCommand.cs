using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenShowClothGameCommand : GameCommand, IMemoryPackable<CharGenShowClothGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenShowClothGameCommandFormatter : MemoryPackFormatter<CharGenShowClothGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenShowClothGameCommand value)
		{
			CharGenShowClothGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenShowClothGameCommand value)
		{
			CharGenShowClothGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly bool m_ShowCloth;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenShowClothGameCommand(bool m_showCloth)
	{
		m_ShowCloth = m_showCloth;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleShowCloth(m_ShowCloth);
		});
		EventBus.RaiseEvent(delegate(ICharGenVisualHandler h)
		{
			h.HandleShowCloth(m_ShowCloth);
		});
	}

	static CharGenShowClothGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenShowClothGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenShowClothGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenShowClothGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenShowClothGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenShowClothGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(1, in value.m_ShowCloth);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenShowClothGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		bool value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<bool>(out value2);
			}
			else
			{
				value2 = value.m_ShowCloth;
				reader.ReadUnmanaged<bool>(out value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenShowClothGameCommand), 1, memberCount);
				return;
			}
			value2 = value != null && value.m_ShowCloth;
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<bool>(out value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenShowClothGameCommand(value2);
	}
}
