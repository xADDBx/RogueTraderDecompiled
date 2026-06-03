using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[TypeId("13e924320121fb145999e86abe6e450f")]
public abstract class AllowTargetingTypeComponent : BlueprintComponent, IAbilityAllowTargetingType
{
	[SerializeField]
	protected RestrictionCalculator m_Restrictions;

	[SerializeField]
	private IAbilityAllowTargetingType.TargetTypeEnum m_TargetType;

	public IAbilityAllowTargetingType.TargetTypeEnum TargetType => m_TargetType;

	public abstract bool IsRestrictionPassed(AbilityData abilityData);
}
