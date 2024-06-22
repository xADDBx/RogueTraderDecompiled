using System;
using System.Collections.Generic;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationFall", menuName = "Animation Manager/Actions/Unit Prone")]
public class UnitAnimationActionProne : UnitAnimationAction
{
	private class ActionData
	{
		public bool FallingFinished;

		public bool HasLyingClip;

		public float FallingTime;

		public bool IsRagdoll;

		public bool StandUp;

		public bool FastForwarding;
	}

	[Serializable]
	public class WeaponStyleStandUp
	{
		public WeaponAnimationStyle Style;

		[SerializeField]
		public AnimationClipWrapper Wrapper;
	}

	private const float MinLayingTime = 0.5f;

	[Header("Falling")]
	[AssetPicker("")]
	[SerializeField]
	[ValidateNotNull]
	private AnimationClipWrapper m_Falling;

	[AssetPicker("")]
	[SerializeField]
	private AnimationClipWrapper m_DyingWhileProne;

	[SerializeField]
	[Space(5f)]
	private float m_TransitionToLyingTime = 0.2f;

	[Header("Lying")]
	[AssetPicker("")]
	[SerializeField]
	[Space(5f)]
	private AnimationClipWrapper m_Dead;

	[AssetPicker("")]
	[SerializeField]
	private AnimationClipWrapper m_Unconscious;

	public bool AllowFallingBelowGround;

	[AssetPicker("")]
	[SerializeField]
	private AnimationClipWrapper m_StandUpInIdle;

	public List<WeaponStyleStandUp> StandUpByWeaponStileInCombat;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if ((bool)m_Falling)
			{
				yield return m_Falling;
			}
			if ((bool)m_DyingWhileProne)
			{
				yield return m_DyingWhileProne;
			}
			if ((bool)m_Dead)
			{
				yield return m_Dead;
			}
			if ((bool)m_Unconscious)
			{
				yield return m_Unconscious;
			}
			if ((bool)m_StandUpInIdle)
			{
				yield return m_StandUpInIdle;
			}
			foreach (WeaponStyleStandUp item in StandUpByWeaponStileInCombat)
			{
				if ((bool)item.Wrapper)
				{
					yield return item.Wrapper;
				}
			}
		}
	}

	public override UnitAnimationType Type => UnitAnimationType.Prone;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper animationClipWrapper = ((handle.Manager.IsDead || !m_Unconscious) ? m_Dead : m_Unconscious);
		ActionData actionData2 = (ActionData)(handle.ActionData = new ActionData());
		handle.HasCrossfadePriority = true;
		AbstractUnitEntity entityData = handle.Unit.EntityData;
		if (DismembermentHandler.CanUseAnimation(entityData))
		{
			AnimationClipWrapper animationClipWrapper2 = (handle.DeathFromProne ? m_DyingWhileProne : m_Falling);
			animationClipWrapper2 = (animationClipWrapper2 ? animationClipWrapper2 : m_Falling);
			if (animationClipWrapper2 != null && (bool)animationClipWrapper2.AnimationClip)
			{
				handle.StartClip(animationClipWrapper2, animationClipWrapper ? ClipDurationType.Oneshot : ClipDurationType.Endless);
			}
			else
			{
				UberDebug.LogError((UnityEngine.Object)(((object)animationClipWrapper2) ?? ((object)this)), "Animation clip is not set for object {0}", ((object)animationClipWrapper2) ?? ((object)this));
			}
			if ((bool)animationClipWrapper)
			{
				handle.ActiveAnimation.ChangeTransitionTime(m_TransitionToLyingTime);
			}
			actionData2.HasLyingClip = animationClipWrapper;
			actionData2.FallingTime = (animationClipWrapper2 ? animationClipWrapper2.Length : 0f);
		}
		else
		{
			handle.Manager.PlayableGraph.Evaluate();
			handle.Manager.Animator.enabled = false;
			handle.Manager.StopEvents();
			actionData2.FallingTime = handle.Unit.RigidbodyController?.RagdollTime ?? 1f;
			actionData2.IsRagdoll = true;
			DismembermentHandler.UseWithoutAnimationDeath(entityData);
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		AbstractUnitEntity data = handle.Unit.Data;
		if (data.LifeState.IsFinallyDead && handle.DeathFromProne && !actionData.FastForwarding && !data.GetOptional<UnitPartCompanion>())
		{
			actionData.FallingFinished = true;
			handle.Unit.Animator.enabled = false;
			handle.Manager.SuspendEvents();
			handle.Release();
			handle.Manager.Disabled = true;
		}
		else if (!actionData.StandUp)
		{
			if (actionData.HasLyingClip && !actionData.FallingFinished)
			{
				actionData.FallingFinished = true;
				AnimationClipWrapper lyingClip = GetLyingClip(handle);
				handle.StartClip(lyingClip, ClipDurationType.Endless);
				handle.ActiveAnimation.TransitionIn = m_TransitionToLyingTime;
			}
			else
			{
				handle.Release();
			}
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		base.OnUpdate(handle, deltaTime);
		AbstractUnitEntity data = handle.Unit.Data;
		if (data == null || (data.LifeState.IsFinallyDead && handle.DeathFromProne && !data.GetOptional<UnitPartCompanion>()))
		{
			ActionData actionData = (ActionData)handle.ActionData;
			if (!actionData.IsRagdoll && IsActuallyProne(handle) && handle.GetTime() >= actionData.FallingTime + 0.5f)
			{
				handle.Unit.Animator.enabled = false;
				handle.FinishInternal();
				handle.Manager.StopEvents();
				handle.Release();
				handle.Manager.Disabled = true;
			}
		}
		if (handle.ActiveAnimation == null)
		{
			handle.Release();
		}
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		if ((bool)handle.Unit.RigidbodyController && handle.Unit.RigidbodyController.IsActive && ((ActionData)handle.ActionData).IsRagdoll)
		{
			handle.Manager.Animator.enabled = true;
			handle.Manager.StartEvents();
		}
	}

	public void SwitchToExit(UnitAnimationActionHandle handle)
	{
		if (handle.ActionData == null)
		{
			handle.Release();
			return;
		}
		ActionData actionData = handle.ActionData as ActionData;
		if (actionData.StandUp)
		{
			return;
		}
		actionData.StandUp = true;
		AnimationClipWrapper animationClipWrapper = m_StandUpInIdle;
		AbstractUnitEntity data = handle.Unit.Data;
		if (data != null && data.IsInCombat)
		{
			animationClipWrapper = StandUpByWeaponStileInCombat.FirstItem((WeaponStyleStandUp x) => x.Style == handle.Manager.ActiveMainHandWeaponStyle)?.Wrapper;
			if (animationClipWrapper == null)
			{
				animationClipWrapper = m_StandUpInIdle;
			}
		}
		if ((bool)animationClipWrapper)
		{
			handle.StartClip(animationClipWrapper, ClipDurationType.Oneshot);
			handle.ActiveAnimation.DoNotZeroOtherAnimations = true;
			OnUpdate(handle, 0f);
		}
		else
		{
			handle.Release();
		}
	}

	public void FastForward(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (handle.ActiveAnimation == null)
		{
			actionData.FallingFinished = true;
			if (actionData.IsRagdoll)
			{
				PartSavedRagdollState optional = handle.Unit.EntityData.GetOptional<PartSavedRagdollState>();
				if (optional != null && optional.Active)
				{
					return;
				}
			}
			handle.Unit.RigidbodyController.CancelRagdoll();
			AnimationClipWrapper lyingClip = GetLyingClip(handle);
			if ((bool)lyingClip)
			{
				handle.StartClip(lyingClip, ClipDurationType.Endless);
				return;
			}
			handle.StartClip(m_Falling, ClipDurationType.Endless);
		}
		actionData.FastForwarding = true;
		if (actionData.HasLyingClip)
		{
			if (!actionData.FallingFinished)
			{
				handle.ActiveAnimation.TransitionOut = 0f;
				handle.ActiveAnimation.StartTransitionOut();
				handle.ActiveAnimation.StopEvents();
				handle.ActiveAnimation.TransitionIn = 0f;
			}
		}
		else
		{
			handle.ActiveAnimation.SetTime(10f);
			handle.ActiveAnimation.SetWeight(1f);
			handle.UpdateInternal(0.1f);
		}
		actionData.FastForwarding = false;
		actionData.FallingFinished = true;
	}

	public bool IsActuallyProne(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		if (actionData.FallingFinished || handle.IsReleased)
		{
			return true;
		}
		if (!actionData.HasLyingClip && handle.GetTime() >= actionData.FallingTime)
		{
			return true;
		}
		return false;
	}

	private AnimationClipWrapper GetLyingClip(UnitAnimationActionHandle handle)
	{
		AnimationClipWrapper animationClipWrapper = ((!handle.Manager.IsDead) ? m_Unconscious : m_Dead);
		if (!animationClipWrapper)
		{
			return m_Dead;
		}
		return animationClipWrapper;
	}
}
