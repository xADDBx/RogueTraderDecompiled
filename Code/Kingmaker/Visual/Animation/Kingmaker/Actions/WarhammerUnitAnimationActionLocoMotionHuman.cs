using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Kingmaker.View.Animation;
using Owlcat.Runtime.Core.Logging;
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

		public MovementState State;

		public WeaponAnimationStyle MainHandWeaponStyle { get; set; }

		public WeaponAnimationStyle OffHandWeaponStyle { get; set; }
	}

	[SerializeField]
	private bool m_ForDollRoom;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public AnimationClipWrapper NonCombatIdle;

	public MovementStyleLayer NonCombatWalk;

	public List<WeaponStyleIdleLayer> CombatIdle;

	public List<WeaponStyleLayer> CombatWalk;

	public AvatarMask OffHandMask;

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

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		ActionData actionData2 = (ActionData)(handle.ActionData = new ActionData());
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
		AnimationClipWrapper animationClipWrapper = SelectClip(handle, forOffhand: true);
		AnimationClipWrapper animationClipWrapper2 = SelectClip(handle, forOffhand: false);
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
		if (actionData.State == MovementState.Idle)
		{
			handle.ActiveAnimation?.SetTime(PFStatefulRandom.Visuals.AnimationIdle.value * animationClipWrapper2.Length);
		}
		handle.ActiveAnimation?.SetSpeed(1f);
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
		bool flag2 = handle.Unit?.MovementAgent is UnitMovementAgentContinuous;
		WeaponAnimationStyle activeMainHandWeaponStyle = handle.Manager.ActiveMainHandWeaponStyle;
		MovementStyleLayer movementStyleLayer = NonCombatWalk;
		foreach (WeaponStyleLayer item in CombatWalk)
		{
			if (item.Style == activeMainHandWeaponStyle)
			{
				movementStyleLayer = item.MovementStyleLayer;
				break;
			}
		}
		CurrentWalkingStyleLayer currentWalkingStyleLayer = SelectWalkingStyleLayer(movementStyleLayer, actionData.WalkSpeedType);
		if (currentWalkingStyleLayer != null)
		{
			switch (actionData.State)
			{
			case MovementState.In:
				if (currentWalkingStyleLayer.OutSpeed != 0f && currentWalkingStyleLayer.OutDistance > 0f)
				{
					handle.Manager.NewSpeed = currentWalkingStyleLayer.InSpeed;
				}
				else
				{
					handle.Manager.NewSpeed = currentWalkingStyleLayer.InCurve.Evaluate((handle.GetTime() - actionData.StartAnimationTime) / (actionData.EndAnimationTime - actionData.StartAnimationTime)) * currentWalkingStyleLayer.Speed;
				}
				break;
			case MovementState.Out:
				if (currentWalkingStyleLayer.OutSpeed != 0f && currentWalkingStyleLayer.OutDistance > 0f)
				{
					handle.Manager.NewSpeed = currentWalkingStyleLayer.OutSpeed;
				}
				else
				{
					handle.Manager.NewSpeed = currentWalkingStyleLayer.OutCurve.Evaluate((handle.GetTime() - actionData.StartAnimationTime) / (actionData.EndAnimationTime - actionData.StartAnimationTime)) * currentWalkingStyleLayer.Speed;
				}
				break;
			}
		}
		if (actionData.State == MovementState.In && handle.GetTime() > actionData.EndAnimationTime)
		{
			actionData.State = MovementState.Run;
			flag = true;
		}
		if (actionData.State == MovementState.Out && handle.GetTime() > actionData.EndAnimationTime)
		{
			actionData.State = MovementState.Idle;
			flag = true;
		}
		if (actionData.State == MovementState.Out && flag2)
		{
			actionData.State = MovementState.Idle;
			flag = true;
		}
		if (currentWalkingStyleLayer != null && actionData.State == MovementState.Run && handle.Unit != null && !flag2)
		{
			ForcedPath path = handle.Unit.AgentASP.Path;
			if (path != null && path.vectorPath?.Count > 0 && (handle.Unit.AgentASP.Path.vectorPath.Last() - handle.Unit.gameObject.transform.position).magnitude < currentWalkingStyleLayer.OutDistance)
			{
				actionData.State = MovementState.Out;
				flag = true;
			}
		}
		if (actionData.State == MovementState.Run && handle.Unit != null && handle.Unit.AgentASP.IsInNodeLinkQueue && !flag2)
		{
			actionData.State = MovementState.Out;
			flag = true;
		}
		if (actionData.State == MovementState.Idle && handle.Unit != null && !handle.Unit.AgentASP.IsInNodeLinkQueue)
		{
			AbstractUnitEntity data = handle.Unit.Data;
			if (data != null && data.Commands.Contains((AbstractUnitCommand x) => x.IsMoveUnit && x.IsStarted))
			{
				actionData.State = MovementState.In;
				flag = true;
			}
		}
		if (currentWalkingStyleLayer != null && actionData.State == MovementState.Out && handle.Unit != null && !flag2)
		{
			ForcedPath path2 = handle.Unit.AgentASP.Path;
			if (path2 != null && path2.vectorPath?.Count > 0 && (handle.Unit.AgentASP.Path.vectorPath.Last() - handle.Unit.gameObject.transform.position).magnitude > currentWalkingStyleLayer.OutDistance + 0.5f)
			{
				actionData.State = MovementState.Run;
				flag = true;
			}
		}
		if (currentWalkingStyleLayer != null && actionData.State != MovementState.Idle && handle.Unit != null && !UnitAnimationManager.HasMovingCommand(handle.Unit.Data) && !handle.Unit.MovementAgent.IsReallyMoving)
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
		else if (actionData.State != MovementState.Idle)
		{
			handle.ActiveAnimation?.SetSpeed(1f);
		}
		if (actionData.State == MovementState.Out && currentWalkingStyleLayer != null && currentWalkingStyleLayer.Out != null && currentWalkingStyleLayer.OutSpeed > 0f && currentWalkingStyleLayer.OutDistance > 0f)
		{
			handle.ActiveAnimation?.SetSpeed(currentWalkingStyleLayer.Out.Length / (currentWalkingStyleLayer.OutDistance / currentWalkingStyleLayer.OutSpeed));
		}
	}

	private AnimationClipWrapper SelectClip(UnitAnimationActionHandle handle, bool forOffhand)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData == null)
		{
			return NonCombatIdle;
		}
		WeaponAnimationStyle style = (forOffhand ? actionData.OffHandWeaponStyle : actionData.MainHandWeaponStyle);
		if (forOffhand)
		{
			WeaponStyleIdleLayer weaponStyleIdleLayer = CombatIdle.FirstItem((WeaponStyleIdleLayer s) => s.Style == handle.Manager.ActiveMainHandWeaponStyle && !s.IsOffHand);
			if (weaponStyleIdleLayer != null && weaponStyleIdleLayer.NoOffHand)
			{
				return null;
			}
		}
		WeaponStyleIdleLayer weaponStyleIdleLayer2 = CombatIdle.FirstItem((WeaponStyleIdleLayer s) => s.Style == style && s.IsOffHand == forOffhand);
		CurrentWalkingStyleLayer currentWalkingStyleLayer = SelectWalkingStyleLayer((!actionData.InCombat) ? NonCombatWalk : (CombatWalk.FirstItem((WeaponStyleLayer s) => s.Style == style && s.IsOffHand == forOffhand)?.MovementStyleLayer ?? NonCombatWalk), actionData.WalkSpeedType);
		if (currentWalkingStyleLayer != null)
		{
			switch (actionData.State)
			{
			case MovementState.Out:
				if (currentWalkingStyleLayer.Out != null)
				{
					actionData.StartAnimationTime = handle.GetTime();
					actionData.EndAnimationTime = handle.GetTime() + currentWalkingStyleLayer.Out.Length * (currentWalkingStyleLayer.Out.Length / (currentWalkingStyleLayer.OutDistance / currentWalkingStyleLayer.OutSpeed));
					return currentWalkingStyleLayer.Out;
				}
				handle.Manager.NewSpeed = 0f;
				if (!handle.Manager.IsInCombat || weaponStyleIdleLayer2 == null)
				{
					return NonCombatIdle;
				}
				return weaponStyleIdleLayer2.Wrapper;
			case MovementState.Run:
				handle.Manager.NewSpeed = currentWalkingStyleLayer.Speed;
				return currentWalkingStyleLayer.Clip;
			case MovementState.In:
				if (currentWalkingStyleLayer.In != null)
				{
					actionData.StartAnimationTime = handle.GetTime();
					actionData.EndAnimationTime = handle.GetTime() + currentWalkingStyleLayer.In.Length;
					return currentWalkingStyleLayer.In;
				}
				actionData.State = MovementState.Run;
				handle.Manager.NewSpeed = currentWalkingStyleLayer.Speed;
				return currentWalkingStyleLayer.Clip;
			case MovementState.Idle:
				handle.Manager.NewSpeed = 0f;
				if (weaponStyleIdleLayer2 != null && handle.Manager.IsInCombat)
				{
					return weaponStyleIdleLayer2.Wrapper;
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
