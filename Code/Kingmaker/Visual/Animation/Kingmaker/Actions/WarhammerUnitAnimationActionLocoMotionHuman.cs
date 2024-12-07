using System;
using System.Collections.Generic;
using Code.Visual.Animation;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerUnitAnimationLocoMotion", menuName = "Animation Manager/Actions/Warhammer Unit Loco Motion (Human)")]
public class WarhammerUnitAnimationActionLocoMotionHuman : UnitAnimationAction
{
	private enum MovementState
	{
		In,
		Run,
		Out,
		Idle
	}

	[Serializable]
	public class WeaponStyleLayer
	{
		public WeaponAnimationStyle Style;

		public bool IsOffHand;

		[SerializeField]
		public MovementStyleLayer MovementStyleLayer;
	}

	[Serializable]
	public class WeaponStyleIdleLayer
	{
		public WeaponAnimationStyle Style;

		public bool IsOffHand;

		public bool NoOffHand;

		[SerializeField]
		public AnimationClipWrapper Wrapper;
	}

	[Serializable]
	public class MovementStyleLayer
	{
		public CurrentWalkingStyleLayer Crouch;

		public CurrentWalkingStyleLayer Walking;

		public CurrentWalkingStyleLayer Run;

		public CurrentWalkingStyleLayer Sprint;

		public List<AnimationClipWrapper> GetAllWrappers()
		{
			List<AnimationClipWrapper> list = new List<AnimationClipWrapper>();
			list.AddRange(Crouch.GetAllWrappers());
			list.AddRange(Walking.GetAllWrappers());
			list.AddRange(Run.GetAllWrappers());
			list.AddRange(Sprint.GetAllWrappers());
			return list;
		}
	}

	[Serializable]
	public class CurrentWalkingStyleLayer
	{
		public AnimationCurve InCurve;

		public AnimationCurve OutCurve;

		public float OutDistance;

		public AnimationClipWrapper In;

		public AnimationClipWrapper Out;

		public AnimationClipWrapper Clip;

		public float Speed;

		public float OutSpeed;

		public float InSpeed;

		public bool UseCurves
		{
			get
			{
				if (OutSpeed != 0f)
				{
					return OutDistance <= 0f;
				}
				return true;
			}
		}

		public List<AnimationClipWrapper> GetAllWrappers()
		{
			return new List<AnimationClipWrapper> { In, Out, Clip };
		}
	}

	private class ActionData
	{
		public WalkSpeedType WalkSpeedType;

		public bool InCombat;

		public float EndAnimationTime;

		public float StartAnimationTime;

		public float OutDistance;

		public MovementState PreviousState;

		private MovementState m_StateCurrent;

		public WeaponAnimationStyle MainHandWeaponStyle { get; set; }

		public WeaponAnimationStyle OffHandWeaponStyle { get; set; }

		public MovementState State
		{
			get
			{
				return m_StateCurrent;
			}
			set
			{
				if (m_StateCurrent != value)
				{
					PreviousState = m_StateCurrent;
					m_StateCurrent = value;
				}
			}
		}
	}

	[SerializeField]
	private float m_CrossfadeToRun;

	[SerializeField]
	private float m_CrossfadeToOut;

	[SerializeField]
	private bool m_ForDollRoom;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public AnimationClipWrapper NonCombatIdle;

	public MovementStyleLayer NonCombatWalk;

	public List<WeaponStyleIdleLayer> CombatIdle;

	public List<WeaponStyleLayer> CombatWalk;

	public AvatarMask OffHandMask;

	private float CrossfadeToRun
	{
		get
		{
			if (m_CrossfadeToRun.Approximately(0f))
			{
				return TransitionIn;
			}
			return m_CrossfadeToRun;
		}
	}

	private float CrossfadeToOut
	{
		get
		{
			if (m_CrossfadeToRun.Approximately(0f))
			{
				return TransitionIn;
			}
			return m_CrossfadeToOut;
		}
	}

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if (m_ClipWrappersHashSet != null)
			{
				return m_ClipWrappersHashSet;
			}
			m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper> { NonCombatIdle };
			m_ClipWrappersHashSet.AddRange(NonCombatWalk.GetAllWrappers());
			foreach (WeaponStyleIdleLayer item in CombatIdle)
			{
				m_ClipWrappersHashSet.Add(item.Wrapper);
			}
			foreach (WeaponStyleLayer item2 in CombatWalk)
			{
				m_ClipWrappersHashSet.AddRange(item2.MovementStyleLayer.GetAllWrappers());
			}
			return m_ClipWrappersHashSet;
		}
	}

	public override bool DontReleaseOnInterrupt => true;

	public override bool SupportCaching => true;

	public override UnitAnimationType Type
	{
		get
		{
			if (!m_ForDollRoom)
			{
				return UnitAnimationType.LocoMotion;
			}
			return UnitAnimationType.DollRoomLocoMotion;
		}
	}

	private static bool IsGamepadMovement(UnitAnimationActionHandle handle)
	{
		if (!(handle.Unit?.Or(null)?.MovementAgent is UnitMovementAgentContinuous))
		{
			if (handle.Unit?.EntityData.Commands.Current is UnitFollow { Params: { } @params })
			{
				return @params.IsGamepadMovement;
			}
			return false;
		}
		return true;
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		ActionData actionData2 = (ActionData)(handle.ActionData = new ActionData());
		handle.SkipFirstTick = false;
		handle.SkipFirstTickOnHandle = false;
		actionData2.State = MovementState.Idle;
		UpdateCurrentClips(handle);
	}

	private void UpdateCurrentClips(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData == null)
		{
			return;
		}
		actionData.MainHandWeaponStyle = handle.Manager.ActiveMainHandWeaponStyle;
		actionData.OffHandWeaponStyle = handle.Manager.ActiveOffHandWeaponStyle;
		actionData.InCombat = handle.Manager.IsInCombat;
		actionData.WalkSpeedType = handle.Manager.WalkSpeedType;
		CurrentWalkingStyleLayer walkingStyleLair;
		AnimationClipWrapper animationClipWrapper = SelectClip(handle, forOffhand: true, out walkingStyleLair);
		CurrentWalkingStyleLayer walkingStyleLair2;
		AnimationClipWrapper animationClipWrapper2 = SelectClip(handle, forOffhand: false, out walkingStyleLair2);
		AnimationBase activeAnimation = handle.ActiveAnimation;
		if ((bool)animationClipWrapper)
		{
			if ((bool)animationClipWrapper.AnimationClip)
			{
				handle.Manager.AddAnimationClip(handle, animationClipWrapper2, null, useEmptyAvatarMask: true, isAdditive: false, ClipDurationType.Endless, new AnimationComposition(handle));
				handle.Manager.AddClipToComposition(handle, animationClipWrapper, OffHandMask, isAdditive: false);
			}
			else
			{
				UberDebug.LogError(animationClipWrapper, "Offhand locomotion animation clip is not set, object {0}", animationClipWrapper.name);
			}
		}
		else if ((bool)animationClipWrapper2.AnimationClip)
		{
			handle.Manager.AddAnimationClip(handle, animationClipWrapper2, ClipDurationType.Endless);
		}
		else
		{
			UberDebug.LogError(animationClipWrapper2, "Main locomotion animation clip is not set, object {0}", animationClipWrapper2.name);
		}
		handle.ActiveAnimation?.SetSpeed(1f);
		switch (actionData.State)
		{
		case MovementState.In:
			if (handle.ActiveAnimation != null && walkingStyleLair2 != null && walkingStyleLair2.In != null)
			{
				actionData.StartAnimationTime = handle.GetTime();
				actionData.EndAnimationTime = handle.GetTime() + walkingStyleLair2.In.Length;
			}
			break;
		case MovementState.Run:
			if (handle.ActiveAnimation != null && activeAnimation != null && actionData.PreviousState == MovementState.In)
			{
				activeAnimation.TransitionOut = CrossfadeToRun;
				handle.ActiveAnimation.TransitionIn = CrossfadeToRun;
			}
			break;
		case MovementState.Out:
			if (handle.ActiveAnimation == null)
			{
				break;
			}
			if (walkingStyleLair2 != null && walkingStyleLair2.Out != null)
			{
				float num = actionData.OutDistance / walkingStyleLair2.OutSpeed;
				actionData.StartAnimationTime = handle.GetTime();
				actionData.EndAnimationTime = handle.GetTime() + num;
				if (walkingStyleLair2.OutSpeed > 0f && actionData.OutDistance > walkingStyleLair2.OutDistance / 3f)
				{
					handle.ActiveAnimation.SetSpeed(walkingStyleLair2.Out.Length / num);
				}
			}
			if (activeAnimation != null)
			{
				activeAnimation.TransitionOut = CrossfadeToOut;
				handle.ActiveAnimation.TransitionIn = CrossfadeToOut * handle.ActiveAnimation.GetSpeed();
			}
			handle.ActiveAnimation.ChangeTransitionTime(TransitionOut * handle.ActiveAnimation.GetSpeed());
			break;
		case MovementState.Idle:
			if ((bool)animationClipWrapper2.AnimationClip && activeAnimation == null)
			{
				handle.ActiveAnimation?.SetTime(PFStatefulRandom.Visuals.AnimationIdle.value * animationClipWrapper2.Length);
			}
			break;
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		UnitAnimationManager manager = handle.Manager;
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData == null)
		{
			return;
		}
		bool flag = false;
		WeaponAnimationStyle activeMainHandWeaponStyle = handle.Manager.ActiveMainHandWeaponStyle;
		MovementStyleLayer movementStyleLayer = NonCombatWalk;
		if (actionData.InCombat)
		{
			foreach (WeaponStyleLayer item in CombatWalk)
			{
				if (item.Style == activeMainHandWeaponStyle)
				{
					movementStyleLayer = item.MovementStyleLayer;
					break;
				}
			}
		}
		CurrentWalkingStyleLayer currentWalkingStyleLayer = SelectWalkingStyleLayer(movementStyleLayer, actionData.WalkSpeedType);
		if (actionData.State == MovementState.In && handle.GetTime() > actionData.EndAnimationTime - CrossfadeToRun - 0.001f)
		{
			actionData.State = MovementState.Run;
			flag = true;
		}
		if (actionData.State == MovementState.Out && handle.GetTime() > actionData.EndAnimationTime - deltaTime - 0.001f)
		{
			actionData.State = MovementState.Idle;
			flag = true;
		}
		bool flag2 = IsGamepadMovement(handle);
		if (actionData.State == MovementState.Run && handle.Unit != null && handle.Unit.AgentASP.IsInNodeLinkQueue && !flag2)
		{
			actionData.State = MovementState.Idle;
			flag = true;
		}
		if (actionData.State == MovementState.Idle && !flag && handle.Unit != null && !handle.Unit.AgentASP.IsInNodeLinkQueue)
		{
			AbstractUnitEntity data = handle.Unit.Data;
			if (((data != null && data.Commands.Contains((AbstractUnitCommand x) => x.IsMoveUnit && x.IsStarted) && handle.Unit.MovementAgent.IsReallyMoving) || handle.Unit.MovementAgent.IsCharging) && !handle.Manager.IsGoingCover && (actionData.EndAnimationTime.Approximately(0f) || handle.GetTime() > actionData.EndAnimationTime))
			{
				actionData.State = MovementState.Run;
				if (!flag2 && currentWalkingStyleLayer != null && currentWalkingStyleLayer.In != null)
				{
					float num = (currentWalkingStyleLayer.In.Length - CrossfadeToRun) * currentWalkingStyleLayer.InSpeed;
					if (handle.Unit.AgentASP.RemainingDistance > num + currentWalkingStyleLayer.OutDistance)
					{
						actionData.State = MovementState.In;
					}
				}
				flag = true;
			}
		}
		if (currentWalkingStyleLayer != null && actionData.State == MovementState.Out && handle.Unit != null && !flag2 && handle.Unit.AgentASP.RemainingDistance > currentWalkingStyleLayer.OutDistance + 0.5f)
		{
			actionData.State = MovementState.Run;
			flag = true;
		}
		if (currentWalkingStyleLayer != null && actionData.State == MovementState.Run && handle.Unit != null && !flag2 && handle.Unit.AgentASP.RemainingDistance < currentWalkingStyleLayer.OutDistance && handle.Unit.AgentASP.RemainingDistance > currentWalkingStyleLayer.OutDistance / 3f && handle.Unit.AgentASP.AngleToNextWaypoint < 5f)
		{
			actionData.State = MovementState.Out;
			actionData.OutDistance = handle.Unit.AgentASP.RemainingDistance + currentWalkingStyleLayer.OutSpeed * deltaTime;
			flag = true;
		}
		if (currentWalkingStyleLayer != null && actionData.State != MovementState.Idle && handle.Unit != null && !handle.Unit.MovementAgent.IsCharging && (!UnitAnimationManager.HasMovingCommand(handle.Unit.Data) || !handle.Unit.MovementAgent.IsReallyMoving))
		{
			actionData.State = MovementState.Idle;
			flag = true;
		}
		bool flag3 = flag || actionData.WalkSpeedType != handle.Manager.WalkSpeedType;
		AnimationBase animationBase = manager.CurrentEquipHandle?.ActiveAnimation;
		if (animationBase == null || animationBase.GetWeight() > 0.99f)
		{
			flag3 = flag3 || actionData.InCombat != manager.IsInCombat || (manager.IsInCombat && (actionData.MainHandWeaponStyle != manager.ActiveMainHandWeaponStyle || actionData.OffHandWeaponStyle != manager.ActiveOffHandWeaponStyle));
		}
		if (flag3 && handle.Manager.Animator.enabled)
		{
			handle.ActiveAnimation?.StartTransitionOut();
			handle.ActiveAnimation?.StopEvents();
			UpdateCurrentClips(handle);
		}
		else if (flag3 && handle.Manager.IsDead)
		{
			handle.ActiveAnimation?.StartTransitionOut();
			handle.ActiveAnimation?.StopEvents();
		}
		UpdateMovementSpeed(actionData, handle, currentWalkingStyleLayer);
	}

	private static void UpdateMovementSpeed(ActionData data, UnitAnimationActionHandle handle, CurrentWalkingStyleLayer walkingStyleLayer)
	{
		if (walkingStyleLayer != null)
		{
			handle.Manager.NewSpeed = GetMovementSpeed(data, handle, walkingStyleLayer);
		}
	}

	private static float GetMovementSpeed(ActionData data, UnitAnimationActionHandle handle, CurrentWalkingStyleLayer walkingStyleLayer)
	{
		switch (data.State)
		{
		case MovementState.In:
			if (walkingStyleLayer.UseCurves)
			{
				return GetSpeedFromCurve(walkingStyleLayer.InCurve);
			}
			return walkingStyleLayer.InSpeed;
		case MovementState.Out:
			if (walkingStyleLayer.UseCurves)
			{
				return GetSpeedFromCurve(walkingStyleLayer.OutCurve);
			}
			return walkingStyleLayer.OutSpeed;
		case MovementState.Run:
			return walkingStyleLayer.Speed;
		default:
			return 0f;
		}
		float GetSpeedFromCurve(AnimationCurve curve)
		{
			return curve.Evaluate((handle.GetTime() - data.StartAnimationTime) / (data.EndAnimationTime - data.StartAnimationTime)) * walkingStyleLayer.Speed;
		}
	}

	private AnimationClipWrapper SelectClip(UnitAnimationActionHandle handle, bool forOffhand, out CurrentWalkingStyleLayer walkingStyleLair)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData == null)
		{
			walkingStyleLair = null;
			return NonCombatIdle;
		}
		WeaponAnimationStyle style = (forOffhand ? actionData.OffHandWeaponStyle : actionData.MainHandWeaponStyle);
		if (forOffhand)
		{
			WeaponStyleIdleLayer weaponStyleIdleLayer = CombatIdle.FirstItem((WeaponStyleIdleLayer s) => s.Style == handle.Manager.ActiveMainHandWeaponStyle && !s.IsOffHand);
			if (weaponStyleIdleLayer != null && weaponStyleIdleLayer.NoOffHand)
			{
				walkingStyleLair = null;
				return null;
			}
		}
		AnimationClipWrapper animationClipWrapper = ((!actionData.InCombat) ? NonCombatIdle : (CombatIdle.FirstItem((WeaponStyleIdleLayer s) => s.Style == style && s.IsOffHand == forOffhand)?.Wrapper ?? NonCombatIdle));
		walkingStyleLair = SelectWalkingStyleLayer((!actionData.InCombat) ? NonCombatWalk : (CombatWalk.FirstItem((WeaponStyleLayer s) => s.Style == style && s.IsOffHand == forOffhand)?.MovementStyleLayer ?? NonCombatWalk), actionData.WalkSpeedType);
		if (walkingStyleLair != null)
		{
			switch (actionData.State)
			{
			case MovementState.Out:
				if (walkingStyleLair.Out != null)
				{
					return walkingStyleLair.Out;
				}
				if (!handle.Manager.IsInCombat || !(animationClipWrapper != null))
				{
					return NonCombatIdle;
				}
				return animationClipWrapper;
			case MovementState.Run:
				return walkingStyleLair.Clip;
			case MovementState.In:
				if (walkingStyleLair.In != null)
				{
					return walkingStyleLair.In;
				}
				actionData.State = MovementState.Run;
				return walkingStyleLair.Clip;
			case MovementState.Idle:
				if (animationClipWrapper != null && handle.Manager.IsInCombat)
				{
					return animationClipWrapper;
				}
				if (!(handle.Manager.IsInDollRoom && forOffhand))
				{
					return NonCombatIdle;
				}
				return null;
			}
		}
		if (forOffhand)
		{
			return null;
		}
		return NonCombatIdle;
	}

	private CurrentWalkingStyleLayer SelectWalkingStyleLayer(MovementStyleLayer movementStyleLayer, WalkSpeedType t)
	{
		switch (t)
		{
		case WalkSpeedType.Crouch:
			if (movementStyleLayer.Crouch != null)
			{
				return movementStyleLayer.Crouch;
			}
			break;
		case WalkSpeedType.Sprint:
			if (movementStyleLayer.Sprint != null)
			{
				return movementStyleLayer.Sprint;
			}
			break;
		case WalkSpeedType.Walk:
			if (movementStyleLayer.Walking != null)
			{
				return movementStyleLayer.Walking;
			}
			break;
		case WalkSpeedType.Run:
			if (movementStyleLayer.Run != null)
			{
				return movementStyleLayer.Run;
			}
			break;
		}
		if (movementStyleLayer.Run != null && movementStyleLayer.Run.Clip != null)
		{
			return movementStyleLayer.Run;
		}
		if (movementStyleLayer.Walking != null && movementStyleLayer.Walking.Clip != null)
		{
			return movementStyleLayer.Walking;
		}
		if (movementStyleLayer.Sprint != null && movementStyleLayer.Sprint.Clip != null)
		{
			return movementStyleLayer.Sprint;
		}
		if (movementStyleLayer.Crouch != null && movementStyleLayer.Crouch.Clip != null)
		{
			return movementStyleLayer.Crouch;
		}
		UberDebug.LogError(this, "selected clip from speed return default null");
		return null;
	}
}
