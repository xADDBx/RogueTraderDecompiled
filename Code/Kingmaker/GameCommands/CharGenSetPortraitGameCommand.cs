using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetPortraitGameCommand : GameCommand, IMemoryPackable<CharGenSetPortraitGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetPortraitGameCommandFormatter : MemoryPackFormatter<CharGenSetPortraitGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetPortraitGameCommand value)
		{
			CharGenSetPortraitGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortraitGameCommand value)
		{
			CharGenSetPortraitGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintPortraitReference m_Blueprint;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSetPortraitGameCommand([NotNull] BlueprintPortraitReference m_blueprint)
	{
		if (m_blueprint == null)
		{
			throw new ArgumentNullException("m_blueprint");
		}
		m_Blueprint = m_blueprint;
	}

	public CharGenSetPortraitGameCommand([NotNull] BlueprintPortrait blueprint)
		: this(blueprint.ToReference<BlueprintPortraitReference>())
	{
		if (blueprint == null)
		{
			throw new ArgumentNullException("blueprint");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintPortrait blueprint = m_Blueprint;
		if (blueprint == null)
		{
			PFLog.GameCommands.Log("[CharGenSetPortraitGameCommand] BlueprintPortrait was not found id=" + m_Blueprint.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenPortraitHandler h)
		{
			h.HandleSetPortrait(blueprint);
		});
	}

	static CharGenSetPortraitGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortraitGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetPortraitGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetPortraitGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetPortraitGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetPortraitGameCommand? value)
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
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetPortraitGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintPortraitReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintPortraitReference>();
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
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetPortraitGameCommand), 1, memberCount);
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
		value = new CharGenSetPortraitGameCommand(value2);
	}
}
