using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationDodgeJumpAside", menuName = "Animation Manager/Actions/Unit Dodge Jump Aside")]
public class UnitAnimationActionDodgeJumpAside : UnitAnimationAction
{
	private class ActionData
	{
		public WeaponStyleSettings Settings;
	}

	[Serializable]
	public class WeaponStyleSettings
	{
		public float Distance;

		[AssetPicker("")]
		[ValidateNotNull]
		public AnimationClipWrapper Clip;

		public IEnumerable<AnimationClipWrapper> Clips
		{
			get
			{
				yield return Clip;
			}
		}
	}

	[SerializeField]
	private WeaponStyleSettings[] m_Settings = new WeaponStyleSettings[1];

	public override UnitAnimationType Type => UnitAnimationType.JumpAsideDodge;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => m_Settings.SelectMany((WeaponStyleSettings i) => i.Clips);

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		float dodgeDistance = handle.Manager.DodgeDistance;
		if (dodgeDistance <= 0f)
		{
			handle.Release();
			return;
		}
		WeaponStyleSettings variant = GetVariant(dodgeDistance);
		if (variant != null)
		{
			ActionData actionData2 = (ActionData)(handle.ActionData = new ActionData());
			actionData2.Settings = variant;
			if ((bool)variant.Clip && (bool)variant.Clip.AnimationClip)
			{
				SafeStart(handle, variant.Clip, ClipDurationType.Oneshot);
			}
			else
			{
				handle.Release();
			}
		}
	}

	private static bool SafeStart(UnitAnimationActionHandle handle, AnimationClipWrapper clip, ClipDurationType duration)
	{
		if ((bool)clip && (bool)clip.AnimationClip)
		{
			handle.StartClip(clip, duration);
			if (!(handle.ActionData is ActionData actionData))
			{
				handle.Release();
				return true;
			}
			float num = handle.Manager.DodgeDistance / UnitJumpAsideDodgeParams.Speed;
			float speedScale = actionData.Settings.Clip.Length / handle.Manager.DefaultMixerSpeed / num;
			handle.SpeedScale = speedScale;
			return true;
		}
		handle.Release();
		return false;
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (!(handle.ActionData is ActionData actionData))
		{
			handle.Release();
		}
		else if (handle.Manager.DodgeDistance > 0.01f)
		{
			float num = handle.Manager.DodgeDistance / UnitJumpAsideDodgeParams.Speed;
			float speedScale = actionData.Settings.Clip.Length / handle.Manager.DefaultMixerSpeed / num;
			handle.SpeedScale = speedScale;
		}
		else
		{
			handle.Release();
		}
	}

	private WeaponStyleSettings GetVariant(float distance)
	{
		return m_Settings.Aggregate((WeaponStyleSettings res, WeaponStyleSettings next) => (!(Mathf.Abs(next.Distance - distance) < Mathf.Abs(res.Distance - distance))) ? res : next);
	}

	public override void OnFinish(UnitAnimationActionHandle handle)
	{
		base.OnFinish(handle);
		handle.SpeedScale = 1f;
	}
}
