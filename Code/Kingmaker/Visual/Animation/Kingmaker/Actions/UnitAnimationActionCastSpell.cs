using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Animation;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationCast", menuName = "Animation Manager/Actions/Unit Animation Cast")]
public class UnitAnimationActionCastSpell : UnitAnimationAction
{
	public enum CastAnimationStyle
	{
		Directional,
		Fly,
		Self,
		BattleCry,
		Special,
		Scream,
		Levitate,
		Kick,
		Grenade,
		None,
		Drugs,
		Aquila,
		Mechadendrites,
		Reload,
		Medicine,
		MedicineSelf
	}

	public enum SpecialBehaviourType
	{
		None,
		NoPrecast,
		NoCast
	}

	[Serializable]
	public class AnimationEntry
	{
		[AssetPicker("")]
		public AnimationClipWrapper PrecastStart;

		public float BlendToLoopedTime = 0.1f;

		public readonly float PrecastStartDuration;

		[AssetPicker("")]
		public AnimationClipWrapper PrecastLooped;

		public float BlendToCastTime = 0.1f;

		[AssetPicker("")]
		[ValidateNotNull]
		[ValidateHasActEvent]
		[DrawEventWarning]
		public AnimationClipWrapper CastClip;

		public readonly float PrecastSpeedup = 1f;

		public readonly float CastSpeedup = 1f;

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
				if ((bool)PrecastStart)
				{
					yield return PrecastStart;
				}
				if ((bool)PrecastLooped)
				{
					yield return PrecastLooped;
				}
				if ((bool)CastClip)
				{
					yield return CastClip;
				}
			}
		}
	}

	[Serializable]
	[IKnowWhatImDoing]
	public class AnimationEntryWeaponOverride
	{
		public WeaponAnimationStyle Weapon;

		public AnimationEntry Entry;
	}

	[Serializable]
	public class AnimationStyleEntry
	{
		public CastAnimationStyle Style;

		public AnimationEntry Default;

		public List<AnimationEntryWeaponOverride> Overrides;
	}

	public readonly float PrecastSpeedup = 1f;

	public readonly float CastSpeedup = 1f;

	public List<AnimationStyleEntry> Animations;

	private List<AnimationClip> m_ClipsListCache;

	private List<AnimationClipWrapper> m_ClipWrappersListCache;

	public override UnitAnimationType Type => UnitAnimationType.CastSpell;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => Animations.SelectMany((AnimationStyleEntry entry) => entry.Default.ClipWrappers.Concat(entry.Overrides.SelectMany((AnimationEntryWeaponOverride ov) => ov.Entry.ClipWrappers)));

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		CastAnimationStyle castStyle = handle.CastStyle;
		WeaponAnimationStyle wpnStyle = handle.AttackWeaponStyle;
		AnimationStyleEntry animationStyleEntry = Animations.SingleOrDefault((AnimationStyleEntry e) => e.Style == castStyle);
		if (animationStyleEntry == null)
		{
			handle.IsSkipped = true;
			handle.Release();
			return;
		}
		AnimationEntry animationEntry = animationStyleEntry.Overrides.SingleOrDefault((AnimationEntryWeaponOverride e) => e.Weapon == wpnStyle)?.Entry ?? animationStyleEntry.Default;
		if (!handle.Manager.IsInCombat)
		{
			animationEntry = animationStyleEntry.Default;
		}
		animationEntry.PrecastLooped = (animationEntry.PrecastLooped ? animationEntry.PrecastLooped : animationStyleEntry.Default.PrecastLooped);
		animationEntry.PrecastStart = (animationEntry.PrecastStart ? animationEntry.PrecastStart : animationStyleEntry.Default.PrecastStart);
		animationEntry.CastClip = (animationEntry.CastClip ? animationEntry.CastClip : animationStyleEntry.Default.CastClip);
		handle.ActionData = animationEntry;
		if (!animationEntry.CastClip)
		{
			PFLog.Default.Error($"No cast clip for {castStyle}/{wpnStyle} in {handle.Manager.AnimationSet}");
			return;
		}
		if (animationEntry.PrecastStart == null && !animationEntry.PrecastLooped && handle.SpecialCastBehaviour == SpecialBehaviourType.None)
		{
			handle.SpecialCastBehaviour = SpecialBehaviourType.NoPrecast;
		}
		if (handle.SpecialCastBehaviour == SpecialBehaviourType.None && handle.CastingTime - animationEntry.CastClip.Length / (CastSpeedup * animationEntry.CastSpeedup) < 1f)
		{
			handle.SpecialCastBehaviour = SpecialBehaviourType.NoPrecast;
		}
		if (handle.SpecialCastBehaviour == SpecialBehaviourType.NoPrecast)
		{
			handle.StartClip(animationEntry.CastClip, ClipDurationType.Oneshot);
			float speed = CastSpeedup * animationEntry.CastSpeedup;
			handle.ActiveAnimation.SetSpeed(speed);
			handle.IsPrecastFinished = true;
		}
		else if (handle.Manager.CurrentAction is UnitAnimationActionHandle unitAnimationActionHandle && unitAnimationActionHandle.Action is UnitAnimationActionCastSpell && unitAnimationActionHandle.SpecialCastBehaviour == SpecialBehaviourType.NoCast && unitAnimationActionHandle.CastStyle == handle.CastStyle)
		{
			handle.StartClip(animationEntry.PrecastLooped, ClipDurationType.Endless);
			handle.ActiveAnimation.SetSpeed(PrecastSpeedup * animationEntry.PrecastSpeedup);
		}
		else
		{
			AnimationClipWrapper clipWrapper = (animationEntry.PrecastStart ? animationEntry.PrecastStart : animationEntry.PrecastLooped);
			handle.StartClip(clipWrapper, ClipDurationType.Endless);
			handle.ActiveAnimation.SetSpeed(PrecastSpeedup * animationEntry.PrecastSpeedup);
		}
	}

	private float GetPrecastDuration(UnitAnimationActionHandle handle)
	{
		switch (handle.SpecialCastBehaviour)
		{
		case SpecialBehaviourType.None:
		{
			float num = 0f;
			if (handle.ActionData is AnimationEntry animationEntry && animationEntry.CastClip != null)
			{
				num = animationEntry.CastClip.Length / (CastSpeedup * animationEntry.CastSpeedup);
			}
			return handle.CastingTime - num;
		}
		case SpecialBehaviourType.NoPrecast:
			return 0f;
		case SpecialBehaviourType.NoCast:
			return handle.CastingTime;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (handle.SpecialCastBehaviour != SpecialBehaviourType.NoCast && handle.IsPrecastFinished)
		{
			base.OnTransitionOutStarted(handle);
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (handle.Manager.BlockAttackAnimation)
		{
			handle.SpeedScale = 0f;
		}
		else
		{
			handle.SpeedScale = Game.CombatAnimSpeedUp;
		}
		AnimationEntry animationEntry = (AnimationEntry)handle.ActionData;
		if (animationEntry?.CastClip == null)
		{
			if (handle.GetTime() >= handle.CastingTime)
			{
				handle.ActEventsCounter++;
				handle.Release();
			}
		}
		else
		{
			if (handle.SpecialCastBehaviour == SpecialBehaviourType.NoPrecast)
			{
				return;
			}
			if (animationEntry.PrecastStart != null && animationEntry.PrecastLooped != null && !handle.IsPrecastFinished && handle.ActiveAnimation?.GetPlayableClip() == animationEntry.PrecastStart)
			{
				float num = ((animationEntry.PrecastStartDuration > 0f) ? animationEntry.PrecastStartDuration : (animationEntry.PrecastStart.Length / (PrecastSpeedup * animationEntry.PrecastSpeedup) - animationEntry.BlendToLoopedTime));
				if (handle.GetTime() > num)
				{
					handle.ActiveAnimation.TransitionOut = animationEntry.BlendToLoopedTime;
					handle.StartClip(animationEntry.PrecastLooped, ClipDurationType.Endless);
					handle.ActiveAnimation.SetSpeed(PrecastSpeedup * animationEntry.PrecastSpeedup);
				}
			}
			if (!handle.IsPrecastFinished && handle.GetTime() > GetPrecastDuration(handle))
			{
				if (handle.SpecialCastBehaviour == SpecialBehaviourType.NoCast || animationEntry.CastClip == null)
				{
					handle.ActEventsCounter++;
					handle.Release();
				}
				else
				{
					handle.ActiveAnimation.TransitionOut = animationEntry.BlendToCastTime;
					handle.StartClip(animationEntry.CastClip, ClipDurationType.Oneshot);
					handle.ActiveAnimation.SetSpeed(CastSpeedup * animationEntry.CastSpeedup);
				}
				handle.IsPrecastFinished = true;
			}
		}
	}
}
