using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.CharGen;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class CharGenSetShipGameCommand : GameCommand, IMemoryPackable<CharGenSetShipGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class CharGenSetShipGameCommandFormatter : MemoryPackFormatter<CharGenSetShipGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref CharGenSetShipGameCommand value)
		{
			CharGenSetShipGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref CharGenSetShipGameCommand value)
		{
			CharGenSetShipGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly BlueprintStarshipReference m_BlueprintStarship;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private CharGenSetShipGameCommand([NotNull] BlueprintStarshipReference m_blueprintStarship)
	{
		if (m_blueprintStarship == null)
		{
			throw new ArgumentNullException("m_blueprintStarship");
		}
		m_BlueprintStarship = m_blueprintStarship;
	}

	public CharGenSetShipGameCommand([NotNull] BlueprintStarship blueprintStarship)
		: this(blueprintStarship.ToReference<BlueprintStarshipReference>())
	{
		if (blueprintStarship == null)
		{
			throw new ArgumentNullException("blueprintStarship");
		}
	}

	protected override void ExecuteInternal()
	{
		BlueprintStarship blueprintStarship = m_BlueprintStarship;
		if (blueprintStarship == null)
		{
			PFLog.GameCommands.Log("[CharGenSetShipGameCommand] BlueprintStarship was not found id=" + m_BlueprintStarship.Guid);
			return;
		}
		EventBus.RaiseEvent(delegate(ICharGenShipPhaseHandler h)
		{
			h.HandleSetShip(blueprintStarship);
		});
	}

	static CharGenSetShipGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetShipGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new CharGenSetShipGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<CharGenSetShipGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<CharGenSetShipGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref CharGenSetShipGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.m_BlueprintStarship);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref CharGenSetShipGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		BlueprintStarshipReference value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<BlueprintStarshipReference>();
			}
			else
			{
				value2 = value.m_BlueprintStarship;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(CharGenSetShipGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.m_BlueprintStarship : null);
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new CharGenSetShipGameCommand(value2);
	}
}
