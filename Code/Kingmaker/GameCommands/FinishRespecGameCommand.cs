using System;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class FinishRespecGameCommand : GameCommand, IMemoryPackable<FinishRespecGameCommand>, IMemoryPackFormatterRegister
{
	[Preserve]
	private sealed class FinishRespecGameCommandFormatter : MemoryPackFormatter<FinishRespecGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref FinishRespecGameCommand value)
		{
			FinishRespecGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref FinishRespecGameCommand value)
		{
			FinishRespecGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_RespecEntity;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly bool m_ForFree;

	public override bool IsSynchronized => true;

	[JsonConstructor]
	public FinishRespecGameCommand()
	{
	}

	[MemoryPackConstructor]
	private FinishRespecGameCommand(EntityRef<BaseUnitEntity> m_respecEntity, bool m_forfree)
	{
		m_RespecEntity = m_respecEntity;
		m_ForFree = m_forfree;
	}

	public FinishRespecGameCommand(BaseUnitEntity respecEntity, bool forFree)
		: this((EntityRef<BaseUnitEntity>)respecEntity, forFree)
	{
	}

	protected override void ExecuteInternal()
	{
		Player player = Game.Instance.Player;
		PartUnitProgression progression = m_RespecEntity.Entity.Progression;
		progression.Respec();
		if (!m_ForFree)
		{
			player.ProfitFactor.AddModifier(-progression.GetRespecCost(), ProfitFactorModifierType.Respec);
			progression.CountRespecIn();
		}
		Game.Instance.AdvanceGameTime(1.Days());
		EventBus.RaiseEvent((IBaseUnitEntity)m_RespecEntity.Entity, (Action<IRespecHandler>)delegate(IRespecHandler h)
		{
			h.HandleRespecFinished();
		}, isCheckRuntime: true);
	}

	static FinishRespecGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<FinishRespecGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new FinishRespecGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<FinishRespecGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<FinishRespecGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref FinishRespecGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_RespecEntity);
		writer.WriteUnmanaged(in value.m_ForFree);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref FinishRespecGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityRef<BaseUnitEntity> value2;
		bool value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
				reader.ReadUnmanaged<bool>(out value3);
			}
			else
			{
				value2 = value.m_RespecEntity;
				value3 = value.m_ForFree;
				reader.ReadPackable(ref value2);
				reader.ReadUnmanaged<bool>(out value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(FinishRespecGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityRef<BaseUnitEntity>);
				value3 = false;
			}
			else
			{
				value2 = value.m_RespecEntity;
				value3 = value.m_ForFree;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadUnmanaged<bool>(out value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new FinishRespecGameCommand(value2, value3);
	}
}
