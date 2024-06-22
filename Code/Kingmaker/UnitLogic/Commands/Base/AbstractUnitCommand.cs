using System;
using System.Text;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.Settings;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands.Base;

public abstract class AbstractUnitCommand
{
	public enum ResultType
	{
		None,
		Fail,
		Interrupt,
		Success
	}

	[NotNull]
	private readonly UnitCommandParams m_Params;

	private bool m_StoredDoNotZeroOtherAnimations;

	private bool m_Accelerated;

	private int m_ActEventsCounter;

	private bool m_SlowMotionRequired;

	[CanBeNull]
	private AbstractUnitEntity m_Executor;

	private bool m_IsAnimationActHandled;

	private float m_StoredWeightMultiplier;

	public TimeSpan StartTime;

	public ResultType Result { get; private set; }

	public bool IsStarted { get; private set; }

	public bool IsFinished { get; private set; }

	protected bool DidRun { get; private set; }

	[CanBeNull]
	public UnitAnimationActionHandle Animation { get; protected set; }

	public float TimeSinceStart { get; private set; }

	protected float PretendActTime { get; private set; }

	protected bool HasAnimation { get; set; }

	public abstract bool IsMoveUnit { get; }

	[NotNull]
	public UnitCommandParams Params => m_Params;

	[NotNull]
	public AbstractUnitEntity Executor => m_Executor ?? throw new NullReferenceException("Executor is missing");

	[CanBeNull]
	public TargetWrapper Target => Params.Target;

	[CanBeNull]
	public AbstractUnitEntity TargetUnit => Target?.Entity as AbstractUnitEntity;

	public int ApproachRadius => Params.ApproachRadius;

	public bool IsFreeAction => Params.FreeAction;

	public bool FromCutscene => Params.FromCutscene;

	public bool NeedLoS => Params.NeedLoS;

	public WalkSpeedType MovementType => Params.MovementType;

	public float? OverrideSpeed => Params.OverrideSpeed;

	public float? SpeedLimit => Params.SpeedLimit;

	public bool IsOneFrameCommand => Params.IsOneFrameCommand;

	public bool SlowMotionRequired => Params.SlowMotionRequired;

	public bool AiCanInterruptMark => Params.AiCanInterruptMark;

	public bool DoNotInterruptAfterFight => Params.DoNotInterruptAfterFight;

	public bool IsRunning
	{
		get
		{
			if (IsStarted)
			{
				return !IsFinished;
			}
			return false;
		}
	}

	public bool IsActed => m_ActEventsCounter > 0;

	public bool IsPreventMovement
	{
		get
		{
			if (Animation != null && m_IsAnimationActHandled)
			{
				return !Animation.DoesNotPreventMovement;
			}
			return false;
		}
	}

	public virtual bool IsInterruptible
	{
		get
		{
			if (IsStarted && IsActed && Animation != null)
			{
				return Animation.IsFinished;
			}
			return true;
		}
	}

	public Vector3 ApproachPoint => GetTargetPoint();

	public virtual bool AwaitMovementFinish => false;

	public virtual bool ShouldBeInterrupted => false;

	public virtual bool IsUnitEnoughClose
	{
		get
		{
			if (!Executor.InRangeInCells(ApproachPoint, ApproachRadius))
			{
				return false;
			}
			if (!NeedLoS)
			{
				return true;
			}
			return (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(Executor, ApproachPoint, default(IntRect)) != LosCalculations.CoverType.Invisible;
		}
	}

	public virtual bool ShouldTurnToTarget => true;

	public bool ShouldUnitApproach
	{
		get
		{
			if (!IsMoveUnit || !AwaitMovementFinish || !Executor.View.IsMoving())
			{
				return !IsUnitEnoughClose;
			}
			return true;
		}
	}

	public virtual bool MarkManualTarget => false;

	public virtual bool NeedEquipWeapons => false;

	public virtual bool CanStart => true;

	public virtual bool DontWaitForHands => false;

	public bool IsAccelerated => m_Accelerated;

	public ForcedPath ForcedPath => Params.ForcedPath;

	protected bool IsPretendAct
	{
		get
		{
			if (Animation == null)
			{
				return PretendActTime > 0f;
			}
			return false;
		}
	}

	protected virtual int ExpectedActEventsCount => 1;

	public virtual bool IsWaitingForAnimation => false;

	protected AbstractUnitCommand([NotNull] UnitCommandParams @params)
	{
		m_Params = @params;
	}

	public virtual void Init(AbstractUnitEntity executor)
	{
		m_Executor = executor;
	}

	public virtual void Clear()
	{
		m_Executor = null;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(128);
		stringBuilder.Append(GetType().Name);
		string innerDataDescription = GetInnerDataDescription();
		stringBuilder.Append("[");
		if (!string.IsNullOrEmpty(innerDataDescription))
		{
			stringBuilder.Append("data=");
			stringBuilder.Append("(");
			stringBuilder.Append(innerDataDescription);
			stringBuilder.Append("), ");
		}
		stringBuilder.Append("executor=");
		stringBuilder.Append(Executor?.CharacterName ?? "NULL");
		stringBuilder.Append(", target=");
		stringBuilder.Append(Target?.Entity?.Name ?? Target?.Point.ToString() ?? "None");
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	[CanBeNull]
	protected virtual string GetInnerDataDescription()
	{
		return null;
	}

	protected void ScheduleAct(float delay = 1f)
	{
		PretendActTime = TimeSinceStart + delay / Game.CombatAnimSpeedUp;
	}

	protected void StartAnimation(UnitAnimationType animationType, [CanBeNull] Action<UnitAnimationActionHandle> initializer = null)
	{
		AbstractUnitEntityView abstractUnitEntityView = ObjectExtensions.Or(Executor.View, null);
		Animation = (((object)abstractUnitEntityView == null) ? null : ObjectExtensions.Or(abstractUnitEntityView.AnimationManager, null)?.CreateHandle(animationType));
		HasAnimation = true;
		if (Animation == null)
		{
			ScheduleAct((this is UnitAttackOfOpportunity) ? SlowMoController.SlowMoFactor : 1f);
			return;
		}
		initializer?.Invoke(Animation);
		StartAnimation(Animation);
	}

	protected virtual void StartAnimation([NotNull] UnitAnimationActionHandle handle)
	{
		Animation = handle;
		HasAnimation = true;
		Executor.View.AnimationManager.Execute(Animation);
		m_IsAnimationActHandled = false;
	}

	private void ClearAnimation()
	{
		Animation = null;
		m_IsAnimationActHandled = false;
	}

	public void SuppressAnimation()
	{
		AnimationBase animationBase = Animation?.ActiveAnimation;
		m_StoredWeightMultiplier = animationBase?.GetWeight() ?? 1f;
		m_StoredDoNotZeroOtherAnimations = animationBase?.DoNotZeroOtherAnimations ?? false;
		if (animationBase != null)
		{
			animationBase.SetWeightMultiplier(0f);
			animationBase.DoNotZeroOtherAnimations = true;
		}
	}

	public void RestoreAnimation()
	{
		AnimationBase animationBase = Animation?.ActiveAnimation;
		if (animationBase != null)
		{
			animationBase.SetWeightMultiplier(m_StoredWeightMultiplier);
			animationBase.DoNotZeroOtherAnimations = m_StoredDoNotZeroOtherAnimations;
		}
	}

	public void Start()
	{
		if (IsStarted)
		{
			PFLog.Default.Error("Command {0} is already started", this);
			Interrupt();
			return;
		}
		if (Target?.Entity != null && !Target.Entity.IsInState)
		{
			PFLog.Default.Error("Target {0} is not in state", Target.Entity);
			Interrupt();
			return;
		}
		if (!IsUnitEnoughClose)
		{
			PFLog.Default.Error("Unit is not enough close for start action");
			Interrupt();
			return;
		}
		StartTime = Game.Instance.TimeController.RealTime;
		IsStarted = true;
		OnStart();
		EventBus.RaiseEvent((IMechanicEntity)Executor, (Action<IUnitCommandStartHandler>)delegate(IUnitCommandStartHandler h)
		{
			h.HandleUnitCommandDidStart(this);
		}, isCheckRuntime: true);
	}

	public virtual void Tick()
	{
		if (!IsRunning)
		{
			PFLog.Default.Error("Command {0} is not running", this);
			Interrupt();
			return;
		}
		if (Target?.Entity != null && !Target.Entity.IsInState)
		{
			PFLog.Default.Error("Target {0} is not in state", Target.Entity);
			Interrupt();
			return;
		}
		TimeController timeController = Game.Instance.TimeController;
		TimeSinceStart += timeController.DeltaTime;
		using (ProfileScope.New("OnTick"))
		{
			OnTick();
		}
		if (IsFinished)
		{
			return;
		}
		bool flag = Animation != null && Animation.ActEventsCounter > m_ActEventsCounter;
		bool flag2 = Animation?.IsFinished ?? false;
		int num = (flag2 ? (flag ? Math.Max(0, ExpectedActEventsCount - m_ActEventsCounter - 1) : Math.Max(0, ExpectedActEventsCount - m_ActEventsCounter)) : 0);
		if (!m_IsAnimationActHandled || flag || num > 0)
		{
			bool flag3 = flag || IsOneFrameCommand;
			if (flag2 && !flag3)
			{
				UnitAnimationActionHandle animation = Animation;
				if (animation != null && !animation.IsSkipped)
				{
					PFLog.Default.Error(Animation.Action, $"{Animation.Action.NameSafe()} send not enough Act events: expected {ExpectedActEventsCount}, received {m_ActEventsCounter}");
				}
				flag3 = true;
			}
			int num2 = Animation?.ActEventsCounter ?? 1;
			if (flag2 && num > 0)
			{
				UnitAnimationActionHandle animation = Animation;
				if (animation != null && !animation.IsSkipped)
				{
					PFLog.Default.Error(Animation.Action, $"{Animation.Action.NameSafe()}: {num2} Act events sent, {num} missing");
				}
			}
			if (flag3 | ((IsPretendAct && TimeSinceStart >= PretendActTime) || !HasAnimation || num > 0))
			{
				m_IsAnimationActHandled = true;
				int num3 = (flag2 ? (ExpectedActEventsCount - m_ActEventsCounter) : Math.Max(1, num2 - m_ActEventsCounter));
				for (int i = 0; i < num3; i++)
				{
					using (ProfileScope.New("OnAction"))
					{
						Result = OnAction();
					}
					m_ActEventsCounter++;
					using (ProfileScope.New("HandleUnitCommandDidAct"))
					{
						EventBus.RaiseEvent((IMechanicEntity)Executor, (Action<IUnitCommandActHandler>)delegate(IUnitCommandActHandler h)
						{
							h.HandleUnitCommandDidAct(this);
						}, isCheckRuntime: true);
					}
					if (Result != 0)
					{
						break;
					}
				}
				if (flag2)
				{
					UnitAnimationActionHandle animation = Animation;
					if (animation != null && !animation.IsSkipped && Result == ResultType.None)
					{
						PFLog.Default.ErrorWithReport("UnitCommand's animation is finished but Result is None");
						Result = ResultType.Fail;
					}
				}
			}
		}
		if (Result == ResultType.None || (Animation != null && !Animation.IsFinished))
		{
			return;
		}
		using (ProfileScope.New("OnEnded"))
		{
			OnEnded();
		}
	}

	protected virtual Vector3 GetTargetPoint()
	{
		return Target?.Point ?? Executor.Position;
	}

	protected void ForceFinish(ResultType result)
	{
		if (IsFinished)
		{
			PFLog.Default.Error($"{this} is already finished, return");
			return;
		}
		if (result == ResultType.None)
		{
			PFLog.Default.Error("Invalid result type");
			return;
		}
		Result = result;
		m_ActEventsCounter = 1;
		OnEnded();
	}

	public void ForceFinishForTurnBased(ResultType result)
	{
		ForceFinish(result);
	}

	public virtual void ConvertToOneFrame()
	{
		Params.IsOneFrameCommand = true;
		ClearAnimation();
	}

	public void Interrupt()
	{
		if (Result != ResultType.Interrupt && !IsFinished)
		{
			Result = ResultType.Interrupt;
			OnEnded();
		}
	}

	protected virtual void OnTick()
	{
	}

	protected virtual void OnStart()
	{
		if (!IsOneFrameCommand)
		{
			TriggerAnimation();
		}
	}

	protected virtual void TriggerAnimation()
	{
	}

	protected abstract ResultType OnAction();

	protected virtual void OnEnded()
	{
		IsFinished = true;
		if (Animation != null && !Animation.IsReleased)
		{
			if (Game.Instance.IsPaused)
			{
				Animation.Release(0f);
			}
			else
			{
				Animation.Release();
			}
		}
		if (NeedEquipWeapons && DidRun && Executor.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandleAttackCommandEnd();
		}
		EventBus.RaiseEvent((IMechanicEntity)Executor, (Action<IUnitCommandEndHandler>)delegate(IUnitCommandEndHandler h)
		{
			h.HandleUnitCommandDidEnd(this);
		}, isCheckRuntime: true);
		Params.ForcedPath = null;
	}

	public virtual void OnRun()
	{
		if (Game.Instance.IsPaused && Target != null && Target.Entity != Executor && ShouldTurnToTarget)
		{
			Executor.ForceLookAt(Target.Point);
		}
		DidRun = true;
		if (NeedEquipWeapons && Executor.View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandleAttackCommandRun();
		}
	}

	public void AccelerateIfPathIsLong(ForcedPath path)
	{
		if (CanAccelerate())
		{
			path.GetTotalLength();
			_ = Game.Instance.BlueprintRoot.SystemMechanics.MinMetersToRun;
		}
	}

	public void Accelerate()
	{
		if (CanAccelerate())
		{
			Params.MovementType = WalkSpeedType.Run;
			if (SpeedLimit.HasValue)
			{
				Params.SpeedLimit *= 1.8f;
			}
			m_Accelerated = true;
		}
	}

	public void Decelerate()
	{
		if (m_Accelerated)
		{
			Params.MovementType = WalkSpeedType.Walk;
			if (SpeedLimit.HasValue)
			{
				Params.SpeedLimit /= 1.8f;
			}
			m_Accelerated = false;
		}
	}

	private bool CanAccelerate()
	{
		if (!m_Accelerated && (bool)SettingsRoot.Game.Main.AcceleratedMove && MovementType == WalkSpeedType.Walk)
		{
			return !Executor.IsInCombat;
		}
		return false;
	}

	public virtual void TurnToTarget()
	{
		Executor.LookAt(ApproachPoint);
	}

	public static bool CommandTargetUntargetable(Entity sourceEntity, MechanicEntity targetEntity, RulebookEvent evt = null)
	{
		if (sourceEntity == targetEntity)
		{
			return false;
		}
		if ((evt?.Reason.Fact)?.Owner == targetEntity)
		{
			return false;
		}
		BaseUnitEntity obj = sourceEntity as BaseUnitEntity;
		BaseUnitEntity baseUnitEntity = targetEntity as BaseUnitEntity;
		if ((bool)obj?.CutsceneControlledUnit?.GetCurrentlyActive() && (bool)baseUnitEntity?.CutsceneControlledUnit?.GetCurrentlyActive())
		{
			return false;
		}
		return targetEntity.Features.IsUntargetable;
	}

	public bool RequireSlowMotion()
	{
		return m_SlowMotionRequired = true;
	}

	public void PostLoad(AbstractUnitEntity executor)
	{
		try
		{
			m_Executor = executor;
			OnPostLoad();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}

	protected virtual void OnPostLoad()
	{
	}
}
public abstract class AbstractUnitCommand<TParams> : AbstractUnitCommand where TParams : UnitCommandParams
{
	public new TParams Params => (TParams)base.Params;

	protected AbstractUnitCommand([NotNull] TParams @params)
		: base(@params)
	{
	}
}
