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
public class UnitAnimationActionJump : UnitAnimationAction, UnitAnimationActionJump.IJumpAnimationProvider
{
	public interface IJumpAnimationProvider
	{
		AnimationClipWrapper JumpIn { get; }

		AnimationClipWrapper JumpOut { get; }

		AnimationClipWrapper JumpFly { get; }
	}

	[Serializable]
	public class JumpVariantSettings : IJumpAnimationProvider
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

		public AnimationClipWrapper JumpIn => m_JumpIn;

		public AnimationClipWrapper JumpOut => m_JumpOut;

		public AnimationClipWrapper JumpFly => m_JumpFly;

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

		public bool JumpFinished;
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

	[SerializeField]
	private bool m_LoopedFly;

	[SerializeField]
	private UnitAnimationJumpSubType m_SubType;

	private HashSet<AnimationClipWrapper> m_ClipWrappersHashSet;

	public List<JumpVariantSettings> WeaponStyleSettings;

	public bool LoopedFly => m_LoopedFly;

	public AnimationClipWrapper JumpIn => m_JumpIn;

	public AnimationClipWrapper JumpOut => m_JumpOut;

	public AnimationClipWrapper JumpFly => m_JumpFly;

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

	public override UnitAnimationType Type => m_SubType.ToAnimationType();

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		handle.HasCrossfadePriority = true;
		handle.SkipFirstTick = false;
		handle.SkipFirstTickOnHandle = false;
		handle.CorrectTransitionOutTime = true;
		ActionData ad = (ActionData)(handle.ActionData = new ActionData
		{
			State = State.In
		});
		AnimationClipWrapper animation = GetAnimation(State.In, handle);
		if (animation != null)
		{
			handle.StartClip(animation, ClipDurationType.Oneshot);
			handle.ActiveAnimation.ChangeTransitionTime(CrossfadeTime);
		}
		else
		{
			StartFlyAnimation(handle, ad);
		}
	}

	public void FinishFly(UnitAnimationActionHandle handle)
	{
		if (!m_LoopedFly)
		{
			return;
		}
		if (!(handle.ActionData is ActionData actionData))
		{
			handle.Release();
		}
		else if (!actionData.JumpFinished)
		{
			actionData.JumpFinished = true;
			AnimationBase activeAnimation = handle.ActiveAnimation;
			if (activeAnimation != null)
			{
				activeAnimation.StartTransitionOut();
				activeAnimation.StopEvents();
			}
		}
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
			if (!actionData.JumpFinished)
			{
				StartFlyAnimation(handle, actionData);
				handle.ActiveAnimation.TransitionIn = CrossfadeTime;
				return;
			}
			break;
		case State.Fly:
		{
			AnimationClipWrapper animation = GetAnimation(State.Out, handle);
			if (animation == null)
			{
				handle.Release();
				return;
			}
			handle.StartClip(animation, ClipDurationType.Oneshot);
			handle.ActiveAnimation.TransitionIn = CrossfadeTime;
			actionData.State = State.Out;
			return;
		}
		}
		handle.Release();
	}

	private void StartFlyAnimation(UnitAnimationActionHandle handle, ActionData ad)
	{
		ClipDurationType duration = ((!m_LoopedFly) ? ClipDurationType.Oneshot : ClipDurationType.Endless);
		handle.StartClip(GetAnimation(State.Fly, handle), duration);
		if (GetAnimation(State.Out, handle) != null)
		{
			handle.ActiveAnimation.ChangeTransitionTime(CrossfadeTime);
		}
		ad.State = State.Fly;
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
		JumpVariantSettings jumpVariantSettings = WeaponStyleSettings.FirstOrDefault((JumpVariantSettings i) => i.Style == weaponStyle && isOffHand == i.IsOffHand);
		if (jumpVariantSettings != null)
		{
			AnimationClipWrapper animation = GetAnimation(jumpVariantSettings, state, handle);
			if (animation != null)
			{
				return animation;
			}
		}
		return GetAnimation(this, state, handle);
	}

	private static AnimationClipWrapper GetAnimation(IJumpAnimationProvider provider, State state, UnitAnimationActionHandle handle)
	{
		switch (state)
		{
		case State.Fly:
			return provider.JumpFly;
		case State.In:
			return provider.JumpIn;
		case State.Out:
			if (!handle.CastInOffhand)
			{
				return provider.JumpOut;
			}
			break;
		}
		throw new ArgumentOutOfRangeException("state", state, null);
	}

	public float GetInClipLength()
	{
		return m_JumpIn.Or(null)?.Length ?? 0f;
	}

	public float GetOutClipLength()
	{
		return m_JumpOut.Or(null)?.Length ?? 0f;
	}

	public float GetFlyClipLength()
	{
		return m_JumpFly.Or(null)?.Length ?? 0f;
	}
}
