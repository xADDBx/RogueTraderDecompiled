using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.CustomIdleComponents;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation.Kingmaker;

public class UnitAnimationManager : AnimationManager, IEntitySubscriber, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>
{
	private enum ExclusiveStateType
	{
		None,
		Prone,
		StandUp,
		Stun,
		Dead,
		ExitingStun,
		Cover,
		StandUpInCover,
		CoverStepOut,
		TraverseNodeLink,
		ForceMove,
		JumpAsideDodge
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("UnitAnimationManager");

	private UnitAnimationActionHandle m_LocoMotionHandle;

	private UnitAnimationActionHandle m_CurrentMainHandAttackForPrepare;

	private ExclusiveStateType m_ExclusiveState;

	private UnitAnimationActionHandle m_ExclusiveHandle;

	private UnitAnimationActionHandle m_MicroIdleHandle;

	private UnitAnimationActionMicroIdle m_MicroIdle;

	private UnitAnimationActionVariantIdle m_VariantIdle;

	private TimedProbabilityCurve.Tracker m_RandomIdleTracker;

	private UnitAnimationActionHandle m_WhileIdleHandle;

	private UnitAnimationDecoratorManager m_DecoratorManager;

	private bool m_IsAnimating = true;

	private bool m_InCutsceneCoverAvailable;

	private bool m_BlockAttackAnimation;

	private Playable m_OldSource;

	private LosCalculations.CoverType m_CoverType;

	private static readonly LogChannel Debug = LogChannelFactory.GetOrCreate("ExclusiveState");

	public UnitAnimationActionHandle BuffLoopAction { get; set; }

	public AbstractUnitEntityView View { get; private set; }

	public BlueprintRace AnimationRace { get; private set; }

	public bool IsStopping { get; set; }

	public bool IsInCombat { get; set; }

	public WeaponAnimationStyle ActiveMainHandWeaponStyle { get; set; }

	public WeaponAnimationStyle ActiveOffHandWeaponStyle { get; set; }

	public float Speed { get; set; }

	public float NewSpeed { get; set; }

	public float Orientation { get; set; }

	public List<AnimationClipWrapper> CustomIdleWrappers { get; set; }

	public WalkSpeedType WalkSpeedType { get; set; }

	public bool IsDead { get; set; }

	public bool IsProne { get; set; }

	public bool IsSleeping { get; set; }

	public bool IsUnconscious { get; set; }

	public bool IsStunned { get; set; }

	public bool IsTraverseLink { get; set; }

	public bool PreviousInCombat { get; set; }

	public float ForceMoveDistance { get; set; }

	public float[] ForceMoveTime { get; set; }

	public float DodgeDistance { get; set; }

	public bool IsMechadendrite { get; set; }

	public bool BlockAttackAnimation
	{
		get
		{
			if (NeedStepOut)
			{
				return m_BlockAttackAnimation;
			}
			return false;
		}
		set
		{
			m_BlockAttackAnimation = value;
		}
	}

	public LosCalculations.CoverType CoverType
	{
		get
		{
			if (StepOutDirectionAnimationType != 0)
			{
				return LosCalculations.CoverType.Full;
			}
			return m_CoverType;
		}
		set
		{
			m_CoverType = value;
		}
	}

	public UnitAnimationActionCover.StepOutDirectionAnimationType StepOutDirectionAnimationType { get; set; }

	public bool AbilityIsSpell { get; set; }

	public float UseAbilityDirection { get; set; }

	public bool InCutsceneCoverAvailable
	{
		get
		{
			if (Game.Instance.CurrentMode == GameModeType.Cutscene || (View is UnitEntityView { Data: not null } unitEntityView && unitEntityView.Data.Body.InCombatVisual && !unitEntityView.Data.IsInCombat))
			{
				return m_InCutsceneCoverAvailable;
			}
			m_InCutsceneCoverAvailable = false;
			return true;
		}
		set
		{
			m_InCutsceneCoverAvailable = value;
		}
	}

	public bool IsWaitingForIncomingAttackOfOpportunity { get; set; }

	public bool InCover
	{
		get
		{
			if (IsInCombat && InCutsceneCoverAvailable && View is UnitEntityView unitEntityView && (unitEntityView.Data == null || unitEntityView.Data != Game.Instance.TurnController.CurrentUnit || unitEntityView.Data.IsInPlayerParty))
			{
				PartUnitCommands partUnitCommands = unitEntityView.Data?.Commands;
				if (partUnitCommands != null && !partUnitCommands.Empty)
				{
					AbstractUnitCommand abstractUnitCommand = unitEntityView.Data?.Commands.Current;
					if (abstractUnitCommand == null || !abstractUnitCommand.IsOneFrameCommand)
					{
						goto IL_00f4;
					}
				}
				if (!unitEntityView.AgentASP.IsCharging)
				{
					UnitViewHandsEquipment handsEquipment = unitEntityView.HandsEquipment;
					if ((handsEquipment == null || !handsEquipment.AreHandsBusyWithAnimation.Value) && CoverType != 0)
					{
						return base.ActiveActions.SingleOrDefault((AnimationActionHandle x) => x.Action as WarhammerUnitAnimationActionParry) == null;
					}
				}
			}
			goto IL_00f4;
			IL_00f4:
			return false;
		}
	}

	public bool NeedStepOut
	{
		get
		{
			if (IsInCombat && View is UnitEntityView unitEntityView && (unitEntityView.Data != Game.Instance.TurnController.CurrentUnit || unitEntityView.Data == null || unitEntityView.Data.IsInPlayerParty) && InCutsceneCoverAvailable && View != null && View.Data?.Commands.Current is UnitUseAbility unitUseAbility && unitUseAbility.Target != unitUseAbility.Executor)
			{
				return CoverType == LosCalculations.CoverType.Full;
			}
			return false;
		}
	}

	public UnitAnimationDecoratorManager DecoratorManager
	{
		get
		{
			if (m_DecoratorManager == null)
			{
				m_DecoratorManager = new UnitAnimationDecoratorManager(this);
			}
			return m_DecoratorManager;
		}
	}

	public bool IsAnimating
	{
		get
		{
			return m_IsAnimating;
		}
		set
		{
			if (m_IsAnimating == value)
			{
				return;
			}
			m_IsAnimating = value;
			base.Disabled = !m_IsAnimating;
			if (!m_IsAnimating)
			{
				AnimationBase animationBase = base.ActiveAnimations.MaxBy((AnimationBase a) => a.GetWeight());
				AnimationClip clip = animationBase.GetPlayableClip();
				float time = animationBase.GetTime();
				if (!clip)
				{
					AnimatorControllerPlayable animatorControllerPlayable = (AnimatorControllerPlayable)((PlayableInfo)animationBase).Playable;
					if (animatorControllerPlayable.IsValid())
					{
						GetCurrentClipFromPlayableController(animatorControllerPlayable, out clip, out time);
					}
				}
				if (!clip)
				{
					Logger.Warning(this, "Paralyze: cannot find current played clip for {0}", base.name);
					return;
				}
				SuspendEvents();
				m_OldSource = base.Output.GetSourcePlayable();
				AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(base.PlayableGraph, clip);
				base.Output.SetSourcePlayable(animationClipPlayable);
				animationClipPlayable.SetTime(time);
				animationClipPlayable.SetSpeed(0.0);
			}
			else
			{
				base.Output.SetSourcePlayable(m_OldSource);
				ResumeEvents();
			}
		}
	}

	public bool HasActedAnimation => base.ActiveActions.Any((AnimationActionHandle a) => a.IsActed);

	public bool IsGoingProne
	{
		get
		{
			if (m_ExclusiveHandle?.Action is UnitAnimationActionProne unitAnimationActionProne)
			{
				return !unitAnimationActionProne.IsActuallyProne(m_ExclusiveHandle);
			}
			return false;
		}
	}

	public UnitAnimationActionHandle ExclusiveHandle => m_ExclusiveHandle;

	public UnitAnimationActionHandle CurrentMainHandAttackForPrepare => m_CurrentMainHandAttackForPrepare;

	public bool IsGoingCover
	{
		get
		{
			if (m_ExclusiveHandle?.Action is UnitAnimationActionCover unitAnimationActionCover && !m_ExclusiveHandle.IsReleased)
			{
				return !unitAnimationActionCover.IsCoverForceExitingFinished(m_ExclusiveHandle);
			}
			return false;
		}
	}

	public bool IsPreventingMovement => m_ExclusiveState switch
	{
		ExclusiveStateType.None => false, 
		ExclusiveStateType.Stun => false, 
		ExclusiveStateType.StandUpInCover => false, 
		ExclusiveStateType.CoverStepOut => false, 
		ExclusiveStateType.TraverseNodeLink => false, 
		ExclusiveStateType.JumpAsideDodge => false, 
		ExclusiveStateType.ForceMove => false, 
		ExclusiveStateType.Prone => false, 
		_ => true, 
	};

	public bool IsPreventingRotation
	{
		get
		{
			if (!IsProne && !IsStandUp && !base.IsRotationForbidden)
			{
				return IsGoingProne;
			}
			return true;
		}
	}

	public bool IsInExclusiveState => m_ExclusiveState switch
	{
		ExclusiveStateType.None => false, 
		ExclusiveStateType.Cover => false, 
		ExclusiveStateType.StandUpInCover => false, 
		ExclusiveStateType.CoverStepOut => false, 
		ExclusiveStateType.TraverseNodeLink => false, 
		ExclusiveStateType.JumpAsideDodge => false, 
		ExclusiveStateType.ForceMove => false, 
		_ => true, 
	};

	public CombatMicroIdle CombatMicroIdle { get; set; }

	public UnitAnimationActionHandle CurrentEquipHandle { get; set; }

	public bool IsStandUp => m_ExclusiveState == ExclusiveStateType.StandUp;

	public UnitAnimationActionHandle LocoMotionHandle => m_LocoMotionHandle;

	private void GetCurrentClipFromPlayableController(AnimatorControllerPlayable controller, out AnimationClip clip, out float time)
	{
		float weight = 0f;
		clip = null;
		GetCurrentClipFromPlayableRecursive(controller, 1f, ref clip, out time, ref weight);
	}

	private void GetCurrentClipFromPlayableRecursive(Playable p, float inWeight, ref AnimationClip clip, out float time, ref float weight)
	{
		time = 0f;
		for (int i = 0; i < p.GetInputCount(); i++)
		{
			Playable input = p.GetInput(i);
			float inputWeight = p.GetInputWeight(i);
			if (input.IsPlayableOfType<AnimationClipPlayable>())
			{
				if (inputWeight * inWeight > weight)
				{
					weight = inputWeight;
					clip = ((AnimationClipPlayable)input).GetAnimationClip();
					time = (float)input.GetTime();
				}
			}
			else if (inputWeight > 0f)
			{
				GetCurrentClipFromPlayableRecursive(input, inWeight * inputWeight, ref clip, out time, ref weight);
			}
		}
	}

	public void AtachToView(AbstractUnitEntityView view, BlueprintRace animationRace)
	{
		View = view;
		AnimationRace = animationRace;
		if (View?.Blueprint.GetComponent<CustomIdleAnimationBlueprintComponent>() != null)
		{
			AbstractUnitEntityView view2 = View;
			if ((object)view2 != null && view2.Blueprint.GetComponent<CustomIdleAnimationBlueprintComponent>()?.IdleClips.Count > 0)
			{
				CustomIdleWrappers = View.Blueprint.GetComponent<CustomIdleAnimationBlueprintComponent>()?.IdleClips;
			}
		}
		m_MicroIdle = (UnitAnimationActionMicroIdle)GetAction(UnitAnimationType.MicroIdle);
		m_VariantIdle = (UnitAnimationActionVariantIdle)GetAction(UnitAnimationType.VariantIdle);
		m_RandomIdleTracker = ((m_VariantIdle != null) ? m_VariantIdle.RetriggerProbability.Track(base.StatefulRandom) : null);
		PreviousInCombat = IsInCombat;
	}

	public void ChangeLocoMotion(WarhammerUnitAnimationActionLocoMotion action)
	{
		m_LocoMotionHandle = (UnitAnimationActionHandle)CreateHandle(action);
		if (m_LocoMotionHandle != null)
		{
			Execute(m_LocoMotionHandle);
			m_LocoMotionHandle.ActiveAnimation.UpdateInternal(1f, 1f);
		}
	}

	internal void ResetLocoMotion()
	{
		if (m_LocoMotionHandle != null)
		{
			m_LocoMotionHandle.Release();
			RemoveActionHandle(m_LocoMotionHandle);
			m_LocoMotionHandle = null;
		}
	}

	private void TryInitLocoMotion()
	{
		if (m_LocoMotionHandle == null)
		{
			m_LocoMotionHandle = ((IsInDollRoom && GetAction(UnitAnimationType.DollRoomLocoMotion) != null) ? CreateHandle(UnitAnimationType.DollRoomLocoMotion) : CreateHandle(UnitAnimationType.LocoMotion));
			if (m_LocoMotionHandle != null)
			{
				Execute(m_LocoMotionHandle);
				m_LocoMotionHandle.ActiveAnimation?.UpdateInternal(1f, 1f);
			}
		}
	}

	public void Tick(float deltaTime)
	{
		using (ProfileScope.New("UnitAnimationManager.Tick()"))
		{
			try
			{
				TickInternal(deltaTime);
			}
			catch (Exception ex)
			{
				Logger.Exception(this, ex);
			}
		}
	}

	private void TickInternal(float deltaTime)
	{
		if (base.AnimationSet == null)
		{
			return;
		}
		TryInitLocoMotion();
		if (m_WhileIdleHandle != null && NewSpeed > 0f)
		{
			m_WhileIdleHandle.Release();
			m_WhileIdleHandle = null;
		}
		if (!IsProne && CanRunIdleAction())
		{
			TickIdleVariants(deltaTime);
		}
		if (IsDead && !IsMechadendrite)
		{
			SetExclusiveAnimation(ExclusiveStateType.Dead);
			return;
		}
		if (IsProne && !IsMechadendrite)
		{
			SetExclusiveAnimation(ExclusiveStateType.Prone);
			return;
		}
		if (ForceMoveDistance > 0f)
		{
			SetExclusiveAnimation(ExclusiveStateType.ForceMove);
			return;
		}
		if (DodgeDistance > 0f)
		{
			SetExclusiveAnimation(ExclusiveStateType.JumpAsideDodge);
			return;
		}
		if (IsStunned)
		{
			SetExclusiveAnimation(ExclusiveStateType.Stun);
			return;
		}
		if (m_ExclusiveState == ExclusiveStateType.Prone || m_ExclusiveState == ExclusiveStateType.Dead)
		{
			SetExclusiveAnimation(ExclusiveStateType.StandUp);
			return;
		}
		if (m_ExclusiveState == ExclusiveStateType.Stun)
		{
			SetExclusiveAnimation(ExclusiveStateType.ExitingStun);
			return;
		}
		if ((m_ExclusiveHandle == null || m_ExclusiveHandle.IsReleased) && NeedStepOut && !HasMovingCommand(View.Data) && StepOutDirectionAnimationType != 0)
		{
			SetExclusiveAnimation(ExclusiveStateType.CoverStepOut);
			return;
		}
		if (m_ExclusiveState == ExclusiveStateType.Cover && HasMovingCommand(View.Data))
		{
			SetExclusiveAnimation(ExclusiveStateType.StandUpInCover);
			return;
		}
		if (InCover && View is UnitEntityView unitEntityView && (unitEntityView.HandsEquipment == null || !unitEntityView.HandsEquipment.AreHandsBusyWithAnimation) && (m_ExclusiveState != ExclusiveStateType.CoverStepOut || m_ExclusiveHandle.IsReleased) && !HasMovingCommand(View.Data) && !View.MovementAgent.IsReallyMoving)
		{
			SetExclusiveAnimation(ExclusiveStateType.Cover);
			return;
		}
		if (IsTraverseLink)
		{
			SetExclusiveAnimation(ExclusiveStateType.TraverseNodeLink);
			return;
		}
		UnitAnimationActionHandle exclusiveHandle = m_ExclusiveHandle;
		if (exclusiveHandle == null || exclusiveHandle.IsReleased)
		{
			SetExclusiveAnimation(ExclusiveStateType.None);
		}
	}

	public static bool HasMovingCommand([CanBeNull] PartUnitCommands.IOwner unit)
	{
		return unit?.Commands.Contains((AbstractUnitCommand x) => x.IsMoveUnit) ?? false;
	}

	private void TickIdleVariants(float deltaTime)
	{
		if (IsInCombat && (bool)m_MicroIdle)
		{
			switch (CombatMicroIdle)
			{
			case CombatMicroIdle.None:
				m_MicroIdleHandle?.Release();
				m_MicroIdleHandle = null;
				break;
			case CombatMicroIdle.Weapon:
				if (m_RandomIdleTracker == null)
				{
					m_RandomIdleTracker = m_MicroIdle.TriggerProbability.Track(base.StatefulRandom);
				}
				if (m_MicroIdleHandle.NullOrFinished() && m_RandomIdleTracker.Tick(deltaTime))
				{
					m_MicroIdleHandle = ((IsInDollRoom && GetAction(UnitAnimationType.DollRoomMicroIdle) != null) ? ExecuteIfIdle(UnitAnimationType.DollRoomMicroIdle) : ExecuteIfIdle(UnitAnimationType.MicroIdle));
					m_RandomIdleTracker = m_MicroIdle.TriggerProbability.Track(base.StatefulRandom);
					if (m_MicroIdleHandle != null)
					{
						m_MicroIdleHandle.SpeedScale = base.StatefulRandom.Range(0.9f, 1.2f);
					}
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		if (IsInCombat || !(Game.Instance.CurrentMode == GameModeType.Default) || !m_VariantIdle || !View)
		{
			return;
		}
		if (CustomIdleWrappers == null)
		{
			AbstractUnitEntity data = View.Data;
			if ((data == null || !data.IsPlayerFaction) && !m_VariantIdle.PlayOnNPC)
			{
				return;
			}
		}
		if (m_RandomIdleTracker == null)
		{
			m_RandomIdleTracker = m_VariantIdle.RetriggerProbability.Track(base.StatefulRandom);
		}
		if (m_MicroIdleHandle.NullOrFinished() && m_RandomIdleTracker.Tick(deltaTime))
		{
			m_MicroIdleHandle = ((IsInDollRoom && GetAction(UnitAnimationType.DollRoomVariantIdle) != null) ? ExecuteIfIdle(UnitAnimationType.DollRoomVariantIdle) : ExecuteIfIdle(UnitAnimationType.VariantIdle));
			m_RandomIdleTracker = m_VariantIdle.RetriggerProbability.Track(base.StatefulRandom);
		}
	}

	private void SetExclusiveAnimation(ExclusiveStateType state)
	{
		if ((state == m_ExclusiveState && (m_ExclusiveState != ExclusiveStateType.Cover || !m_ExclusiveHandle.IsReleased || base.ActiveActions.Contains((AnimationActionHandle x) => x is UnitAnimationActionHandle unitAnimationActionHandle && unitAnimationActionHandle.Action.Type == UnitAnimationType.Hit))) || (m_ExclusiveState == ExclusiveStateType.StandUp && !m_ExclusiveHandle.IsReleased))
		{
			return;
		}
		if (state != ExclusiveStateType.StandUpInCover && state != ExclusiveStateType.StandUp)
		{
			m_ExclusiveHandle?.Release();
		}
		switch (state)
		{
		case ExclusiveStateType.Prone:
			m_ExclusiveHandle = Execute(UnitAnimationType.Prone);
			break;
		case ExclusiveStateType.Cover:
		{
			UnitAnimationActionCover unitAnimationActionCover3 = (UnitAnimationActionCover)GetAction(UnitAnimationType.Cover);
			if (unitAnimationActionCover3 == null)
			{
				return;
			}
			m_ExclusiveHandle = (UnitAnimationActionHandle)CreateHandle(unitAnimationActionCover3);
			m_ExclusiveHandle.IsAdditive = false;
			unitAnimationActionCover3 = m_ExclusiveHandle?.Action as UnitAnimationActionCover;
			if (unitAnimationActionCover3 != null)
			{
				unitAnimationActionCover3.PrepareEnteringCoverData(m_ExclusiveHandle);
			}
			Execute(m_ExclusiveHandle);
			break;
		}
		case ExclusiveStateType.StandUpInCover:
		{
			UnitAnimationActionCover unitAnimationActionCover = m_ExclusiveHandle?.Action as UnitAnimationActionCover;
			if (unitAnimationActionCover != null)
			{
				unitAnimationActionCover.ForceExit(m_ExclusiveHandle);
				break;
			}
			m_ExclusiveHandle?.Release();
			state = ExclusiveStateType.None;
			break;
		}
		case ExclusiveStateType.CoverStepOut:
		{
			UnitAnimationActionCover unitAnimationActionCover2 = (UnitAnimationActionCover)GetAction(UnitAnimationType.Cover);
			if (unitAnimationActionCover2 == null)
			{
				return;
			}
			m_ExclusiveHandle = (UnitAnimationActionHandle)CreateHandle(unitAnimationActionCover2);
			m_ExclusiveHandle.IsAdditive = !(View.Data?.Commands.Current is UnitUseAbility) || base.CurrentAction == null;
			unitAnimationActionCover2 = m_ExclusiveHandle?.Action as UnitAnimationActionCover;
			if (unitAnimationActionCover2 != null)
			{
				unitAnimationActionCover2.PrepareStepOutData(m_ExclusiveHandle);
			}
			Execute(m_ExclusiveHandle);
			break;
		}
		case ExclusiveStateType.StandUp:
		{
			UnitAnimationActionProne unitAnimationActionProne = m_ExclusiveHandle?.Action as UnitAnimationActionProne;
			if ((bool)unitAnimationActionProne)
			{
				unitAnimationActionProne.SwitchToExit(m_ExclusiveHandle);
				break;
			}
			m_ExclusiveHandle?.Release();
			state = ExclusiveStateType.None;
			break;
		}
		case ExclusiveStateType.Stun:
			m_ExclusiveHandle = Execute(UnitAnimationType.Stunned);
			break;
		case ExclusiveStateType.Dead:
			m_ExclusiveHandle = CreateHandle(UnitAnimationType.Death);
			if (m_ExclusiveHandle == null)
			{
				m_ExclusiveHandle = CreateHandle(UnitAnimationType.Prone);
				if (m_ExclusiveHandle != null)
				{
					m_ExclusiveHandle.DeathFromProne = m_ExclusiveState == ExclusiveStateType.Prone;
				}
			}
			else
			{
				m_ExclusiveHandle.DeathType = UnitAnimationActionDeath.DeathType.DeathBase;
			}
			Execute(m_ExclusiveHandle);
			break;
		case ExclusiveStateType.ExitingStun:
		{
			UnitAnimationActionStun unitAnimationActionStun = m_ExclusiveHandle?.Action as UnitAnimationActionStun;
			if ((bool)unitAnimationActionStun)
			{
				unitAnimationActionStun.SwitchToExit(m_ExclusiveHandle);
				break;
			}
			m_ExclusiveHandle?.Release();
			state = ExclusiveStateType.None;
			break;
		}
		case ExclusiveStateType.TraverseNodeLink:
			m_ExclusiveHandle = Execute(UnitAnimationType.TraverseNodeLink);
			break;
		case ExclusiveStateType.ForceMove:
			m_ExclusiveHandle = Execute(UnitAnimationType.ForceMove);
			break;
		case ExclusiveStateType.JumpAsideDodge:
			m_ExclusiveHandle = Execute(UnitAnimationType.JumpAsideDodge);
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		case ExclusiveStateType.None:
			break;
		}
		string arg = ((View == null) ? "View is null" : ((View.EntityData == null) ? "View.EntityData is null" : $"{View.EntityData}"));
		Debug.Log($"{arg}: {m_ExclusiveState} -> {state}");
		m_ExclusiveState = state;
	}

	[CanBeNull]
	public UnitAnimationAction GetAction(UnitAnimationType type)
	{
		return base.AnimationSet.GetAction(type);
	}

	[CanBeNull]
	public UnitAnimationActionSpecialAttack GetAction(UnitAnimationSpecialAttackType type)
	{
		return base.AnimationSet.GetSpecialAttack(type);
	}

	protected override void UpdateAnimations(float dt)
	{
		base.UpdateAnimations(dt);
		m_DecoratorManager?.Update(dt);
	}

	protected override void OnAnimationSetChanged()
	{
		base.OnAnimationSetChanged();
		m_MicroIdle = (UnitAnimationActionMicroIdle)GetAction(UnitAnimationType.MicroIdle);
		m_VariantIdle = (UnitAnimationActionVariantIdle)GetAction(UnitAnimationType.VariantIdle);
		m_RandomIdleTracker = ((m_VariantIdle != null) ? m_VariantIdle.RetriggerProbability.Track(base.StatefulRandom) : null);
	}

	public override AnimationActionHandle CreateHandle(AnimationActionBase animationAction)
	{
		if (!animationAction)
		{
			Logger.Error(this, "Animation action is null");
			return null;
		}
		UnitAnimationAction unitAnimationAction = animationAction as UnitAnimationAction;
		if (!unitAnimationAction)
		{
			Logger.Error(this, "{0} doesn't support actions of type {1}", this, animationAction.GetType().Name);
			return null;
		}
		return new UnitAnimationActionHandle(unitAnimationAction, this);
	}

	public UnitAnimationActionHandle CreateHandle(UnitAnimationType type, bool errorOnEmpty = true)
	{
		UnitAnimationAction action = GetAction(type);
		if (!action)
		{
			if (errorOnEmpty && type != UnitAnimationType.Death)
			{
				Logger.Error(this, "{0} Has no animation of type {1}", this, type);
			}
			return null;
		}
		return (UnitAnimationActionHandle)CreateHandle(action);
	}

	public UnitAnimationActionHandle CreateSpecialAttackHandle(UnitAnimationSpecialAttackType type)
	{
		UnitAnimationActionSpecialAttack action = GetAction(type);
		if (action == null)
		{
			Logger.Error(this, $"Has no animation for special attack {type}");
			return null;
		}
		return (UnitAnimationActionHandle)CreateHandle(action);
	}

	public UnitAnimationActionHandle Execute(UnitAnimationType type)
	{
		UnitAnimationActionHandle unitAnimationActionHandle = CreateHandle(type, errorOnEmpty: false);
		if (unitAnimationActionHandle != null)
		{
			Execute(unitAnimationActionHandle);
		}
		return unitAnimationActionHandle;
	}

	public void CreateMainHandAttackHandlerForPrepare()
	{
		m_CurrentMainHandAttackForPrepare?.Release();
		m_CurrentMainHandAttackForPrepare = CreateHandle(UnitAnimationType.MainHandAttack);
		m_CurrentMainHandAttackForPrepare.AttackWeaponStyle = ActiveMainHandWeaponStyle;
		m_CurrentMainHandAttackForPrepare.NeedPreparingForShooting = true;
		m_CurrentMainHandAttackForPrepare.IsPreparingForShooting = true;
		base.Execute(m_CurrentMainHandAttackForPrepare);
	}

	public override void Execute(AnimationActionHandle handle)
	{
		TryInitLocoMotion();
		if (CanExecute(handle))
		{
			if (IsSleeping)
			{
				PFLog.Default.Warning($"Trying to Execute animation action on a sleeping unit, UnitAnimationManager={this}");
			}
			if (handle != m_WhileIdleHandle)
			{
				m_WhileIdleHandle?.Release();
				m_WhileIdleHandle = null;
			}
			base.Execute(handle);
		}
	}

	private bool CanExecute(AnimationActionHandle handle)
	{
		UnitAnimationAction unitAnimationAction = handle?.Action as UnitAnimationAction;
		if (!unitAnimationAction)
		{
			return true;
		}
		UnitAnimationType type = unitAnimationAction.Type;
		if (type == UnitAnimationType.Dodge || type == UnitAnimationType.JumpAsideDodge || type == UnitAnimationType.Hit)
		{
			if (!IsWaitingForIncomingAttackOfOpportunity && !IsProne)
			{
				return !IsGoingProne;
			}
			return false;
		}
		return true;
	}

	public UnitAnimationActionHandle ExecuteIfIdle(UnitAnimationType type)
	{
		return ExecuteIfIdle(CreateHandle(type));
	}

	public UnitAnimationActionHandle ExecuteIfIdle(UnitAnimationActionHandle handle)
	{
		if (handle == null || !CanRunIdleAction())
		{
			return null;
		}
		if (GetIdleActionPriority(handle) < GetIdleActionPriority(m_WhileIdleHandle))
		{
			return null;
		}
		m_WhileIdleHandle = handle;
		Execute(m_WhileIdleHandle);
		return m_WhileIdleHandle;
	}

	public bool CanRunIdleAction()
	{
		if (NewSpeed > 0f)
		{
			return false;
		}
		if (CutsceneLock.Active)
		{
			return false;
		}
		for (int i = 0; i < base.ActiveActions.Count; i++)
		{
			AnimationActionHandle animationActionHandle = base.ActiveActions[i];
			bool isAdditive = animationActionHandle.IsAdditive;
			if (animationActionHandle != m_LocoMotionHandle && !isAdditive && !animationActionHandle.IsReleased)
			{
				return false;
			}
		}
		return true;
	}

	private int GetIdleActionPriority(UnitAnimationActionHandle handle)
	{
		if (handle == null)
		{
			return int.MinValue;
		}
		if (handle.Action.Type == UnitAnimationType.MicroIdle || handle.Action.Type == UnitAnimationType.VariantIdle || handle.Action.Type == UnitAnimationType.DollRoomMicroIdle || handle.Action.Type == UnitAnimationType.DollRoomVariantIdle)
		{
			return -1;
		}
		if (handle.Action.Type == UnitAnimationType.Dodge || handle.Action.Type == UnitAnimationType.Cover)
		{
			return 1;
		}
		if (handle.Action.Type == UnitAnimationType.MainHandAttack || handle.Action.Type == UnitAnimationType.OffHandAttack || handle.Action.Type == UnitAnimationType.SpecialAttack)
		{
			return 2;
		}
		return 0;
	}

	public void OnCommandActEvent()
	{
		UnitUseAbility unitUseAbility = (UnitUseAbility.TestPauseOnCast ? (View.Data?.Commands.Current as UnitUseAbility) : null);
		foreach (AnimationActionHandle activeAction in base.ActiveActions)
		{
			activeAction.ActEventsCounter++;
			if (unitUseAbility != null && unitUseAbility.Animation == activeAction && unitUseAbility.Executor.Faction.IsPlayer && unitUseAbility.Ability.TargetAnchor != 0)
			{
				Game.Instance.IsPaused = true;
				Game.Instance.SelectedAbilityHandler.SetAbility(unitUseAbility.Ability);
				string notification = "Ready to resolve: " + unitUseAbility.Ability.Blueprint.Name;
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler l)
				{
					l.HandleWarning(notification);
				});
			}
		}
	}

	public void FastForwardProneAnimation(bool forceDeadFromProne = false)
	{
		if (m_ExclusiveState != ExclusiveStateType.Prone && m_ExclusiveState != ExclusiveStateType.Dead)
		{
			SetExclusiveAnimation(ExclusiveStateType.Prone);
		}
		if (forceDeadFromProne && m_ExclusiveHandle != null)
		{
			m_ExclusiveHandle.DeathFromProne = true;
		}
		UnitAnimationActionProne unitAnimationActionProne = m_ExclusiveHandle?.Action as UnitAnimationActionProne;
		if ((bool)unitAnimationActionProne)
		{
			unitAnimationActionProne.FastForward(m_ExclusiveHandle);
		}
		else
		{
			Logger.Warning(this, $"{this} cannot fast-forward prone animation: not prone ({m_ExclusiveState})");
		}
	}

	public void FastForwardDeathAnimation()
	{
		if (m_ExclusiveState != ExclusiveStateType.Prone && m_ExclusiveState != ExclusiveStateType.Dead)
		{
			SetExclusiveAnimation(ExclusiveStateType.Dead);
		}
		UnitAnimationActionDeath unitAnimationActionDeath = m_ExclusiveHandle?.Action as UnitAnimationActionDeath;
		if ((bool)unitAnimationActionDeath)
		{
			unitAnimationActionDeath.FastForward(m_ExclusiveHandle);
			return;
		}
		Logger.Warning(this, "{0} cannot fast-forward prone animation: not prone ({1})", this, m_ExclusiveState);
	}

	public void StandUpImmediately()
	{
		if (m_ExclusiveState == ExclusiveStateType.Prone || m_ExclusiveState == ExclusiveStateType.Dead || m_ExclusiveState == ExclusiveStateType.StandUp)
		{
			m_ExclusiveHandle.Release(0f);
			SetExclusiveAnimation(ExclusiveStateType.None);
		}
	}

	protected override void OnDisable()
	{
		if (View is UnitEntityView unitEntityView)
		{
			unitEntityView.HandsEquipment?.ForceEndChangeEquipment();
		}
		base.OnDisable();
	}

	public IEntity GetSubscribingEntity()
	{
		return View.EntityData;
	}

	public void HandleUnitJoinCombat()
	{
		PrepareForCombat();
	}

	public void HandleUnitLeaveCombat()
	{
	}
}
