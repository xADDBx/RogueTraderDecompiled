using System;
using System.Collections.Generic;
using System.Linq;
using Code.Visual.Animation;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Animation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "WarhammerUnitAnimationLocoMotion", menuName = "Animation Manager/Actions/Warhammer Unit Loco Motion")]
public class WarhammerUnitAnimationActionLocoMotion : UnitAnimationAction
{
	private enum MovementState
	{
		In,
		Run,
		Out,
		Idle
	}

	[Serializable]
	public class WeaponStyleWalkingLayer
	{
		public WeaponAnimationStyle Style;

		public bool IsOffHand;

		[SerializeField]
		public WalkingStyleLayer WalkingStyleLayer;
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
	public class WalkingStyleLayer
	{
		public AnimationCurve RunInCurve;

		public AnimationCurve RunOutCurve;

		public float RunOutDistance;

		public AnimationClipWrapper RunIn;

		public AnimationClipWrapper RunOut;

		public AnimationClipWrapper Crouch;

		public float CrouchSpeed;

		public AnimationClipWrapper Walking;

		public float WalkingSpeed;

		public AnimationClipWrapper Run;

		public float RunSpeed;

		public AnimationClipWrapper Sprint;

		public float SprintSpeed;

		public List<AnimationClipWrapper> GetAllWrappers()
		{
			return new List<AnimationClipWrapper> { RunIn, RunOut, Crouch, Walking, Run, Sprint };
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

	private static class WeaponAnimationStyleChecker
	{
		public static WeaponAnimationStyle Style;

		public static bool Check(WeaponStyleWalkingLayer layer)
		{
			return layer.Style == Style;
		}
	}

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public AnimationClipWrapper NonCombatIdle;

	public WalkingStyleLayer NonCombatWalk;

	public List<WeaponStyleIdleLayer> CombatIdle;

	public List<WeaponStyleWalkingLayer> CombatWalk;

	public AvatarMask OffHandMask;

	public bool SkipIdleDesync;

	private static Func<WeaponStyleWalkingLayer, bool> CheckWeaponAnimationStyle = (WeaponStyleWalkingLayer layer) => WeaponAnimationStyleChecker.Check(layer);

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
			foreach (WeaponStyleWalkingLayer item2 in CombatWalk)
			{
				m_ClipWrappersHashSet.AddRange(item2.WalkingStyleLayer.GetAllWrappers());
			}
			return m_ClipWrappersHashSet;
		}
	}

	public override bool DontReleaseOnInterrupt => true;

	public override bool SupportCaching => true;

	public override UnitAnimationType Type => UnitAnimationType.LocoMotion;

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
		AnimationClipWrapper animationClipWrapper = SelectClip(handle, forOffhand: false);
		AnimationClipWrapper animationClipWrapper2 = SelectClip(handle, forOffhand: true);
		if ((bool)animationClipWrapper2)
		{
			if ((bool)animationClipWrapper2.AnimationClip)
			{
				handle.Manager.AddAnimationClip(handle, animationClipWrapper, null, useEmptyAvatarMask: true, isAdditive: false, ClipDurationType.Endless, new AnimationComposition(handle));
				handle.Manager.AddClipToComposition(handle, animationClipWrapper2, OffHandMask, isAdditive: false);
			}
			else
			{
				UberDebug.LogError(animationClipWrapper2, "Offhand locomotion animation clip is not set, object {0}", animationClipWrapper2.name);
			}
		}
		else if ((bool)animationClipWrapper.AnimationClip)
		{
			handle.Manager.AddAnimationClip(handle, animationClipWrapper, ClipDurationType.Endless);
		}
		else
		{
			UberDebug.LogError(animationClipWrapper, "Main locomotion animation clip is not set, object {0}", animationClipWrapper.name);
		}
		if (actionData.State == MovementState.Idle && !SkipIdleDesync)
		{
			handle.ActiveAnimation?.SetTime(PFStatefulRandom.Visuals.AnimationIdle.value * animationClipWrapper.Length);
		}
		handle.ActiveAnimation?.SetSpeed(1f);
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		UnitAnimationManager manager = handle.Manager;
		ActionData actionData = (ActionData)handle.ActionData;
		bool flag = false;
		WeaponAnimationStyleChecker.Style = handle.Manager.ActiveMainHandWeaponStyle;
		WeaponStyleWalkingLayer weaponStyleWalkingLayer = CombatWalk.FirstItem(CheckWeaponAnimationStyle);
		WalkingStyleLayer walkingStyleLayer = NonCombatWalk;
		if (weaponStyleWalkingLayer != null && handle.Manager.IsInCombat)
		{
			walkingStyleLayer = weaponStyleWalkingLayer.WalkingStyleLayer;
		}
		if (walkingStyleLayer != null && actionData.WalkSpeedType == WalkSpeedType.Run)
		{
			if (actionData.State == MovementState.In)
			{
				handle.Manager.NewSpeed = (walkingStyleLayer.RunInCurve?.Evaluate((handle.GetTime() - actionData.StartAnimationTime) / (actionData.EndAnimationTime - actionData.StartAnimationTime)) * walkingStyleLayer.RunSpeed) ?? walkingStyleLayer.RunSpeed;
			}
			else if (actionData.State == MovementState.Out)
			{
				handle.Manager.NewSpeed = (walkingStyleLayer.RunOutCurve?.Evaluate((handle.GetTime() - actionData.StartAnimationTime) / (actionData.EndAnimationTime - actionData.StartAnimationTime)) * walkingStyleLayer.RunSpeed) ?? walkingStyleLayer.RunSpeed;
			}
		}
		if (actionData.WalkSpeedType == WalkSpeedType.Run)
		{
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
			if (walkingStyleLayer != null && actionData.State == MovementState.Run && handle.Unit.AgentASP.Path != null && handle.Unit.AgentASP.Path.vectorPath.Count > 0 && (handle.Unit.AgentASP.Path.vectorPath.Last() - handle.Unit.gameObject.transform.position).magnitude < walkingStyleLayer.RunOutDistance)
			{
				actionData.State = MovementState.Out;
				flag = true;
			}
			if (actionData.State == MovementState.Run && handle.Unit.AgentASP.IsInNodeLinkQueue)
			{
				actionData.State = MovementState.Out;
				flag = true;
			}
		}
		if (actionData.State == MovementState.Idle && handle.Unit != null)
		{
			AbstractUnitEntity data = handle.Unit.Data;
			if (data != null && data.Commands.Contains((AbstractUnitCommand x) => x.IsMoveUnit && x.IsStarted) && !handle.Unit.AgentASP.IsInNodeLinkQueue)
			{
				actionData.State = MovementState.In;
				flag = true;
			}
		}
		if (actionData.State != MovementState.Idle && handle.Unit != null && !UnitAnimationManager.HasMovingCommand(handle.Unit.Data) && (walkingStyleLayer.RunIn == null || walkingStyleLayer.RunOut == null || actionData.WalkSpeedType != WalkSpeedType.Run))
		{
			actionData.State = MovementState.Idle;
			flag = true;
		}
		bool flag2 = actionData != null && (flag || actionData.WalkSpeedType != handle.Manager.WalkSpeedType);
		AnimationBase animationBase = manager.CurrentEquipHandle?.ActiveAnimation;
		if ((animationBase == null || animationBase.GetWeight() > 0.99f) && actionData != null)
		{
			flag2 = flag2 || actionData.InCombat != manager.IsInCombat || (manager.IsInCombat && (actionData.MainHandWeaponStyle != manager.ActiveMainHandWeaponStyle || actionData.OffHandWeaponStyle != manager.ActiveOffHandWeaponStyle));
		}
		if (flag2 && handle.Manager.Animator.enabled)
		{
			handle.ActiveAnimation?.StartTransitionOut();
			handle.ActiveAnimation?.StopEvents();
			UpdateCurrentClips(handle);
		}
		else if (flag2 && handle.Manager.IsDead)
		{
			handle.ActiveAnimation?.StartTransitionOut();
			handle.ActiveAnimation?.StopEvents();
		}
		else if (actionData.State != MovementState.Idle)
		{
			handle.ActiveAnimation?.SetSpeed(1f);
		}
	}

	private AnimationClipWrapper SelectClip(UnitAnimationActionHandle handle, bool forOffhand)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		float speed = 0f;
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
		WeaponStyleWalkingLayer weaponStyleWalkingLayer = CombatWalk.FirstItem((WeaponStyleWalkingLayer s) => s.Style == style && s.IsOffHand == forOffhand);
		WalkingStyleLayer walkingStyleLayer = NonCombatWalk;
		if (weaponStyleWalkingLayer != null && handle.Manager.IsInCombat)
		{
			walkingStyleLayer = weaponStyleWalkingLayer.WalkingStyleLayer;
		}
		if (walkingStyleLayer != null)
		{
			switch (actionData.State)
			{
			case MovementState.Out:
				if (walkingStyleLayer.RunOut != null && actionData.WalkSpeedType == WalkSpeedType.Run)
				{
					actionData.StartAnimationTime = handle.GetTime();
					actionData.EndAnimationTime = handle.GetTime() + walkingStyleLayer.RunOut.Length;
					return walkingStyleLayer.RunOut;
				}
				handle.Manager.NewSpeed = 0f;
				if (!handle.Manager.IsInCombat || weaponStyleIdleLayer2 == null)
				{
					return NonCombatIdle;
				}
				return weaponStyleIdleLayer2.Wrapper;
			case MovementState.Run:
			{
				AnimationClipWrapper animationClipWrapper = SelectClipFromSpeed(walkingStyleLayer, handle.Manager.WalkSpeedType, out speed);
				if (animationClipWrapper == null)
				{
					animationClipWrapper = SelectClipFromSpeed(walkingStyleLayer, WalkSpeedType.Run, out speed);
				}
				handle.Manager.NewSpeed = speed;
				return animationClipWrapper;
			}
			case MovementState.In:
			{
				if (walkingStyleLayer.RunIn != null && actionData.WalkSpeedType == WalkSpeedType.Run)
				{
					actionData.StartAnimationTime = handle.GetTime();
					actionData.EndAnimationTime = handle.GetTime() + walkingStyleLayer.RunIn.Length;
					return walkingStyleLayer.RunIn;
				}
				AnimationClipWrapper animationClipWrapper2 = SelectClipFromSpeed(walkingStyleLayer, handle.Manager.WalkSpeedType, out speed);
				if (animationClipWrapper2 == null)
				{
					animationClipWrapper2 = SelectClipFromSpeed(walkingStyleLayer, WalkSpeedType.Run, out speed);
				}
				handle.Manager.NewSpeed = speed;
				return animationClipWrapper2;
			}
			case MovementState.Idle:
				handle.Manager.NewSpeed = 0f;
				if (weaponStyleIdleLayer2 != null && handle.Manager.IsInCombat)
				{
					return weaponStyleIdleLayer2.Wrapper;
				}
				return NonCombatIdle;
			}
		}
		if (forOffhand)
		{
			return null;
		}
		return NonCombatIdle;
	}

	private AnimationClipWrapper SelectClipFromSpeed(WalkingStyleLayer clipWrappers, WalkSpeedType t, out float speed)
	{
		switch (t)
		{
		case WalkSpeedType.Crouch:
			if (clipWrappers.Crouch != null)
			{
				speed = clipWrappers.CrouchSpeed;
				return clipWrappers.Crouch;
			}
			break;
		case WalkSpeedType.Sprint:
			if (clipWrappers.Sprint != null)
			{
				speed = clipWrappers.SprintSpeed;
				return clipWrappers.Sprint;
			}
			break;
		case WalkSpeedType.Walk:
			if (clipWrappers.Walking != null)
			{
				speed = clipWrappers.WalkingSpeed;
				return clipWrappers.Walking;
			}
			break;
		case WalkSpeedType.Run:
			if (clipWrappers.Run != null)
			{
				speed = clipWrappers.RunSpeed;
				return clipWrappers.Run;
			}
			break;
		}
		if (clipWrappers.Run != null)
		{
			speed = clipWrappers.RunSpeed;
			return clipWrappers.Run;
		}
		if (clipWrappers.Walking != null)
		{
			speed = clipWrappers.WalkingSpeed;
			return clipWrappers.Walking;
		}
		if (clipWrappers.Sprint != null)
		{
			speed = clipWrappers.SprintSpeed;
			return clipWrappers.Sprint;
		}
		if (clipWrappers.Crouch != null)
		{
			speed = clipWrappers.CrouchSpeed;
			return clipWrappers.Crouch;
		}
		UberDebug.LogError(this, "selected clip from speed return default null");
		speed = 0f;
		return null;
	}
}
