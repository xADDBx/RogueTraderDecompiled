using System;
using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationSpecialAttack", menuName = "Animation Manager/Actions/Unit Special Attack")]
public class UnitAnimationActionSpecialAttack : UnitAnimationAction, IUnitAnimationActionHasVariants
{
	[Serializable]
	public class Entry
	{
		[ValidateNotEmpty]
		[ValidateNoNullEntries]
		[ValidateHasActEvent]
		public AnimationClipWrapper[] VariantWrappers = new AnimationClipWrapper[0];

		[AssetPicker("")]
		[ValidateHasActEvent]
		public AnimationClipWrapper RendWrapper;

		[AssetPicker("")]
		[ValidateHasActEvent]
		public AnimationClipWrapper ChargeWrapper;

		public bool HasRangeBlend;

		public float[] BlendRanges;
	}

	[SerializeField]
	private UnitAnimationSpecialAttackType m_AttackType;

	[SerializeField]
	[ValidateNotEmpty]
	private List<Entry> m_Attacks;

	public List<Entry> Attacks => m_Attacks;

	public override UnitAnimationType Type => UnitAnimationType.SpecialAttack;

	public UnitAnimationSpecialAttackType AttackType
	{
		get
		{
			return m_AttackType;
		}
		set
		{
			m_AttackType = value;
		}
	}

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			foreach (Entry entry in m_Attacks)
			{
				for (int i = 0; i < entry.VariantWrappers.Length; i++)
				{
					yield return entry.VariantWrappers[i];
				}
				if ((bool)entry.RendWrapper)
				{
					yield return entry.RendWrapper;
				}
				if ((bool)entry.ChargeWrapper)
				{
					yield return entry.ChargeWrapper;
				}
			}
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		if (handle.AnimationClipWrapper != null)
		{
			handle.StartClip(handle.AnimationClipWrapper, ClipDurationType.Oneshot);
			return;
		}
		if (m_Attacks.Count == 0)
		{
			PFLog.Default.Error($"Cannot perform SpecialAttack {AttackType} animation because there is no animation clip {base.name} and no overriding clip was provided.");
			return;
		}
		Entry entry = m_Attacks[handle.SpecialAttackCount % m_Attacks.Count];
		if (entry.HasRangeBlend && entry.BlendRanges.Length == entry.VariantWrappers.Length)
		{
			StartBlendedAttack(entry, handle);
			return;
		}
		AnimationClipWrapper variant = GetVariant(handle);
		if ((bool)variant)
		{
			handle.StartClip(variant, ClipDurationType.Oneshot);
		}
	}

	private void StartBlendedAttack(Entry attack, UnitAnimationActionHandle handle)
	{
		int num = 0;
		for (int i = 0; i < attack.BlendRanges.Length; i++)
		{
			num = i;
			if (attack.BlendRanges[i] > handle.AttackTargetDistance)
			{
				break;
			}
		}
		AnimationClipWrapper animationClipWrapper = null;
		if (num == 0)
		{
			animationClipWrapper = attack.VariantWrappers[0];
			if ((bool)animationClipWrapper)
			{
				handle.StartClip(animationClipWrapper, ClipDurationType.Oneshot);
			}
			return;
		}
		if (attack.BlendRanges[num] <= handle.AttackTargetDistance)
		{
			animationClipWrapper = attack.VariantWrappers[num];
			if ((bool)animationClipWrapper)
			{
				handle.StartClip(animationClipWrapper, ClipDurationType.Oneshot);
			}
			return;
		}
		AnimationClipWrapper animationClipWrapper2 = attack.VariantWrappers[num - 1];
		AnimationClipWrapper animationClipWrapper3 = attack.VariantWrappers[num];
		if ((bool)animationClipWrapper2 && (bool)animationClipWrapper3)
		{
			float num2 = (attack.BlendRanges[num] - handle.AttackTargetDistance) / (attack.BlendRanges[num] - attack.BlendRanges[num - 1]);
			AnimationBlend animationBlend = new AnimationBlend(handle);
			PFLog.Default.Log($"Start blend attack: range {handle.AttackTargetDistance} blend {num2} expected range {attack.BlendRanges[num - 1] * num2 + attack.BlendRanges[num] * (1f - num2)}");
			handle.Manager.AddAnimationClip(handle, animationClipWrapper2, null, useEmptyAvatarMask: true, isAdditive: false, ClipDurationType.Oneshot, animationBlend);
			handle.Manager.AddClipToComposition(handle, animationClipWrapper3, null, isAdditive: false);
			animationBlend.SetSpeed(handle.SpeedScale);
			animationBlend.Blend = num2;
		}
	}

	public override void OnUpdate(UnitAnimationActionHandle handle, float deltaTime)
	{
		if (handle.ActiveAnimation == null && handle.GetTime() > 0.1f)
		{
			handle.ActEventsCounter++;
			handle.Release();
		}
	}

	public float GetDuration(UnitAnimationActionHandle handle)
	{
		return GetVariant(handle)?.Length ?? 0.1f;
	}

	private AnimationClipWrapper GetVariant(UnitAnimationActionHandle handle)
	{
		Entry entry = m_Attacks[handle.SpecialAttackCount % m_Attacks.Count];
		if (entry.VariantWrappers == null || entry.VariantWrappers.Length == 0)
		{
			PFLog.Default.Error(handle.Action, $"Action has zero variants: {base.name} (attack #{handle.SpecialAttackCount})");
			return null;
		}
		AnimationClipWrapper animationClipWrapper = (handle.AnimationClipWrapper ? handle.AnimationClipWrapper : entry.VariantWrappers.Get(Mathf.RoundToInt(handle.Manager.StatefulRandom.Range(0, entry.VariantWrappers.Length - 1))));
		if (animationClipWrapper == null)
		{
			PFLog.Default.Error(this, "Action has null variant at index {0}", handle.Variant);
			animationClipWrapper = entry.VariantWrappers.Get(0);
		}
		return animationClipWrapper;
	}

	public int GetVariantsCount(UnitAnimationActionHandle handle)
	{
		if (m_Attacks.Count == 0)
		{
			return 0;
		}
		return m_Attacks[handle.SpecialAttackCount % m_Attacks.Count].VariantWrappers.Length;
	}
}
