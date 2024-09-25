using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("3577eb7fd5b1caa4ca13855b16201704")]
public class BuffDescriptorImmunity : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateCanApplyBuff>, IRulebookHandler<RuleCalculateCanApplyBuff>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public SpellDescriptorWrapper Descriptor;

	[SerializeField]
	[FormerlySerializedAs("IgnoreFeature")]
	private BlueprintUnitFactReference m_IgnoreFeature;

	public bool CheckFact;

	[ShowIf("CheckFact")]
	[SerializeField]
	[FormerlySerializedAs("FactToCheck")]
	private BlueprintUnitFactReference m_FactToCheck;

	public BlueprintUnitFact IgnoreFeature => m_IgnoreFeature?.Get();

	public BlueprintUnitFact FactToCheck => m_FactToCheck?.Get();

	private bool IsImmune(MechanicsContext context)
	{
		bool num = Descriptor.HasAnyFlag(context.SpellDescriptor);
		bool flag = context.MaybeCaster == null;
		bool flag2 = IgnoreFeature == null || flag || !context.MaybeCaster.Facts.Contains(IgnoreFeature);
		bool flag3 = !CheckFact || (!flag && context.MaybeCaster.Facts.Contains(FactToCheck));
		return num && flag2 && flag3;
	}

	public void OnEventAboutToTrigger(RuleCalculateCanApplyBuff evt)
	{
		if (IsImmune(evt.Context))
		{
			evt.Immunity = true;
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
