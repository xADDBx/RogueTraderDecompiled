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
		SideStepOut,
		Idle,
		SideStepIn,
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

		public bool FirstAnimationStarted;

		public bool SideStepAbilityIsOffHand;
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

	[Serializable]
	private class OffHandStyleSettings
	{
		public WeaponAnimationStyle Style;

		public bool IsMainHand;

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

		private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				HashSet<AnimationClipWrapper> hashSet = m_ClipWrappersHashSet;
				if (hashSet == null)
				{
					HashSet<AnimationClipWrapper> obj = new HashSet<AnimationClipWrapper> { HalfCoverEntering, HalfCoverIdle, HalfCoverExiting, FullCoverEntering, FullCoverIdle, FullCoverExiting, LeftStepFullCoverEntering, LeftStepFullCoverExiting, RightStepFullCoverEntering, RightStepFullCoverExiting };
					HashSet<AnimationClipWrapper> hashSet2 = obj;
					m_ClipWrappersHashSet = obj;
					hashSet = hashSet2;
				}
				return hashSet;
			}
		}
	}

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

	[SerializeField]
	private List<OffHandStyleSettings> m_InactiveHandWeaponStyleOverrides = new List<OffHandStyleSettings>();

	[SerializeField]
	private AvatarMask m_OffHandMask;

	[SerializeField]
	private AvatarMask m_MainHandMask;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

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
			foreach (OffHandStyleSettings inactiveHandWeaponStyleOverride in m_InactiveHandWeaponStyleOverrides)
			{
				m_ClipWrappersHashSet.AddRange(inactiveHandWeaponStyleOverride.ClipWrappers);
			}
			return m_ClipWrappersHashSet;
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.HasCrossfadePriority = true;
		handle.SkipFirstTick = false;
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
			if (handle.Manager.HitAnimationIsActive)
			{
				data.ActionFinished = true;
				data.ActionStarted = false;
				ChangeStateAndStartAction(handle, AnimationState.Idle);
				handle.Manager.HitAnimationIsActive = false;
				break;
			}
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (handle.GetTime() < data.Time)
			{
				return;
			}
			data.ActionFinished = true;
			data.ActionStarted = false;
			ChangeStateAndStartAction(handle, AnimationState.Idle);
			break;
		case AnimationState.SideStepOut:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (handle.GetTime() < data.Time)
			{
				return;
			}
			data.ActionFinished = true;
			data.ActionStarted = false;
			handle.Manager.StepOutDirectionAnimationType = StepOutDirectionAnimationType.None;
			handle.Manager.AbilityIsSpell = false;
			handle.Release();
			break;
		case AnimationState.Idle:
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (handle.GetTime() < data.Time)
			{
				return;
			}
			handle.Manager.StepOutDirectionAnimationType = StepOutDirectionAnimationType.None;
			handle.Manager.AbilityIsSpell = false;
			break;
		case AnimationState.SideStepIn:
			handle.Manager.BlockAttackAnimation = true;
			if (data.CoverType == LosCalculations.CoverType.Full && handle.Manager.NeedStepOut && data.CurrentAnimationState == AnimationState.SideStepIn && Math.Abs(handle.Manager.Orientation - handle.Manager.UseAbilityDirection) > 10f)
			{
				return;
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
			if (handle.GetTime() < data.Time)
			{
				return;
			}
			handle.Manager.BlockAttackAnimation = false;
			if (handle.Manager.NeedStepOut)
			{
				return;
			}
			data.ActionFinished = true;
			ChangeStateAndStartAction(handle, AnimationState.SideStepOut);
			break;
		case AnimationState.ForceExitingTheCover:
			handle.Manager.BlockAttackAnimation = true;
			if (!data.ActionStarted)
			{
				StartAction(handle);
			}
			if (handle.GetTime() < data.Time)
			{
				return;
			}
			data.WaitForceExit = true;
			data.ActionFinished = true;
			data.ActionStarted = false;
			handle.Manager.BlockAttackAnimation = false;
			handle.Release();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		handle.Manager.HitAnimationIsActive = false;
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		Data data = (Data)handle.ActionData;
		if (data != null && !handle.IsReleased)
		{
			data.CoverType = handle.Manager.CoverType;
			if (data.CoverType == LosCalculations.CoverType.Half && handle.Manager.InCover && data.CurrentAnimationState != AnimationState.Idle && data.ActionFinished)
			{
				ChangeStateAndStartAction(handle, AnimationState.ForceEnteringTheCover);
			}
			if (!handle.Manager.InCover && data.CurrentAnimationState == AnimationState.Idle)
			{
				ChangeStateAndStartAction(handle, AnimationState.ForceExitingTheCover);
			}
			UpdateAnimations(handle);
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if ((Data)handle.ActionData != null && handle.IsInterrupted && handle.ActiveAnimation != null)
		{
			handle.ActiveAnimation.TransitionOut = TransitionOut;
		}
	}

	public void ForceExit(UnitAnimationActionHandle handle)
	{
		Data data = (Data)handle.ActionData;
		if (data != null)
		{
			data.IsForce = true;
		}
		ChangeStateAndStartAction(handle, AnimationState.ForceExitingTheCover);
	}

	public bool IsCoverForceExitingFinished(UnitAnimationActionHandle handle)
	{
		return ((Data)handle.ActionData)?.WaitForceExit ?? false;
	}

	private void ChangeStateAndStartAction(UnitAnimationActionHandle handle, AnimationState state)
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
		(AnimationClipWrapper, AnimationClipWrapper) animationClips = GetAnimationClips(handle, data);
		AnimationClipWrapper item = animationClips.Item1;
		AnimationClipWrapper item2 = animationClips.Item2;
		data.IsForce = false;
		if (item == null)
		{
			handle.Release();
			return;
		}
		AnimationState currentAnimationState = data.CurrentAnimationState;
		bool flag = currentAnimationState == AnimationState.ForceExitingTheCover || currentAnimationState == AnimationState.SideStepOut;
		ClipDurationType clipDurationType = (flag ? ClipDurationType.Oneshot : ClipDurationType.Endless);
		if (item2 != null)
		{
			handle.Manager.AddAnimationClip(handle, item, AvatarMasks, UseEmptyAvatarMask, IsAdditive, clipDurationType, new AnimationComposition(handle));
			AvatarMask avatarMask = (data.SideStepAbilityIsOffHand ? m_MainHandMask : m_OffHandMask);
			handle.Manager.AddClipToComposition(handle, item2, avatarMask, isAdditive: false);
		}
		else
		{
			handle.StartClip(item, clipDurationType);
		}
		if (handle.ActiveAnimation != null)
		{
			if (handle.Manager.HitAnimationIsActive)
			{
				handle.ActiveAnimation.TransitionIn = UnitAnimationActionHit.CrossfadeToCoverTime;
			}
			else if (data.FirstAnimationStarted)
			{
				handle.ActiveAnimation.TransitionIn = 0f;
			}
			if (!flag)
			{
				handle.ActiveAnimation.ChangeTransitionTime(0f);
			}
		}
		if (Game.CombatAnimSpeedUp > 1f && data.CurrentAnimationState != AnimationState.Idle)
		{
			handle.SpeedScale = Game.CombatAnimSpeedUp;
		}
		data.ActionFinished = false;
		data.ActionStarted = true;
		data.Time = handle.GetTime() + item.Length;
		data.FirstAnimationStarted = true;
		handle.ActionData = data;
	}

	public void PrepareSideStepData(UnitAnimationActionHandle handle)
	{
		handle.ActionData = new Data
		{
			CurrentAnimationState = AnimationState.SideStepIn,
			CoverType = handle.Manager.CoverType,
			SideStepAbilityIsOffHand = (handle.Manager.View.Data?.Commands?.Current is UnitUseAbility unitUseAbility && unitUseAbility.Ability.Caster?.GetOptional<UnitPartMechadendrites>() == null && unitUseAbility.Ability.Weapon != handle.Manager.View.Data.GetBodyOptional()?.PrimaryHand?.MaybeWeapon)
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

	private (AnimationClipWrapper, AnimationClipWrapper) GetAnimationClips(UnitAnimationActionHandle handle, Data data)
	{
		if (handle.Manager.CoverType == LosCalculations.CoverType.Full && handle.Manager.AbilityIsSpell)
		{
			AnimationClipWrapper animationClipWrapper = null;
			switch (data.CurrentAnimationState)
			{
			case AnimationState.SideStepOut:
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
			case AnimationState.SideStepIn:
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
				return (animationClipWrapper, null);
			}
		}
		return (GetActingHandAnimationClip(handle, data), GetInactiveHandAnimationClip(handle, data));
	}

	private AnimationClipWrapper GetActingHandAnimationClip(UnitAnimationActionHandle handle, Data data)
	{
		bool isSideStepOffhand = data.SideStepAbilityIsOffHand;
		WeaponAnimationStyle weaponStyle = (isSideStepOffhand ? handle.Manager.ActiveOffHandWeaponStyle : handle.Manager.ActiveMainHandWeaponStyle);
		WeaponStyleSettings weaponStyleSettings = WeaponStyleOverrides.FirstItem((WeaponStyleSettings x) => x.WeaponAnimationStyle == weaponStyle && x.IsOffHand == isSideStepOffhand) ?? WeaponStyleOverrides.FirstItem((WeaponStyleSettings x) => x.WeaponAnimationStyle == handle.Manager.ActiveMainHandWeaponStyle);
		AnimationState currentAnimationState = data.CurrentAnimationState;
		if (weaponStyleSettings != null)
		{
			return handle.Manager.CoverType switch
			{
				LosCalculations.CoverType.Full => currentAnimationState switch
				{
					AnimationState.ForceEnteringTheCover => (!data.IsForce) ? (weaponStyleSettings.FullCoverEntering ? weaponStyleSettings.FullCoverEntering : FullCoverEntering) : ((FullCoverInside != null) ? FullCoverInside : (weaponStyleSettings.FullCoverEntering ? weaponStyleSettings.FullCoverEntering : FullCoverEntering)), 
					AnimationState.Idle => (weaponStyleSettings.FullCoverIdle != null) ? weaponStyleSettings.FullCoverIdle : FullCoverIdle, 
					AnimationState.ForceExitingTheCover => (!data.IsForce) ? (weaponStyleSettings.FullCoverExiting ? weaponStyleSettings.FullCoverExiting : FullCoverExiting) : ((FullCoverOutside != null) ? FullCoverOutside : (weaponStyleSettings.FullCoverExiting ? weaponStyleSettings.FullCoverExiting : FullCoverExiting)), 
					AnimationState.SideStepOut => handle.Manager.StepOutDirectionAnimationType switch
					{
						StepOutDirectionAnimationType.Left => (weaponStyleSettings.LeftStepFullCoverEntering != null) ? weaponStyleSettings.LeftStepFullCoverEntering : LeftStepFullCoverEntering, 
						StepOutDirectionAnimationType.Right => (weaponStyleSettings.RightStepFullCoverEntering != null) ? weaponStyleSettings.RightStepFullCoverEntering : RightStepFullCoverEntering, 
						StepOutDirectionAnimationType.None => (weaponStyleSettings.FullCoverEntering != null) ? weaponStyleSettings.FullCoverEntering : FullCoverEntering, 
						_ => null, 
					}, 
					AnimationState.SideStepIn => handle.Manager.StepOutDirectionAnimationType switch
					{
						StepOutDirectionAnimationType.Left => (weaponStyleSettings.LeftStepFullCoverExiting != null) ? weaponStyleSettings.LeftStepFullCoverExiting : LeftStepFullCoverExiting, 
						StepOutDirectionAnimationType.Right => (weaponStyleSettings.RightStepFullCoverExiting != null) ? weaponStyleSettings.RightStepFullCoverExiting : RightStepFullCoverExiting, 
						StepOutDirectionAnimationType.None => (weaponStyleSettings.FullCoverExiting != null) ? weaponStyleSettings.FullCoverExiting : FullCoverExiting, 
						_ => null, 
					}, 
					_ => null, 
				}, 
				LosCalculations.CoverType.Half => currentAnimationState switch
				{
					AnimationState.SideStepOut => (weaponStyleSettings.HalfCoverEntering != null) ? weaponStyleSettings.HalfCoverEntering : HalfCoverEntering, 
					AnimationState.ForceEnteringTheCover => (weaponStyleSettings.HalfCoverEntering != null) ? weaponStyleSettings.HalfCoverEntering : HalfCoverEntering, 
					AnimationState.Idle => (weaponStyleSettings.HalfCoverIdle != null) ? weaponStyleSettings.HalfCoverIdle : HalfCoverIdle, 
					AnimationState.SideStepIn => (weaponStyleSettings.HalfCoverExiting != null) ? weaponStyleSettings.HalfCoverExiting : HalfCoverExiting, 
					AnimationState.ForceExitingTheCover => (weaponStyleSettings.HalfCoverExiting != null) ? weaponStyleSettings.HalfCoverExiting : HalfCoverExiting, 
					_ => null, 
				}, 
				_ => null, 
			};
		}
		return handle.Manager.CoverType switch
		{
			LosCalculations.CoverType.Full => currentAnimationState switch
			{
				AnimationState.ForceEnteringTheCover => FullCoverEntering, 
				AnimationState.SideStepOut => handle.Manager.StepOutDirectionAnimationType switch
				{
					StepOutDirectionAnimationType.Left => LeftStepFullCoverEntering, 
					StepOutDirectionAnimationType.Right => RightStepFullCoverEntering, 
					StepOutDirectionAnimationType.None => FullCoverEntering, 
					_ => null, 
				}, 
				AnimationState.Idle => FullCoverIdle, 
				AnimationState.SideStepIn => handle.Manager.StepOutDirectionAnimationType switch
				{
					StepOutDirectionAnimationType.Left => LeftStepFullCoverExiting, 
					StepOutDirectionAnimationType.Right => RightStepFullCoverExiting, 
					StepOutDirectionAnimationType.None => FullCoverExiting, 
					_ => null, 
				}, 
				AnimationState.ForceExitingTheCover => FullCoverExiting, 
				_ => null, 
			}, 
			LosCalculations.CoverType.Half => currentAnimationState switch
			{
				AnimationState.SideStepOut => HalfCoverEntering, 
				AnimationState.ForceEnteringTheCover => HalfCoverEntering, 
				AnimationState.Idle => HalfCoverIdle, 
				AnimationState.SideStepIn => HalfCoverExiting, 
				AnimationState.ForceExitingTheCover => HalfCoverExiting, 
				_ => null, 
			}, 
			_ => null, 
		};
	}

	private AnimationClipWrapper GetInactiveHandAnimationClip(UnitAnimationActionHandle handle, Data data)
	{
		bool isSideStepOffhand = data.SideStepAbilityIsOffHand;
		WeaponAnimationStyle weaponStyle = (isSideStepOffhand ? handle.Manager.ActiveMainHandWeaponStyle : handle.Manager.ActiveOffHandWeaponStyle);
		OffHandStyleSettings offHandStyleSettings = m_InactiveHandWeaponStyleOverrides.FirstItem((OffHandStyleSettings x) => x.Style == weaponStyle && x.IsMainHand == isSideStepOffhand);
		if (offHandStyleSettings == null)
		{
			return null;
		}
		AnimationState currentAnimationState = data.CurrentAnimationState;
		return handle.Manager.CoverType switch
		{
			LosCalculations.CoverType.Full => currentAnimationState switch
			{
				AnimationState.ForceEnteringTheCover => offHandStyleSettings.FullCoverEntering, 
				AnimationState.Idle => offHandStyleSettings.FullCoverIdle, 
				AnimationState.ForceExitingTheCover => offHandStyleSettings.FullCoverExiting, 
				AnimationState.SideStepOut => handle.Manager.StepOutDirectionAnimationType switch
				{
					StepOutDirectionAnimationType.Left => offHandStyleSettings.LeftStepFullCoverEntering, 
					StepOutDirectionAnimationType.Right => offHandStyleSettings.RightStepFullCoverEntering, 
					StepOutDirectionAnimationType.None => offHandStyleSettings.FullCoverEntering, 
					_ => null, 
				}, 
				AnimationState.SideStepIn => handle.Manager.StepOutDirectionAnimationType switch
				{
					StepOutDirectionAnimationType.Left => offHandStyleSettings.LeftStepFullCoverExiting, 
					StepOutDirectionAnimationType.Right => offHandStyleSettings.RightStepFullCoverExiting, 
					StepOutDirectionAnimationType.None => offHandStyleSettings.FullCoverExiting, 
					_ => null, 
				}, 
				_ => null, 
			}, 
			LosCalculations.CoverType.Half => currentAnimationState switch
			{
				AnimationState.SideStepOut => offHandStyleSettings.HalfCoverEntering, 
				AnimationState.ForceEnteringTheCover => offHandStyleSettings.HalfCoverEntering, 
				AnimationState.Idle => offHandStyleSettings.HalfCoverIdle, 
				AnimationState.SideStepIn => offHandStyleSettings.HalfCoverExiting, 
				AnimationState.ForceExitingTheCover => offHandStyleSettings.HalfCoverExiting, 
				_ => null, 
			}, 
			_ => null, 
		};
	}
}
