using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.Visual.Sound;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenChangeVoiceGameCommand : GameCommand, IMemoryPackable<CharGenChangeVoiceGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenChangeVoiceGameCommandFormatter : MemoryPackFormatter<CharGenChangeVoiceGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenChangeVoiceGameCommand value)
		{
			CharGenChangeVoiceGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenChangeVoiceGameCommand value)
		{
			CharGenChangeVoiceGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintUnitAsksListReference m_Blueprint;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenChangeVoiceGameCommand([NotNull] BlueprintUnitAsksListReference m_blueprint)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
	}

	public CharGenChangeVoiceGameCommand([NotNull] BlueprintUnitAsksList blueprint)
		: this(blueprint.ToReference<BlueprintUnitAsksListReference>())
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintUnitAsksList blueprint = m_Blueprint;
		if (blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenChangeVoiceGameCommand] BlueprintUnitAsksList was not found id=" + m_Blueprint.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenAppearancePhaseVoiceHandler h)
		{
			h.HandleChangeVoice(blueprint);
		});
	}

	static CharGenChangeVoiceGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangeVoiceGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenChangeVoiceGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenChangeVoiceGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenChangeVoiceGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenChangeVoiceGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_Blueprint);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenChangeVoiceGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintUnitAsksListReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintUnitAsksListReference>();
			}
			else
			{
				value2 = value.m_Blueprint;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenChangeVoiceGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_Blueprint : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenChangeVoiceGameCommand(value2);
	}
}
