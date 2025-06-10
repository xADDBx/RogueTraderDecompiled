using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("3a6f23e8d92c41d19150be5b2f7f8148")]
public class AbilityNoCastAnimation : BlueprintComponent, IAbilityCustomAnimation
{
	[SerializeField]
	private float m_PretendActDelay = 1f;

	public float PretendActDelay => m_PretendActDelay;

	public UnitAnimationActionLink GetAbilityAction(BaseUnitEntity caster)
	{
		return null;
	}
}
