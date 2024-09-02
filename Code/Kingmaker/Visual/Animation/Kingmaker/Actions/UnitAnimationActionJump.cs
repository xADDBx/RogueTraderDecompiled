using System;
using System.Collections.Generic;
using Kingmaker.Controllers;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationActionJump", menuName = "Animation Manager/Actions/Unit Animation Jump")]
public class UnitAnimationActionJump : UnitAnimationAction
{
	[Serializable]
	public class JumpVariantSettings
	{
		public WeaponAnimationStyle Style;

		public bool IsOffHand;

		[AssetPicker("")]
		[SerializeField]
		private AnimationClipWrapper m_JumpIn;

		[AssetPicker("")]
		[SerializeField]
		private AnimationClipWrapper m_JumpOut;

		[AssetPicker("")]
		[SerializeField]
		private AnimationClipWrapper m_JumpFly;

		private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				if (m_ClipWrappersHashSet != null)
				{
					return m_ClipWrappersHashSet;
				}
				m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper> { m_JumpIn, m_JumpOut, m_JumpFly };
				return m_ClipWrappersHashSet;
			}
		}

		public AnimationClipWrapper GetAnimation(State state, UnitAnimationActionHandle handle)
		{
			switch (state)
			{
			case State.Fly:
				return m_JumpFly;
			case State.In:
				return m_JumpIn;
			case State.Out:
				if (!handle.CastInOffhand)
				{
					return m_JumpOut;
				}
				break;
			}
			throw new ArgumentOutOfRangeException("state", state, null);
		}
	}

	public enum State
	{
		Fly,
		In,
		Out
	}

	private class ActionData
	{
		public State State;
	}

	[AssetPicker("")]
	[SerializeField]
	[ValidateNotNull]
	private AnimationClipWrapper m_JumpIn;

	[AssetPicker("")]
	[SerializeField]
	[ValidateNotNull]
	private AnimationClipWrapper m_JumpOut;

	[AssetPicker("")]
	[SerializeField]
	[ValidateNotNull]
	private AnimationClipWrapper m_JumpFly;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public List<JumpVariantSettings> WeaponStyleSettings;

	private static float CrossfadeTime => RealTimeController.SystemStepDurationSeconds;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			if (m_ClipWrappersHashSet != null)
			{
				return m_ClipWrappersHashSet;
			}
			m_ClipWrappersHashSet = new HashSet<AnimationClipWrapper> { m_JumpIn, m_JumpOut, m_JumpFly };
			foreach (JumpVariantSettings weaponStyleSetting in WeaponStyleSettings)
			{
				m_ClipWrappersHashSet.AddRange(weaponStyleSetting.ClipWrappers);
			}
			return m_ClipWrappersHashSet;
		}
	}

	public override UnitAnimationType Type => UnitAnimationType.Jump;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.HasCrossfadePriority = true;
		handle.SkipFirstTick = false;
		ActionData actionData = new ActionData
		{
			State = State.In
		};
		handle.ActionData = actionData;
		handle.StartClip(GetAnimation(State.In, handle), ClipDurationType.Oneshot);
		PFLog.Actions.Log($"Crossfade = {CrossfadeTime}");
		handle.ActiveAnimation.ChangeTransitionTime(CrossfadeTime);
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (!(handle.ActionData is ActionData { State: var state } actionData))
		{
			base.OnTransitionOutStarted(handle);
			return;
		}
		switch (state)
		{
		case State.In:
			handle.StartClip(GetAnimation(State.Fly, handle), ClipDurationType.Oneshot);
			handle.ActiveAnimation.TransitionIn = CrossfadeTime;
			if (!handle.NeedAttackAfterJump)
			{
				handle.ActiveAnimation.ChangeTransitionTime(CrossfadeTime);
			}
			actionData.State = State.Fly;
			return;
		case State.Fly:
			if (!handle.NeedAttackAfterJump)
			{
				handle.StartClip(GetAnimation(State.Out, handle), ClipDurationType.Oneshot);
				handle.ActiveAnimation.TransitionIn = CrossfadeTime;
				actionData.State = State.Out;
				return;
			}
			break;
		}
		handle.Release();
	}

	private AnimationClipWrapper GetAnimation(State state, UnitAnimationActionHandle handle)
	{
		bool isOffHand = false;
		WeaponAnimationStyle weaponStyle = handle.Manager.ActiveMainHandWeaponStyle;
		if (weaponStyle == WeaponAnimationStyle.None)
		{
			isOffHand = true;
			weaponStyle = handle.Manager.ActiveOffHandWeaponStyle;
		}
		AnimationClipWrapper animationClipWrapper = WeaponStyleSettings.FirstOrDefault((JumpVariantSettings i) => i.Style == weaponStyle && isOffHand == i.IsOffHand)?.GetAnimation(state, handle);
		if (animationClipWrapper != null)
		{
			return animationClipWrapper;
		}
		switch (state)
		{
		case State.Fly:
			return m_JumpFly;
		case State.In:
			return m_JumpIn;
		case State.Out:
			if (!handle.CastInOffhand)
			{
				return m_JumpOut;
			}
			break;
		}
		throw new ArgumentOutOfRangeException("state", state, null);
	}

	public float GetInClipLenght()
	{
		return m_JumpIn.Or(null)?.Length ?? 0f;
	}

	public float GetFlyClipLenght()
	{
		return m_JumpFly.Or(null)?.Length ?? 0f;
	}
}
