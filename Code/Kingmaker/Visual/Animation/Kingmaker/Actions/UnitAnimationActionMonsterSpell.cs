using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[CreateAssetMenu(fileName = "UnitAnimationCast", menuName = "Animation Manager/Actions/Unit Animation Monster Cast")]
public class UnitAnimationActionMonsterSpell : UnitAnimationAction
{
	[Serializable]
	public class BlueprintAbilityLink
	{
		public string AssetId;
	}

	[Serializable]
	public class AdditionalClipData
	{
		[AssetPicker("")]
		[ValidateNotNull]
		[ValidateHasActEvent]
		[DrawEventWarning]
		public AnimationClipWrapper ClipWrapper;

		public List<BlueprintAbilityLink> SpellsList;
	}

	[SerializeField]
	[ValidateNotNull]
	[ValidateHasActEvent]
	[DrawEventWarning]
	private AnimationClipWrapper m_CastClipWrapper;

	[SerializeField]
	private AdditionalClipData[] Additional;

	private List<AnimationClipWrapper> m_ClipWrappersListCache;

	public override UnitAnimationType Type => UnitAnimationType.CastSpell;

	public AnimationClipWrapper CastClipWrapper => m_CastClipWrapper;

	public override IEnumerable<AnimationClipWrapper> ClipWrappers
	{
		get
		{
			m_ClipWrappersListCache = m_ClipWrappersListCache ?? (from d in Additional.EmptyIfNull()
				select d.ClipWrapper).Concat(new AnimationClipWrapper[1] { m_CastClipWrapper }).Distinct().ToList();
			return m_ClipWrappersListCache;
		}
	}

	public override void OnStart(UnitAnimationActionHandle handle)
	{
		BlueprintAbility spell = handle.Spell;
		AnimationClipWrapper animationClipWrapper = m_CastClipWrapper;
		if ((bool)spell)
		{
			AdditionalClipData[] additional = Additional;
			foreach (AdditionalClipData additionalClipData in additional)
			{
				if ((bool)additionalClipData.ClipWrapper && additionalClipData.SpellsList.HasItem((BlueprintAbilityLink s) => s.AssetId == spell.AssetGuid))
				{
					animationClipWrapper = additionalClipData.ClipWrapper;
				}
			}
		}
		handle.ActionData = animationClipWrapper;
		handle.StartClip(animationClipWrapper, ClipDurationType.Oneshot);
	}
}
