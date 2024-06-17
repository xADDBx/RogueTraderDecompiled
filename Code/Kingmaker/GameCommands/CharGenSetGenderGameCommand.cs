using Kingmaker.Blueprints.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetGenderGameCommand : GameCommand, IMemoryPackable<CharGenSetGenderGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetGenderGameCommandFormatter : MemoryPackFormatter<CharGenSetGenderGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetGenderGameCommand value)
		{
			CharGenSetGenderGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetGenderGameCommand value)
		{
			CharGenSetGenderGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly Gender m_Gender;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_Index;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetGenderGameCommand(Gender m_gender, int m_index)
	{
		m_Gender = m_gender;
		m_Index = m_index;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetGender(m_Gender, m_Index);
		});
	}

	static CharGenSetGenderGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetGenderGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetGenderGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetGenderGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetGenderGameCommand>());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<Gender>())
		{
			MemoryPackFormatterProvider.Register(new UnmanagedFormatter<Gender>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetGenderGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
		}
		else
		{
			writer.WriteUnmanagedWithObjectHeader(2, in value.m_Gender, in value.m_Index);
		}
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetGenderGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		Gender value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				reader.ReadUnmanaged<Gender, int>(out value2, out value3);
			}
			else
			{
				value2 = value.m_Gender;
				value3 = value.m_Index;
				reader.ReadUnmanaged<Gender>(out value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetGenderGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = Gender.Male;
				value3 = 0;
			}
			else
			{
				value2 = value.m_Gender;
				value3 = value.m_Index;
			}
			if (memberCount != 0)
			{
				reader.ReadUnmanaged<Gender>(out value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetGenderGameCommand(value2, value3);
	}
}
