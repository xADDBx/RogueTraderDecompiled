using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[TypeId("b0adfac2fd2f415a90680ac21219acce")]
public class AbilityRestrictionDuringInterruption : BlueprintComponent, IAbilityRestriction
{
	public bool UseOnlyDuringInterruption;

	[SerializeField]
	private BlueprintUnitFactReference[] m_ExceptionFacts = new BlueprintUnitFactReference[0];

	public ReferenceArrayProxy<BlueprintUnitFact> ExceptionFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] exceptionFacts = m_ExceptionFacts;
			return exceptionFacts;
		}
	}

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		if (ExceptionFacts.Length > 0)
		{
			foreach (BlueprintUnitFact exceptionFact in ExceptionFacts)
			{
				if (ability.Caster.Facts.Contains(exceptionFact))
				{
					return true;
				}
			}
		}
		if (ability.Caster.Initiative.InterruptingOrder > 0)
		{
			return UseOnlyDuringInterruption;
		}
		return !UseOnlyDuringInterruption;
	}

	public string GetAbilityRestrictionUIText()
	{
		return UseOnlyDuringInterruption ? BlueprintRoot.Instance.LocalizedTexts.Reasons.CanUseAbilityOnlyDuringInterruptingTurn : BlueprintRoot.Instance.LocalizedTexts.Reasons.CannotUseAbilityDuringInterruptingTurn;
	}
}
