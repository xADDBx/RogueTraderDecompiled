using System;
using JetBrains.Annotations;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetNameGameCommand : GameCommand, IMemoryPackable<CharGenSetNameGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetNameGameCommandFormatter : MemoryPackFormatter<CharGenSetNameGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetNameGameCommand value)
		{
			CharGenSetNameGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetNameGameCommand value)
		{
			CharGenSetNameGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly string m_Name;

	public override bool IsSynchronized => true;

	public override bool IsForcedSynced => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	public CharGenSetNameGameCommand([NotNull] string m_name)
	{
		if (m_name == null)
		{
			throw new ArgumentNullException("m_name");
		}
		m_Name = m_name;
	}

	protected override void ExecuteInternal()
	{
		EventBus.RaiseEvent(delegate(ICharGenSummaryPhaseHandler h)
		{
			h.HandleSetName(m_Name);
		});
	}

	static CharGenSetNameGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetNameGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetNameGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetNameGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetNameGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetNameGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WriteString(value.m_Name);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetNameGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		string name;
		if (memberCount == 1)
		{
			if (value == null)
			{
				name = reader.ReadString();
			}
			else
			{
				name = value.m_Name;
				name = reader.ReadString();
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetNameGameCommand), 1, memberCount);
				return;
			}
			name = ((value != null) ? value.m_Name : null);
			if (memberCount != 0)
			{
				name = reader.ReadString();
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSetNameGameCommand(name);
	}
}
