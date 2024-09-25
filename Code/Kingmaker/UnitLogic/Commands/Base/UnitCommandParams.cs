using System;
using Code.Visual.Animation;
using CodeGenerators.MemoryPackUnionGenerator;
using JetBrains.Annotations;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Kingmaker.View.Covers;
using MemoryPack;
using Newtonsoft.Json;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands.Base;

[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackDynamicUnion]
public abstract class UnitCommandParams
{
	public enum CommandType
	{
		None,
		Run,
		AddToQueue,
		AddToQueueFirst
	}

	public const int InfiniteRange = 10000;

	[JsonProperty(PropertyName = "ct", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public CommandType Type;

	[JsonProperty(PropertyName = "or", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public EntityRef<BaseUnitEntity> OwnerRef;

	[JsonProperty(PropertyName = "fa", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	protected bool? m_FreeAction;

	[JsonProperty(PropertyName = "ls", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	protected bool? m_NeedLoS;

	[JsonProperty(PropertyName = "ar", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	protected int? m_ApproachRadius;

	[JsonProperty(PropertyName = "fp", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[MemoryPackInclude]
	protected ForcedPath m_ForcedPath;

	[JsonProperty(PropertyName = "mt", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	protected WalkSpeedType? m_MovementType;

	[JsonProperty(PropertyName = "of", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	protected bool? m_IsOneFrameCommand;

	[JsonProperty(PropertyName = "sm", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	protected bool? m_SlowMotionRequired;

	[JsonProperty(PropertyName = "tr", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[CanBeNull]
	[MemoryPackInclude]
	public TargetWrapper Target { get; protected set; }

	[JsonProperty(PropertyName = "fc", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[MemoryPackInclude]
	public bool FromCutscene { get; protected set; }

	[MemoryPackIgnore]
	public bool IsSynchronized { get; set; }

	[JsonProperty(PropertyName = "is", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool InterruptAsSoonAsPossible { get; set; }

	[JsonProperty(PropertyName = "ov", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public float? OverrideSpeed { get; set; }

	[JsonProperty(PropertyName = "ni", DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool DoNotInterruptAfterFight { get; set; }

	[MemoryPackIgnore]
	public bool FreeAction
	{
		get
		{
			return m_FreeAction ?? DefaultFreeAction;
		}
		set
		{
			m_FreeAction = value;
		}
	}

	[MemoryPackIgnore]
	public int ApproachRadius
	{
		get
		{
			return m_ApproachRadius ?? DefaultApproachRadius;
		}
		set
		{
			m_ApproachRadius = value;
		}
	}

	[MemoryPackIgnore]
	public bool NeedLoS
	{
		get
		{
			return m_NeedLoS ?? DefaultNeedLoS;
		}
		set
		{
			m_NeedLoS = value;
		}
	}

	[CanBeNull]
	[MemoryPackIgnore]
	public ForcedPath ForcedPath
	{
		get
		{
			return m_ForcedPath;
		}
		set
		{
			if (value != null && value.error)
			{
				throw new Exception("[ForcedPath] An attempt to set a failed path");
			}
			m_ForcedPath?.Release(this);
			m_ForcedPath = value;
			m_ForcedPath?.Claim(this);
		}
	}

	[MemoryPackIgnore]
	public WalkSpeedType MovementType
	{
		get
		{
			return m_MovementType ?? DefaultMovementType;
		}
		set
		{
			m_MovementType = value;
		}
	}

	[MemoryPackIgnore]
	public bool IsOneFrameCommand
	{
		get
		{
			return m_IsOneFrameCommand ?? DefaultIsOneFrameCommand;
		}
		set
		{
			m_IsOneFrameCommand = value;
		}
	}

	[MemoryPackIgnore]
	public bool SlowMotionRequired
	{
		get
		{
			return m_SlowMotionRequired ?? DefaultSlowMotionRequired;
		}
		set
		{
			m_SlowMotionRequired = value;
		}
	}

	[MemoryPackIgnore]
	protected virtual bool DefaultFreeAction => false;

	[MemoryPackIgnore]
	public virtual int DefaultApproachRadius => 1;

	[MemoryPackIgnore]
	protected virtual bool DefaultNeedLoS => false;

	[MemoryPackIgnore]
	protected virtual WalkSpeedType DefaultMovementType => WalkSpeedType.Run;

	[MemoryPackIgnore]
	protected virtual bool DefaultIsOneFrameCommand => false;

	[MemoryPackIgnore]
	protected virtual bool DefaultSlowMotionRequired => false;

	[MemoryPackIgnore]
	public virtual bool IsDirectionCorrect => true;

	[MemoryPackConstructor]
	protected UnitCommandParams()
	{
	}

	[JsonConstructor]
	protected UnitCommandParams(JsonConstructorMark _)
	{
	}

	protected UnitCommandParams([CanBeNull] TargetWrapper target)
	{
		Target = target;
		FromCutscene = ContextData<NamedParametersContext.ContextData>.Current != null;
	}

	public bool IsOffensiveCommand([NotNull] AbstractUnitEntity executor)
	{
		if (Target?.Entity is BaseUnitEntity baseUnitEntity && this is UnitUseAbilityParams && baseUnitEntity != executor && !baseUnitEntity.LifeState.IsFinallyDead)
		{
			return executor.GetCombatGroupOptional()?.CanAttack(baseUnitEntity) ?? false;
		}
		return false;
	}

	public virtual bool IsUnitEnoughClose([NotNull] AbstractUnitEntity executor)
	{
		Vector3 targetPoint = GetTargetPoint(executor);
		if (!executor.InRangeInCells(targetPoint, ApproachRadius))
		{
			return false;
		}
		if (!NeedLoS)
		{
			return true;
		}
		return (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(executor, targetPoint, default(IntRect)) != LosCalculations.CoverType.Invisible;
	}

	public virtual Vector3 GetTargetPoint([NotNull] AbstractUnitEntity executor)
	{
		return Target?.Point ?? executor.Position;
	}

	public void AfterDeserialization()
	{
		IsSynchronized = true;
		m_ForcedPath?.Claim(this);
	}

	public virtual bool TryMergeInto(AbstractUnitCommand currentCommand)
	{
		return false;
	}

	protected abstract AbstractUnitCommand CreateCommandInternal();

	public AbstractUnitCommand CreateCommand()
	{
		return CreateCommandInternal();
	}
}
public abstract class UnitCommandParams<T> : UnitCommandParams where T : AbstractUnitCommand
{
	protected UnitCommandParams()
	{
	}

	[JsonConstructor]
	protected UnitCommandParams(JsonConstructorMark _)
		: base(_)
	{
	}

	protected UnitCommandParams([CanBeNull] TargetWrapper target)
		: base(target)
	{
	}

	protected override AbstractUnitCommand CreateCommandInternal()
	{
		return (AbstractUnitCommand)Activator.CreateInstance(typeof(T), this);
	}

	public new T CreateCommand()
	{
		return (T)CreateCommandInternal();
	}
}
