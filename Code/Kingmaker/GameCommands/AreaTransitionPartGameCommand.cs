using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.MapObjects;
using MemoryPack;
using MemoryPack.Formatters;
using MemoryPack.Internal;
using Newtonsoft.Json;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.Object)]
public sealed class AreaTransitionPartGameCommand : GameCommandWithSynchronized, IMemoryPackable<AreaTransitionPartGameCommand>, IMemoryPackFormatterRegister
{
	public class TransitionExecutorEntity : ContextData<TransitionExecutorEntity>
	{
		public EntityRef<BaseUnitEntity> EntityRef { get; private set; }

		public TransitionExecutorEntity Setup(EntityRef<BaseUnitEntity> entityRef)
		{
			EntityRef = entityRef;
			return this;
		}

		protected override void Reset()
		{
			EntityRef = null;
		}
	}

	[Preserve]
	private sealed class AreaTransitionPartGameCommandFormatter : MemoryPackFormatter<AreaTransitionPartGameCommand>
	{
		[Preserve]
		public override void Serialize(ref MemoryPackWriter writer, ref AreaTransitionPartGameCommand value)
		{
			AreaTransitionPartGameCommand.Serialize(ref writer, ref value);
		}

		[Preserve]
		public override void Deserialize(ref MemoryPackReader reader, ref AreaTransitionPartGameCommand value)
		{
			AreaTransitionPartGameCommand.Deserialize(ref reader, ref value);
		}
	}

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityPartRef<Entity, AreaTransitionPart> m_AreaTransitionPartRef;

	[JsonProperty]
	[MemoryPackInclude]
	private readonly EntityRef<BaseUnitEntity> m_ExecutorEntity;

	[JsonConstructor]
	public AreaTransitionPartGameCommand()
	{
	}

	[MemoryPackConstructor]
	private AreaTransitionPartGameCommand(EntityPartRef<Entity, AreaTransitionPart> m_areaTransitionPartRef, EntityRef<BaseUnitEntity> m_executorEntity)
	{
		m_AreaTransitionPartRef = m_areaTransitionPartRef;
		m_ExecutorEntity = m_executorEntity;
	}

	public AreaTransitionPartGameCommand([NotNull] AreaTransitionPart areaTransitionPart, bool isPlayerCommand, BaseUnitEntity executorEntity)
		: this(areaTransitionPart, executorEntity)
	{
		m_IsSynchronized = isPlayerCommand;
	}

	protected override void ExecuteInternal()
	{
		AreaTransitionPart entityPart = m_AreaTransitionPartRef.EntityPart;
		if (entityPart == null)
		{
			return;
		}
		if (Game.Instance.Player.IsInCombat)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to IsInCombat=true");
			return;
		}
		if (Game.Instance.CurrentMode == GameModeType.Dialog)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to CurrentMode=Dialog");
			return;
		}
		if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			PFLog.GameCommands.Log("[AreaTransitionPartGameCommand] Canceled due to CurrentMode=Cutscene");
			return;
		}
		if (Game.Instance.IsPaused)
		{
			Game.Instance.IsPaused = false;
		}
		BaseUnitEntity user = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity u) => u.IsDirectlyControllable).FirstOrDefault((BaseUnitEntity u) => !u.IsPet);
		if (!entityPart.CheckRestrictions(user))
		{
			return;
		}
		ConditionAction conditionAction = entityPart.Blueprint?.Actions.FirstOrDefault((ConditionAction ca) => ca.Condition?.Check() ?? true);
		if (conditionAction != null)
		{
			using (ContextData<TransitionExecutorEntity>.Request().Setup(m_ExecutorEntity))
			{
				conditionAction.Actions.Run();
				return;
			}
		}
		BlueprintArea currentArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintAreaEnterPoint targetEnterPoint = entityPart.AreaEnterPoint;
		EventBus.RaiseEvent(delegate(IPartyLeaveAreaHandler h)
		{
			h.HandlePartyLeaveArea(currentArea, targetEnterPoint);
		});
		Game.Instance.LoadArea(targetEnterPoint, entityPart.Settings.AutoSaveMode);
	}

	static AreaTransitionPartGameCommand()
	{
		RegisterFormatter();
	}

	[Preserve]
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<AreaTransitionPartGameCommand>())
		{
			MemoryPackFormatterProvider.Register(new AreaTransitionPartGameCommandFormatter());
		}
		if (!MemoryPackFormatterProvider.IsRegistered<AreaTransitionPartGameCommand[]>())
		{
			MemoryPackFormatterProvider.Register(new ArrayFormatter<AreaTransitionPartGameCommand>());
		}
	}

	[Preserve]
	public static void Serialize(ref MemoryPackWriter writer, ref AreaTransitionPartGameCommand? value)
	{
		if (value == null)
		{
			writer.WriteNullObjectHeader();
			return;
		}
		writer.WriteObjectHeader(2);
		writer.WritePackable(in value.m_AreaTransitionPartRef);
		writer.WritePackable(in value.m_ExecutorEntity);
	}

	[Preserve]
	public static void Deserialize(ref MemoryPackReader reader, ref AreaTransitionPartGameCommand? value)
	{
		if (!reader.TryReadObjectHeader(out var memberCount))
		{
			value = null;
			return;
		}
		EntityPartRef<Entity, AreaTransitionPart> value2;
		EntityRef<BaseUnitEntity> value3;
		if (memberCount == 2)
		{
			if (value == null)
			{
				value2 = reader.ReadPackable<EntityPartRef<Entity, AreaTransitionPart>>();
				value3 = reader.ReadPackable<EntityRef<BaseUnitEntity>>();
			}
			else
			{
				value2 = value.m_AreaTransitionPartRef;
				value3 = value.m_ExecutorEntity;
				reader.ReadPackable(ref value2);
				reader.ReadPackable(ref value3);
			}
		}
		else
		{
			if (memberCount > 2)
			{
				MemoryPackSerializationException.ThrowInvalidPropertyCount(typeof(AreaTransitionPartGameCommand), 2, memberCount);
				return;
			}
			if (value == null)
			{
				value2 = default(EntityPartRef<Entity, AreaTransitionPart>);
				value3 = default(EntityRef<BaseUnitEntity>);
			}
			else
			{
				value2 = value.m_AreaTransitionPartRef;
				value3 = value.m_ExecutorEntity;
			}
			if (memberCount != 0)
			{
				reader.ReadPackable(ref value2);
				if (memberCount != 1)
				{
					reader.ReadPackable(ref value3);
					_ = 2;
				}
			}
			_ = value;
		}
		value = new AreaTransitionPartGameCommand(value2, value3);
	}
}
