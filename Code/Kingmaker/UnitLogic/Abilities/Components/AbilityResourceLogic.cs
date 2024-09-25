using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("2945aafe5b353dc49b283c982269ade6")]
public class AbilityResourceLogic : BlueprintComponent, IAbilityResourceLogic, IAbilityRestriction
{
	[HideIf("HideBaseResourceVariables")]
	[SerializeField]
	[FormerlySerializedAs("RequiredResource")]
	private BlueprintAbilityResourceReference m_RequiredResource;

	[HideIf("HideBaseResourceVariables")]
	[FormerlySerializedAs("IsSpendResource")]
	[SerializeField]
	private bool m_IsSpendResource;

	[HideIf("HideBaseResourceVariables")]
	public bool CostIsCustom;

	[ShowIf("ShowAmount")]
	public int Amount = 1;

	[ShowIf("ShowIncreasingFacts")]
	public List<BlueprintUnitFactReference> ResourceCostIncreasingFacts = new List<BlueprintUnitFactReference>();

	public List<BlueprintUnitFactReference> ResourceCostDecreasingFacts = new List<BlueprintUnitFactReference>();

	[ShowIf("HasCostChangeFacts")]
	public int ResourceCostChangePerFact = 1;

	public BlueprintAbilityResource RequiredResource => m_RequiredResource?.Get();

	[UsedImplicitly]
	private bool HasCostChangeFacts
	{
		get
		{
			if (ResourceCostIncreasingFacts.Count <= 0)
			{
				return ResourceCostDecreasingFacts.Count > 0;
			}
			return true;
		}
	}

	[UsedImplicitly]
	private bool ShowAmount
	{
		get
		{
			if (!HideBaseResourceVariables && IsSpendResource())
			{
				return !CostIsCustom;
			}
			return false;
		}
	}

	[UsedImplicitly]
	private bool ShowIncreasingFacts
	{
		get
		{
			if (!HideBaseResourceVariables && IsSpendResource())
			{
				return !CostIsCustom;
			}
			return false;
		}
	}

	protected virtual bool HideBaseResourceVariables => false;

	public bool IsSpendResource()
	{
		return m_IsSpendResource;
	}

	public virtual bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		int amount = (IsSpendResource() ? CalculateCost(ability) : 0);
		return ability.Caster.GetAbilityResourcesOptional()?.HasEnoughResource(RequiredResource, amount) ?? true;
	}

	public virtual string GetAbilityRestrictionUIText()
	{
		return LocalizedTexts.Instance.Reasons.NoResources;
	}

	public virtual void Spend(AbilityData ability)
	{
		MechanicEntity caster = ability.Caster;
		if (caster == null)
		{
			PFLog.Default.Error("Caster is missing");
		}
		else if (!caster.IsCheater)
		{
			if (!IsAbilityRestrictionPassed(ability))
			{
				PFLog.Default.Error("Ability {0} is not available for caster {1}", ability.Blueprint, caster);
			}
			else if (IsSpendResource())
			{
				ability.Caster.GetAbilityResourcesOptional()?.Spend(SimpleBlueprintExtendAsObject.Or(ability.OverrideRequiredResource, null) ?? RequiredResource, CalculateCost(ability));
			}
		}
	}

	public virtual int CalculateCost(AbilityData ability)
	{
		if (CostIsCustom)
		{
			IAbilityResourceCostCalculator component = ability.Blueprint.GetComponent<IAbilityResourceCostCalculator>();
			if (component == null)
			{
				PFLog.Default.Error(ability.Blueprint, $"Custom resource cost calculator is missing: {ability.Blueprint}");
				return 1;
			}
			return component.Calculate(ability);
		}
		int num = Amount;
		for (int i = 0; i < ResourceCostIncreasingFacts.Count; i++)
		{
			BlueprintUnitFactReference blueprintUnitFactReference = ResourceCostIncreasingFacts[i];
			if (ability.Caster.Facts.Contains(blueprintUnitFactReference.Get()))
			{
				num += ResourceCostChangePerFact;
			}
		}
		for (int j = 0; j < ResourceCostDecreasingFacts.Count; j++)
		{
			BlueprintUnitFactReference blueprintUnitFactReference2 = ResourceCostDecreasingFacts[j];
			if (ability.Caster.Facts.Contains(blueprintUnitFactReference2.Get()))
			{
				num -= ResourceCostChangePerFact;
			}
		}
		return num;
	}

	public int CalculateResourceAmount(AbilityData ability)
	{
		if (IsSpendResource())
		{
			PartAbilityResourceCollection abilityResourcesOptional = ability.Caster.GetAbilityResourcesOptional();
			if (abilityResourcesOptional != null)
			{
				return abilityResourcesOptional.GetResourceAmount(RequiredResource);
			}
		}
		return -1;
	}
}
