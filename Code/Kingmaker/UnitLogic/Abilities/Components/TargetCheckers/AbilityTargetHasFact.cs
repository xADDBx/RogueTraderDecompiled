using System.Linq;
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
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target has fact")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("471113ce4758b9b4d8a3162276d8d570")]
public class AbilityTargetHasFact : BlueprintComponent, IAbilityTargetRestriction
{
	[SerializeField]
	[FormerlySerializedAs("CheckedFacts")]
	private BlueprintUnitFactReference[] m_CheckedFacts;

	public bool Inverted;

	public ReferenceArrayProxy<BlueprintUnitFact> CheckedFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] checkedFacts = m_CheckedFacts;
			return checkedFacts;
		}
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		MechanicEntity entity = target.Entity;
		if (entity == null)
		{
			return false;
		}
		bool flag = false;
		foreach (BlueprintUnitFact checkedFact in CheckedFacts)
		{
			flag = entity.Facts.Contains(checkedFact);
			if (flag)
			{
				break;
			}
		}
		return flag != Inverted;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		string facts = string.Join(", ", m_CheckedFacts.Select((BlueprintUnitFactReference i) => i.Get()?.Name).NotNull());
		return (Inverted ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasNoFact : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasFact).ToString(delegate
		{
			GameLogContext.Text = facts;
		});
	}
}
