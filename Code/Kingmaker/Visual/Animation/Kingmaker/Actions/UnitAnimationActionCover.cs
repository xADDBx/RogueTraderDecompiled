using System;
using System.Collections.Generic;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechadendrites;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationCover", menuName = "Animation Manager/Actions/Unit Cover")]
public class UnitAnimationActionCover : UnitAnimationAction
{
	private enum AnimationState
	{
		ForceEnteringTheCover,
		StepOutCover,
		Idle,
		StepInCover,
		ForceExitingTheCover
	}

	public enum StepOutDirectionAnimationType
	{
		None,
		Left,
		Right
	}

	private class Data
	{
		public LosCalculations.CoverType CoverType;

		public AnimationState CurrentAnimationState;

		public bool ActionStarted;

		public bool ActionFinished;

		public bool WaitForceExit;

		public float Time;

		public bool IsForce;
	}

	[Serializable]
	public class WeaponStyleSettings
	{
		public WeaponAnimationStyle WeaponAnimationStyle;

		public bool IsOffHand;

		public AnimationClipWrapper HalfCoverEntering;

		public AnimationClipWrapper HalfCoverIdle;

		public AnimationClipWrapper HalfCoverExiting;

		public AnimationClipWrapper FullCoverEntering;

		public AnimationClipWrapper FullCoverIdle;

		public AnimationClipWrapper FullCoverExiting;

		public AnimationClipWrapper LeftStepFullCoverEntering;

		public AnimationClipWrapper LeftStepFullCoverExiting;

		public AnimationClipWrapper RightStepFullCoverEntering;

		public AnimationClipWrapper RightStepFullCoverExiting;

		public AnimationClipWrapper FullCoverInside;

		public AnimationClipWrapper FullCoverOutside;

		private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				if (m_ClipWrappersHashSet != null)
				{
					return m_ClipWrappersHashSet;
				}
				m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
				m_ClipWrappersHashSet.Add(HalfCoverEntering);
				m_ClipWrappersHashSet.Add(HalfCoverIdle);
				m_ClipWrappersHashSet.Add(HalfCoverExiting);
				m_ClipWrappersHashSet.Add(FullCoverEntering);
				m_ClipWrappersHashSet.Add(FullCoverIdle);
				m_ClipWrappersHashSet.Add(FullCoverExiting);
				m_ClipWrappersHashSet.Add(LeftStepFullCoverEntering);
				m_ClipWrappersHashSet.Add(LeftStepFullCoverExiting);
				m_ClipWrappersHashSet.Add(RightStepFullCoverEntering);
				m_ClipWrappersHashSet.Add(RightStepFullCoverExiting);
				m_ClipWrappersHashSet.Add(FullCoverInside);
				m_ClipWrappersHashSet.Add(FullCoverOutside);
				return m_ClipWrappersHashSet;
			}
		}
	}

	private bool m_IsAdditive;

	public AnimationClipWrapper HalfCoverEntering;

	public AnimationClipWrapper HalfCoverIdle;

	public AnimationClipWrapper HalfCoverExiting;

	public AnimationClipWrapper FullCoverEntering;

	public AnimationClipWrapper FullCoverIdle;

	public AnimationClipWrapper FullCoverExiting;

	public AnimationClipWrapper LeftStepFullCoverEntering;

	public AnimationClipWrapper LeftStepFullCoverExiting;

	public AnimationClipWrapper RightStepFullCoverEntering;

	public AnimationClipWrapper RightStepFullCoverExiting;

	public AnimationClipWrapper FullCoverInside;

	public AnimationClipWrapper FullCoverOutside;

	[SerializeField]
	private List<WeaponStyleSettings> m_WeaponStyleOverrides = new List<WeaponStyleSettings>();

	public AnimationClipWrapper LeftStepFullCoverEnteringForCast;

	public AnimationClipWrapper LeftStepFullCoverExitingForCast;

	public AnimationClipWrapper RightStepFullCoverEnteringForCast;

	public AnimationClipWrapper RightStepFullCoverExitingForCast;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public override bool IsAdditive => m_IsAdditive;

	public override UnitAnimationType Type => UnitAnimationType.Cover;

	private List<WeaponStyleSettings> WeaponStyleOverrides => m_WeaponStyleOverrides;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if (m_ClipWrappersHashSet != null)
			{
				return m_ClipWrappersHashSet;
			}
			m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper>();
			m_ClipWrappersHashSet.Add(HalfCoverEntering);
			m_ClipWrappersHashSet.Add(HalfCoverIdle);
			m_ClipWrappersHashSet.Add(HalfCoverExiting);
			m_ClipWrappersHashSet.Add(FullCoverEntering);
			m_ClipWrappersHashSet.Add(FullCoverIdle);
			m_ClipWrappersHashSet.Add(FullCoverExiting);
			m_ClipWrappersHashSet.Add(LeftStepFullCoverEntering);
			m_ClipWrappersHashSet.Add(LeftStepFullCoverExiting);
			m_ClipWrappersHashSet.Add(RightStepFullCoverEntering);
			m_ClipWrappersHashSet.Add(RightStepFullCoverExiting);
			m_ClipWrappersHashSet.Add(FullCoverInside);
			m_ClipWrappersHashSet.Add(FullCoverOutside);
			m_ClipWrappersHashSet.Add(LeftStepFullCoverEnteringForCast);
			m_ClipWrappersHashSet.Add(LeftStepFullCoverExitingForCast);
			m_ClipWrappersHashSet.Add(RightStepFullCoverEnteringForCast);
			m_ClipWrappersHashSet.Add(RightStepFullCoverExitingForCast);
			foreach (WeaponStyleSettings weaponStyleOverride in WeaponStyleOverrides)
			{
				m_ClipWrappersHashSet.AddRange(weaponStyleOverride.ClipWrappers);
			}
			return m_ClipWrappersHashSet;
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
	}

	private void UpdateAnimations(UnitAnimationActionHandle handle)
	{
		Data data = (Data)handle.ActionData;
		if (data == null)
		{
			return;
		}
		switch (data.CurrentAnimationState)
		{
		case AnimationState.ForceEnteringTheCover:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				data.ActionFinished = true;
				data.ActionStarted = false;
				ChangeState(handle, AnimationState.Idle);
				StartAction(handle);
			}
			break;
		case AnimationState.StepOutCover:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				data.ActionFinished = true;
				data.ActionStarted = false;
				handle.Manager.StepOutDirectionAnimationType = StepOutDirectionAnimationType.None;
				handle.Manager.AbilityIsSpell = false;
				handle.Release();
			}
			break;
		case AnimationState.Idle:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				ChangeState(handle, AnimationState.Idle);
				handle.Manager.StepOutDirectionAnimationType = StepOutDirectionAnimationType.None;
				handle.Manager.AbilityIsSpell = false;
				StartAction(handle);
			}
			break;
		case AnimationState.StepInCover:
			handle.Manager.BlockAttackAnimation = true;
			if (data.CoverType == LosCalculations.CoverType.Full && handle.Manager.NeedStepOut && data.CurrentAnimationState == AnimationState.StepInCover && Math.Abs(handle.Manager.Orientation - handle.Manager.UseAbilityDirection) > 10f)
			{
				break;
			}
			if (handle.Manager.SequencedActions.Count > 0)
			{
				data.WaitForceExit = true;
				data.ActionFinished = true;
				data.ActionStarted = false;
				handle.Manager.BlockAttackAnimation = false;
				handle.Release();
				break;
			}
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				handle.Manager.BlockAttackAnimation = false;
				if (!handle.Manager.NeedStepOut)
				{
					data.ActionFinished = true;
					ChangeState(handle, AnimationState.StepOutCover);
					StartAction(handle);
				}
			}
			break;
		case AnimationState.ForceExitingTheCover:
			handle.Manager.BlockAttackAnimation = true;
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (!(handle.GetTime() < data.Time))
			{
				data.WaitForceExit = true;
				data.ActionFinished = true;
				data.ActionStarted = false;
				handle.Manager.BlockAttackAnimation = false;
				handle.Release();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		Data data = (Data)handle.ActionData;
		if (data != null)
		{
			data.CoverType = handle.Manager.CoverType;
			if (((data.CoverType == LosCalculations.CoverType.Half && handle.Manager.InCover && data.CurrentAnimationState != AnimationState.Idle) || (data.CoverType == LosCalculations.CoverType.Full && handle.Manager.InCover && data.CurrentAnimationState == AnimationState.StepInCover)) && data.ActionFinished)
			{
				ChangeState(handle, AnimationState.ForceEnteringTheCover);
			}
			if (!handle.Manager.InCover && data.CurrentAnimationState == AnimationState.Idle)
			{
				ChangeState(handle, AnimationState.ForceExitingTheCover);
			}
			if (data.CoverType == LosCalculations.CoverType.Full && !handle.Manager.InCover && !handle.Manager.NeedStepOut)
			{
				handle.Manager.StepOutDirectionAnimationType = StepOutDirectionAnimationType.None;
				handle.Manager.AbilityIsSpell = false;
				handle.Release();
			}
			else
			{
				UpdateAnimations(handle);
			}
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public void ForceExit(UnitAnimationActionHandle handle)
	{
		Data data = (Data)handle.ActionData;
		if (data != null)
		{
			data.IsForce = true;
		}
		ChangeState(handle, AnimationState.ForceExitingTheCover);
	}

	public bool IsCoverForceExitingFinished(UnitAnimationActionHandle handle)
	{
		return ((Data)handle.ActionData)?.WaitForceExit ?? false;
	}

	public void ChangeAdditive(bool isAdditive)
	{
		m_IsAdditive = isAdditive;
	}

	private void ChangeState(UnitAnimationActionHandle handle, AnimationState state)
	{
		Data data = (Data)handle.ActionData;
		if (data != null)
		{
			data.CurrentAnimationState = state;
		}
		handle.ActionData = data;
		StartAction(handle);
	}

	private void StartAction(UnitAnimationActionHandle handle)
	{
		Data data = (Data)handle.ActionData;
		if (data == null)
		{
			return;
		}
		AnimationClipWrapper animationClip = GetAnimationClip(handle, data.CurrentAnimationState);
		data.IsForce = false;
		if (animationClip == null)
		{
			handle.Release();
			return;
		}
		handle.StartClip(animationClip, (data.CurrentAnimationState == AnimationState.StepOutCover) ? ClipDurationType.Oneshot : ClipDurationType.Endless);
		if (data.CurrentAnimationState == AnimationState.Idle && handle.ActiveAnimation != null)
		{
			handle.ActiveAnimation.TransitionIn = 0f;
			handle.ActiveAnimation.TransitionOut = 0f;
		}
		if (Game.CombatAnimSpeedUp > 1f && data.CurrentAnimationState != AnimationState.Idle)
		{
			handle.SpeedScale = Game.CombatAnimSpeedUp;
		}
		data.ActionFinished = false;
		data.ActionStarted = true;
		data.Time = handle.GetTime() + animationClip.Length;
		handle.ActionData = data;
	}

	public void PrepareStepOutData(UnitAnimationActionHandle handle)
	{
		handle.ActionData = new Data
		{
			CurrentAnimationState = AnimationState.StepInCover,
			CoverType = handle.Manager.CoverType
		};
	}

	public void PrepareEnteringCoverData(UnitAnimationActionHandle handle)
	{
		handle.ActionData = new Data
		{
			CurrentAnimationState = AnimationState.ForceEnteringTheCover,
			CoverType = handle.Manager.CoverType,
			IsForce = true
		};
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		handle.Manager.BlockAttackAnimation = false;
		base.OnFinish(handle);
	}

	private AnimationClipWrapper GetAnimationClip(UnitAnimationActionHandle handle, AnimationState animState)
	{
		bool isOffhand = false;
		if (handle.Manager.View.Data?.Commands?.Current is UnitUseAbility unitUseAbility && handle.Manager.View.Data.GetBodyOptional() != null)
		{
			isOffhand = unitUseAbility.Ability.Caster?.GetOptional<UnitPartMechadendrites>() == null && unitUseAbility.Ability.Weapon != handle.Manager.View.Data.GetBodyOptional()?.PrimaryHand?.MaybeWeapon;
		}
		WeaponStyleSettings weaponStyleSettings = ((!isOffhand) ? WeaponStyleOverrides.FirstItem((WeaponStyleSettings x) => x.WeaponAnimationStyle == handle.Manager.ActiveMainHandWeaponStyle && x.IsOffHand == isOffhand) : WeaponStyleOverrides.FirstItem((WeaponStyleSettings x) => x.WeaponAnimationStyle == handle.Manager.ActiveOffHandWeaponStyle && x.IsOffHand == isOffhand));
		if (weaponStyleSettings == null)
		{
			weaponStyleSettings = WeaponStyleOverrides.FirstItem((WeaponStyleSettings x) => x.WeaponAnimationStyle == handle.Manager.ActiveMainHandWeaponStyle);
		}
		Data data = (Data)handle.ActionData;
		if (handle.Manager.CoverType == LosCalculations.CoverType.Full && handle.Manager.AbilityIsSpell)
		{
			AnimationClipWrapper animationClipWrapper = null;
			switch (animState)
			{
			case AnimationState.StepOutCover:
				switch (handle.Manager.StepOutDirectionAnimationType)
				{
				case StepOutDirectionAnimationType.Left:
					animationClipWrapper = LeftStepFullCoverEnteringForCast;
					break;
				case StepOutDirectionAnimationType.Right:
					animationClipWrapper = RightStepFullCoverEnteringForCast;
					break;
				}
				break;
			case AnimationState.StepInCover:
				switch (handle.Manager.StepOutDirectionAnimationType)
				{
				case StepOutDirectionAnimationType.Left:
					animationClipWrapper = LeftStepFullCoverExitingForCast;
					break;
				case StepOutDirectionAnimationType.Right:
					animationClipWrapper = RightStepFullCoverExitingForCast;
					break;
				}
				break;
			}
			if (animationClipWrapper != null)
			{
				return animationClipWrapper;
			}
		}
		if (weaponStyleSettings != null)
		{
			return handle.Manager.CoverType switch
			{
				LosCalculations.CoverType.Full => animState switch
				{
					AnimationState.ForceEnteringTheCover => (data == null || !data.IsForce) ? (weaponStyleSettings.FullCoverEntering ? weaponStyleSettings.FullCoverEntering : FullCoverEntering) : ((FullCoverInside != null) ? FullCoverInside : (weaponStyleSettings.FullCoverEntering ? weaponStyleSettings.FullCoverEntering : FullCoverEntering)), 
					AnimationState.Idle => (weaponStyleSettings.FullCoverIdle != null) ? weaponStyleSettings.FullCoverIdle : FullCoverIdle, 
					AnimationState.ForceExitingTheCover => (data == null || !data.IsForce) ? (weaponStyleSettings.FullCoverExiting ? weaponStyleSettings.FullCoverExiting : FullCoverExiting) : ((FullCoverOutside != null) ? FullCoverOutside : (weaponStyleSettings.FullCoverExiting ? weaponStyleSettings.FullCoverExiting : FullCoverExiting)), 
					AnimationState.StepOutCover => handle.Manager.StepOutDirectionAnimationType switch
					{
						StepOutDirectionAnimationType.Left => (weaponStyleSettings.LeftStepFullCoverEntering != null) ? weaponStyleSettings.LeftStepFullCoverEntering : LeftStepFullCoverEntering, 
						StepOutDirectionAnimationType.Right => (weaponStyleSettings.RightStepFullCoverEntering != null) ? weaponStyleSettings.RightStepFullCoverEntering : RightStepFullCoverEntering, 
						StepOutDirectionAnimationType.None => (weaponStyleSettings.FullCoverEntering != null) ? weaponStyleSettings.FullCoverEntering : FullCoverEntering, 
						_ => null, 
					}, 
					AnimationState.StepInCover => handle.Manager.StepOutDirectionAnimationType switch
					{
						StepOutDirectionAnimationType.Left => (weaponStyleSettings.LeftStepFullCoverExiting != null) ? weaponStyleSettings.LeftStepFullCoverExiting : LeftStepFullCoverExiting, 
						StepOutDirectionAnimationType.Right => (weaponStyleSettings.RightStepFullCoverExiting != null) ? weaponStyleSettings.RightStepFullCoverExiting : RightStepFullCoverExiting, 
						StepOutDirectionAnimationType.None => (weaponStyleSettings.FullCoverExiting != null) ? weaponStyleSettings.FullCoverExiting : FullCoverExiting, 
						_ => null, 
					}, 
					_ => null, 
				}, 
				LosCalculations.CoverType.Half => animState switch
				{
					AnimationState.StepOutCover => (weaponStyleSettings.HalfCoverEntering != null) ? weaponStyleSettings.HalfCoverEntering : HalfCoverEntering, 
					AnimationState.ForceEnteringTheCover => (weaponStyleSettings.HalfCoverEntering != null) ? weaponStyleSettings.HalfCoverEntering : HalfCoverEntering, 
					AnimationState.Idle => (weaponStyleSettings.HalfCoverIdle != null) ? weaponStyleSettings.HalfCoverIdle : HalfCoverIdle, 
					AnimationState.StepInCover => (weaponStyleSettings.HalfCoverExiting != null) ? weaponStyleSettings.HalfCoverExiting : HalfCoverExiting, 
					AnimationState.ForceExitingTheCover => (weaponStyleSettings.HalfCoverExiting != null) ? weaponStyleSettings.HalfCoverExiting : HalfCoverExiting, 
					_ => null, 
				}, 
				_ => null, 
			};
		}
		return handle.Manager.CoverType switch
		{
			LosCalculations.CoverType.Full => animState switch
			{
				AnimationState.ForceEnteringTheCover => FullCoverEntering, 
				AnimationState.Idle => FullCoverIdle, 
				AnimationState.ForceExitingTheCover => FullCoverExiting, 
				AnimationState.StepOutCover => handle.Manager.StepOutDirectionAnimationType switch
				{
					StepOutDirectionAnimationType.Left => LeftStepFullCoverEntering, 
					StepOutDirectionAnimationType.Right => RightStepFullCoverEntering, 
					StepOutDirectionAnimationType.None => FullCoverEntering, 
					_ => null, 
				}, 
				AnimationState.StepInCover => handle.Manager.StepOutDirectionAnimationType switch
				{
					StepOutDirectionAnimationType.Left => LeftStepFullCoverExiting, 
					StepOutDirectionAnimationType.Right => RightStepFullCoverExiting, 
					StepOutDirectionAnimationType.None => FullCoverExiting, 
					_ => null, 
				}, 
				_ => null, 
			}, 
			LosCalculations.CoverType.Half => animState switch
			{
				AnimationState.StepOutCover => HalfCoverEntering, 
				AnimationState.ForceEnteringTheCover => HalfCoverEntering, 
				AnimationState.Idle => HalfCoverIdle, 
				AnimationState.StepInCover => HalfCoverExiting, 
				AnimationState.ForceExitingTheCover => HalfCoverExiting, 
				_ => null, 
			}, 
			_ => null, 
		};
	}
}
