using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("ca70547ab1274b6190a871edecd24624")]
public class BuffGroupImmunity : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCanApplyBuff>, IRulebookHandler<RuleCalculateCanApplyBuff>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	[SerializeField]
	private bool m_DisableGameLog;

	[SerializeField]
	[Tooltip("If true, immunity will apply to all buffs, except those in Groups.")]
	private bool m_InvertCondition;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateCanApplyBuff evt)
	{
		if (IsImmune(evt))
		{
			Debug.Log("Immunity to " + evt.Blueprint.name);
			evt.Immunity = true;
			evt.DisableGameLog = m_DisableGameLog;
		}
	}

	public void OnEventDidTrigger(RuleCalculateCanApplyBuff evt)
	{
	}

	private bool IsImmune(RuleCalculateCanApplyBuff evt)
	{
		ReferenceArrayProxy<BlueprintAbilityGroup> abilityGroups = evt.Blueprint.AbilityGroups;
		if (!m_InvertCondition)
		{
			return abilityGroups.Any((BlueprintAbilityGroup p) => Groups.Contains(p));
		}
		return abilityGroups.All((BlueprintAbilityGroup p) => !Groups.Contains(p));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
