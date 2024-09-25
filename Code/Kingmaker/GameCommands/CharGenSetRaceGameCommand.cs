using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UnitLogic.Levelup.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetRaceGameCommand : GameCommand, IMemoryPackable<CharGenSetRaceGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetRaceGameCommandFormatter : MemoryPackFormatter<CharGenSetRaceGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetRaceGameCommand value)
		{
			CharGenSetRaceGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetRaceGameCommand value)
		{
			CharGenSetRaceGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintRaceVisualPresetReference m_Blueprint;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly int m_Index;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSetRaceGameCommand([NotNull] BlueprintRaceVisualPresetReference m_blueprint, int m_index)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
		m_Index = m_index;
	}

	public CharGenSetRaceGameCommand([NotNull] BlueprintRaceVisualPreset blueprint, int m_index)
		: this(blueprint.ToReference<BlueprintRaceVisualPresetReference>(), m_index)
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
	}

	protected override void ExecuteInternal()
	{
		if ((BlueprintRaceVisualPreset)m_Blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenSetRaceGameCommand] BlueprintRaceVisualPreset was not found id=" + m_Blueprint.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenDollStateHandler h)
		{
			h.HandleSetRace(m_Blueprint, m_Index);
		});
	}

	static CharGenSetRaceGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetRaceGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetRaceGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetRaceGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetRaceGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetRaceGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_Blueprint);
		writer.WriteUnmanaged(in value.m_Index);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetRaceGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintRaceVisualPresetReference value2;
		int value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintRaceVisualPresetReference>();
				reader.ReadUnmanaged<int>(out value3);
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_Index;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<int>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetRaceGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = null;
				value3 = 0;
			}
			else
			{
				value2 = value.m_Blueprint;
				value3 = value.m_Index;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<int>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new CharGenSetRaceGameCommand(value2, value3);
	}
}
