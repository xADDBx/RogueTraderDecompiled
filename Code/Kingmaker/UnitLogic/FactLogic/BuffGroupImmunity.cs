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
		if (evt.Blueprint.AbilityGroups.Any((BlueprintAbilityGroup p) => Groups.Contains(p)))
		{
			evt.Immunity = true;
			evt.DisableGameLog = m_DisableGameLog;
		}
	}

	public void OnEventDidTrigger(RuleCalculateCanApplyBuff evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
