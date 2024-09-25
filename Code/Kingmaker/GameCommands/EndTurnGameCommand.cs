using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class EndTurnGameCommand : GameCommand, IMemoryPackable<EndTurnGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class EndTurnGameCommandFormatter : MemoryPackFormatter<EndTurnGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref EndTurnGameCommand value)
		{
			EndTurnGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref EndTurnGameCommand value)
		{
			EndTurnGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	public readonly EntityRef<MechanicEntity> MechanicEntity;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	[MemoryPackConstructor]
	private EndTurnGameCommand(EntityRef<MechanicEntity> mechanicEntity)
	{
		MechanicEntity = mechanicEntity;
	}

	public EndTurnGameCommand([NotNull] MechanicEntity mechanicEntity)
		: this((EntityRef<MechanicEntity>)mechanicEntity)
	{
		if (mechanicEntity == null)
		{
			throw new ArgumentNullException("mechanicEntity");
		}
	}

	protected override void ExecuteInternal()
	{
		Game.Instance.TurnController.TryEndPlayerTurn(MechanicEntity);
	}

	static EndTurnGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<EndTurnGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new EndTurnGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<EndTurnGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<EndTurnGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref EndTurnGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(1);
		writer.WritePackable(in value.MechanicEntity);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref EndTurnGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<MechanicEntity> value2;
		if (memberCount == 1)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<MechanicEntity>>();
			}
			else
			{
				value2 = value.MechanicEntity;
				reader.ReadPackable(ref value2);
			}
		}
		else
		{
			if (memberCount > 1)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(EndTurnGameCommand), 1, memberCount);
				return;
			}
			value2 = ((value != null) ? value.MechanicEntity : default(EntityRef<MechanicEntity>));
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				_ = 1;
			}
			_ = value;
		}
		value = new EndTurnGameCommand(value2);
	}
}
