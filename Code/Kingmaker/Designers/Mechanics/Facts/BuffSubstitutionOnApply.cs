using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Apply fact of event of applying another fact")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("e16d7aaf01a2ab640beb72e924172c02")]
public class BuffSubstitutionOnApply : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCanApplyBuff>, IRulebookHandler<RuleCalculateCanApplyBuff>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("GainedFact")]
	private BlueprintBuffReference m_GainedFact;

	[SerializeField]
	[FormerlySerializedAs("SubstituteBuff")]
	private BlueprintBuffReference m_SubstituteBuff;

	public BlueprintBuff GainedFact => m_GainedFact?.Get();

	public BlueprintBuff SubstituteBuff => m_SubstituteBuff?.Get();

	public void OnEventAboutToTrigger(RuleCalculateCanApplyBuff evt)
	{
		if (evt.Blueprint == GainedFact)
		{
			evt.CanApply = false;
			base.Owner.Buffs.Add(SubstituteBuff, evt.ConcreteInitiator, evt.Duration);
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
