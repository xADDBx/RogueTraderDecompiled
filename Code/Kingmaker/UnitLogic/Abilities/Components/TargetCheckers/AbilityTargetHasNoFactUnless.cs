using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target has fact")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("c86d7ab572ddfea4ca09cb8c04facb87")]
public class AbilityTargetHasNoFactUnless : BlueprintComponent, IAbilityTargetRestriction, IAbilityCasterRestriction
{
	private enum CheckedTargets
	{
		Target,
		Caster
	}

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintUnitFactReference[] m_CheckedFacts;

	[SerializeField]
	[Tooltip("Target для проверки отсутствия CheckedFacts, по умолчанию это Target")]
	private CheckedTargets m_CheckedFactsTarget;

	[SerializeField]
	private BlueprintUnitFactReference m_UnlessFact;

	[SerializeField]
	[Tooltip("Target для проверки наличия UnlessFact, по умолчанию это Caster")]
	private CheckedTargets m_UnlessFactTarget = CheckedTargets.Caster;

	public ReferenceArrayProxy<BlueprintUnitFact> CheckedFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] checkedFacts = m_CheckedFacts;
			return checkedFacts;
		}
	}

	[CanBeNull]
	public BlueprintUnitFact UnlessFact => m_UnlessFact?.Get();

	private bool IsCasterRestriction
	{
		get
		{
			if (m_CheckedFactsTarget == CheckedTargets.Caster)
			{
				return m_UnlessFactTarget == CheckedTargets.Caster;
			}
			return false;
		}
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (IsCasterRestriction)
		{
			return IsTargetRestrictionPassedInternal(caster, null);
		}
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return GetAbilityTargetRestrictionUIText(null, null, default(Vector3));
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (!IsCasterRestriction)
		{
			return IsTargetRestrictionPassedInternal(ability.Caster, target);
		}
		return true;
	}

	public bool IsTargetRestrictionPassedInternal(MechanicEntity caster, TargetWrapper target)
	{
		MechanicEntity mechanicEntity = ((m_CheckedFactsTarget == CheckedTargets.Target) ? target.Entity : caster);
		if (mechanicEntity == null)
		{
			return false;
		}
		bool flag = false;
		foreach (BlueprintUnitFact checkedFact in CheckedFacts)
		{
			flag = mechanicEntity.Facts.Contains(checkedFact);
			if (flag)
			{
				break;
			}
		}
		bool flag2 = false;
		if (UnlessFact != null)
		{
			switch (m_UnlessFactTarget)
			{
			case CheckedTargets.Target:
				flag2 = target.Entity?.Facts.Contains(UnlessFact) ?? false;
				break;
			case CheckedTargets.Caster:
				flag2 = caster.Facts.Contains(UnlessFact);
				break;
			}
		}
		return !flag || flag2;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		string noFacts = string.Join(", ", CheckedFacts.Select((BlueprintUnitFact f) => f.Name));
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasNoFactUnless.ToString(delegate
		{
			GameLogContext.Text = noFacts;
			GameLogContext.Description = UnlessFact?.Name;
		});
	}
}
