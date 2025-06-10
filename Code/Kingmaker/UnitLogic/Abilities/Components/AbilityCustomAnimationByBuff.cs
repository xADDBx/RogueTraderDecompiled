using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Visual.Animation.Kingmaker;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("d19abe965ccc4bd2891e68c149a9ab56")]
public class AbilityCustomAnimationByBuff : BlueprintComponent, IAbilityCustomAnimation
{
	[Serializable]
	public class Entry
	{
		[ValidateNotNull]
		[SerializeField]
		[FormerlySerializedAs("Buff")]
		private BlueprintBuffReference m_Buff;

		[ValidateNotNull]
		public UnitAnimationActionLink AnimationLink;

		public BlueprintBuff Buff => m_Buff?.Get();
	}

	[ValidateNotNull]
	public UnitAnimationActionLink DefaultAnimationLink;

	public UnitAnimationType OverrideAnimationType = UnitAnimationType.Unused;

	public Entry[] Variants;

	public UnitAnimationActionLink GetAbilityAction(BaseUnitEntity caster)
	{
		for (int i = 0; i < Variants.Length; i++)
		{
			if ((bool)Variants[i].Buff && caster.Buffs.Contains(Variants[i].Buff))
			{
				return Variants[i].AnimationLink;
			}
		}
		return DefaultAnimationLink;
	}
}
