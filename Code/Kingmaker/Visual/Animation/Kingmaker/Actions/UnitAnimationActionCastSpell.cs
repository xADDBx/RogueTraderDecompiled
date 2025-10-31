using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem;
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
		public float BlendToLoopedTime = 0.1f;

		public float BlendToCastTime = 0.1f;

		[AssetPicker("")]
		[ValidateNotNull]
		[ValidateHasActEvent]
		[DrawEventWarning]
		public AnimationClipWrapper CastClip;

		public readonly float CastSpeedup = 1f;

		public IEnumerable<AnimationClipWrapper> ClipWrappers
		{
			get
			{
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

	public readonly float CastSpeedup = 1f;

	public List<AnimationStyleEntry> Animations;

	private List<AnimationClip> m_ClipsListCache;

	private List<AnimationClipWrapper> m_ClipWrappersListCache;

	public override UnitAnimationType Type => UnitAnimationType.CastSpell;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers => Animations.SelectMany((AnimationStyleEntry entry) => entry.Default.ClipWrappers.Concat(entry.Overrides.SelectMany((AnimationEntryWeaponOverride ov) => ov.Entry.ClipWrappers)));

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		if (handle.Manager.NeedStepOut && handle.Manager.StepOutDirectionAnimationType != 0)
		{
			return;
		}
		AnimationStyleEntry animationStyleEntry = Animations.SingleOrDefault((AnimationStyleEntry e) => e.Style == handle.CastStyle);
		if (animationStyleEntry == null)
		{
			handle.IsSkipped = true;
			handle.Release();
			RestoreLoopAnimation(handle);
			return;
		}
		CastAnimationStyle castStyle = handle.CastStyle;
		WeaponAnimationStyle wpnStyle = handle.AttackWeaponStyle;
		AnimationEntry animationEntry = animationStyleEntry.Overrides.SingleOrDefault((AnimationEntryWeaponOverride e) => e.Weapon == wpnStyle)?.Entry ?? animationStyleEntry.Default;
		if (!handle.Manager.IsInCombat)
		{
			animationEntry = animationStyleEntry.Default;
		}
		animationEntry.CastClip = (animationEntry.CastClip ? animationEntry.CastClip : animationStyleEntry.Default.CastClip);
		handle.ActionData = animationEntry;
		if (!animationEntry.CastClip)
		{
			PFLog.Default.Error($"No cast clip for {castStyle}/{wpnStyle} in {handle.Manager.AnimationSet}");
			return;
		}
		handle.StartClip(animationEntry.CastClip, ClipDurationType.Oneshot);
		float speed = CastSpeedup * animationEntry.CastSpeedup;
		handle.ActiveAnimation.SetSpeed(speed);
		handle.IsPrecastFinished = true;
		handle.SkipFirstTick = false;
		handle.SkipFirstTickOnHandle = false;
		handle.CorrectTransitionOutTime = true;
		if (handle.Manager.BuffLoopAction != null)
		{
			handle.HasCrossfadePriority = true;
		}
	}

	public override void OnTransitionOutStarted(UnitAnimationActionHandle handle)
	{
		if (handle.SpecialCastBehaviour != SpecialBehaviourType.NoCast && handle.IsPrecastFinished)
		{
			base.OnTransitionOutStarted(handle);
			RestoreLoopAnimation(handle);
		}
	}

	private static void RestoreLoopAnimation(UnitAnimationActionHandle handle)
	{
		handle.Unit.Data.Facts.GetAll((EntityFact ef) => ef.GetComponent<PlayLoopAnimationByBuff>() != null).FirstOrDefault()?.CallComponents(delegate(PlayLoopAnimationByBuff playLoop)
		{
			playLoop.TrySetAction(skipEnter: true);
		});
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
		if (((AnimationEntry)handle.ActionData)?.CastClip == null)
		{
			UpdateInvalid(handle);
		}
	}

	private static void UpdateInvalid(UnitAnimationActionHandle handle)
	{
		float time = handle.GetTime();
		if (handle.ActEventsCounter < 1 && time >= 0.4f)
		{
			handle.ActEventsCounter = 1;
		}
		if (time >= 0.8f)
		{
			handle.Release();
		}
	}
}
