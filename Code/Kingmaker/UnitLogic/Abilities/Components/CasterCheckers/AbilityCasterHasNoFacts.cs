using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("c53b6a74192ad9d43818c98797b7e0de")]
public class AbilityCasterHasNoFacts : BlueprintComponent, IAbilityCasterRestriction
{
	[SerializeField]
	[FormerlySerializedAs("Facts")]
	private BlueprintUnitFactReference[] m_Facts;

	public ReferenceArrayProxy<BlueprintUnitFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] facts = m_Facts;
			return facts;
		}
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		foreach (BlueprintUnitFact fact in Facts)
		{
			if (caster.Facts.Contains(fact))
			{
				return false;
			}
		}
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		LocalizedString hasForbiddenCondition = LocalizedTexts.Instance.Reasons.HasForbiddenCondition;
		string facts = string.Join(", ", Facts.ToArray().Select(UIUtilityTexts.GetBlueprintUnitFactNameText));
		return hasForbiddenCondition.ToString(delegate
		{
			GameLogContext.Text = facts;
		});
	}
}
