using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a9f6f198a6b74dbd9ce3513674703073")]
public class AbilityShowIfCasterHasFact : BlueprintComponent, IAbilityVisibilityProvider
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("UnitFact")]
	private BlueprintUnitFactReference m_UnitFact;

	public BlueprintUnitFact UnitFact => m_UnitFact?.Get();

	public bool IsAbilityVisible(AbilityData ability)
	{
		return ability.Caster.Facts.Contains(UnitFact);
	}
}
