using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationDeath", menuName = "Animation Manager/Actions/Unit Death")]
public class UnitAnimationActionDeath : UnitAnimationAction
{
	public enum DeathType
	{
		DeathBase,
		DeathInProneBack,
		DeathInProneForward,
		DeathConvultion,
		DeathBehead,
		DeathMashineGun,
		DeathCutTheThroat,
		DeathToxic,
		DeathPsy,
		DeathGuts
	}

	[Serializable]
	public class AnimationContainer
	{
		[AssetPicker("")]
		[SerializeField]
		public AnimationClipWrapper DeathAnimation;

		[AssetPicker("")]
		[SerializeField]
		public AnimationClipWrapper LyingAnimation;
	}

	private class ActionData
	{
		public bool FallingFinished;

		public bool HasLyingClip;

		public float FallingTime;

		public DeathType DeathType;
	}

	private DeathType m_deathType;

	[SerializeField]
	public AnimationContainer DeathBase;

	[SerializeField]
	public AnimationContainer DeathInProneBack;

	[SerializeField]
	public AnimationContainer DeathInProneForward;

	[SerializeField]
	public AnimationContainer DeathConvultion;

	[SerializeField]
	public AnimationContainer DeathBehead;

	[SerializeField]
	public AnimationContainer DeathMashineGun;

	[SerializeField]
	public AnimationContainer DeathCutTheThroat;

	[SerializeField]
	public AnimationContainer DeathToxic;

	[SerializeField]
	public AnimationContainer DeathPsy;

	[SerializeField]
	public AnimationContainer DeathGuts;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers { get; }

	public override UnitAnimationType Type => UnitAnimationType.Death;

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		m_deathType = handle.DeathType;
		bool isDead = handle.Manager.IsDead;
		ActionData actionData = new ActionData();
		actionData.DeathType = m_deathType;
		handle.ActionData = actionData;
		handle.HasCrossfadePriority = true;
		AnimationClipWrapper actionAnimation = GetActionAnimation();
		if ((bool)actionAnimation)
		{
			handle.StartClip(actionAnimation, ClipDurationType.Oneshot);
			if (isDead)
			{
				handle.ActiveAnimation.ChangeTransitionTime(0.2f);
			}
			actionData.HasLyingClip = isDead;
			actionData.FallingTime = (actionAnimation ? actionAnimation.Length : 0f);
		}
		else
		{
			UberDebug.LogError((UnityEngine.Object)(((object)actionAnimation) ?? ((object)this)), "Animation clip is not set for object {0}", ((object)actionAnimation) ?? ((object)this));
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		ActionData actionData = (ActionData)handle.ActionData;
		m_deathType = actionData.DeathType;
		if (actionData.HasLyingClip && !actionData.FallingFinished)
		{
			actionData.FallingFinished = true;
			AnimationClipWrapper lyingAnimation = GetLyingAnimation();
			handle.StartClip(lyingAnimation, ClipDurationType.Endless);
			handle.ActiveAnimation.TransitionIn = 0.2f;
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
			m_deathType = actionData.DeathType;
			AnimationClipWrapper lyingAnimation = GetLyingAnimation();
			if ((bool)lyingAnimation)
			{
				handle.StartClip(lyingAnimation, ClipDurationType.Endless);
				return;
			}
			handle.StartClip(GetActionAnimation(), ClipDurationType.Endless);
		}
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
			handle.UpdateInternal(10f);
		}
		actionData.FallingFinished = true;
	}

	private AnimationClipWrapper GetActionAnimation()
	{
		return m_deathType switch
		{
			DeathType.DeathBase => DeathBase.DeathAnimation, 
			DeathType.DeathInProneBack => DeathInProneBack.DeathAnimation, 
			DeathType.DeathInProneForward => DeathInProneForward.DeathAnimation, 
			DeathType.DeathConvultion => DeathConvultion.DeathAnimation, 
			DeathType.DeathBehead => DeathBehead.DeathAnimation, 
			DeathType.DeathMashineGun => DeathMashineGun.DeathAnimation, 
			DeathType.DeathCutTheThroat => DeathCutTheThroat.DeathAnimation, 
			DeathType.DeathToxic => DeathToxic.DeathAnimation, 
			DeathType.DeathPsy => DeathPsy.DeathAnimation, 
			DeathType.DeathGuts => DeathGuts.DeathAnimation, 
			_ => DeathBase.DeathAnimation, 
		};
	}

	private AnimationClipWrapper GetLyingAnimation()
	{
		return m_deathType switch
		{
			DeathType.DeathBase => DeathBase.LyingAnimation, 
			DeathType.DeathInProneBack => DeathInProneBack.LyingAnimation, 
			DeathType.DeathInProneForward => DeathInProneForward.LyingAnimation, 
			DeathType.DeathConvultion => DeathConvultion.LyingAnimation, 
			DeathType.DeathBehead => DeathBehead.LyingAnimation, 
			DeathType.DeathMashineGun => DeathMashineGun.LyingAnimation, 
			DeathType.DeathCutTheThroat => DeathCutTheThroat.LyingAnimation, 
			DeathType.DeathToxic => DeathToxic.LyingAnimation, 
			DeathType.DeathPsy => DeathPsy.LyingAnimation, 
			DeathType.DeathGuts => DeathGuts.LyingAnimation, 
			_ => DeathBase.LyingAnimation, 
		};
	}
}
