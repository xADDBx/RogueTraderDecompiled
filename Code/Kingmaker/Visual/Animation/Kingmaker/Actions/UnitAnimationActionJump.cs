using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationActionJump", menuName = "Animation Manager/Actions/Unit Animation Jump")]
public class UnitAnimationActionJump : UnitAnimationAction
{
	[Serializable]
	public class JumpVariantSettings
	{
		public WeaponAnimationStyle Style;

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

		public AnimationClipWrapper GetAnimation(State state)
		{
			return state switch
			{
				State.Fly => m_JumpFly, 
				State.In => m_JumpIn, 
				State.Out => m_JumpOut, 
				_ => throw new ArgumentOutOfRangeException("state", state, null), 
			};
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
		ActionData actionData = new ActionData
		{
			State = State.In
		};
		handle.ActionData = actionData;
		JumpVariantSettings jumpVariantSettings = WeaponStyleSettings.FirstOrDefault((JumpVariantSettings i) => i.Style == handle.AttackWeaponStyle);
		handle.StartClip(jumpVariantSettings?.GetAnimation(State.In) ?? m_JumpIn, ClipDurationType.Oneshot);
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (!(handle.ActionData is ActionData actionData))
		{
			handle.Release();
			return;
		}
		JumpVariantSettings jumpVariantSettings = WeaponStyleSettings.FirstOrDefault((JumpVariantSettings i) => i.Style == handle.AttackWeaponStyle);
		if (actionData.State == State.In && handle.GetTime() > m_JumpIn.Length)
		{
			handle.StartClip(jumpVariantSettings?.GetAnimation(State.Fly) ?? m_JumpFly, ClipDurationType.Oneshot);
			actionData.State = State.Fly;
		}
		else if (actionData.State == State.Fly && handle.GetTime() > m_JumpFly.Length + m_JumpIn.Length)
		{
			if (handle.NeedAttackAfterJump)
			{
				handle.Release();
				return;
			}
			handle.StartClip(jumpVariantSettings?.GetAnimation(State.Fly) ?? m_JumpOut, ClipDurationType.Oneshot);
			actionData.State = State.Out;
		}
		else if (actionData.State == State.Out && handle.GetTime() > m_JumpFly.Length + m_JumpIn.Length + m_JumpOut.Length)
		{
			handle.Release();
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public float GetInClipLenght()
	{
		return m_JumpIn?.Length ?? 0f;
	}

	public float GetFlyClipLenght()
	{
		return m_JumpFly?.Length ?? 0f;
	}
}
