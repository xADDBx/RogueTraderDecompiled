using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[ComponentName("Saving throw bonus against fact")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("11ec481f283cbc74f98221b1890b30d9")]
public class SavingThrowBonusAgainstFact : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("CheckedFact")]
	private BlueprintFeatureReference m_CheckedFact;

	public ModifierDescriptor Descriptor;

	public int Value;

	[Tooltip("Всегда успех")]
	public bool AlwaysSucceed;

	[Tooltip("Всегда провал")]
	public bool AlwaysFail;

	public BlueprintFeature CheckedFact => m_CheckedFact?.Get();

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		MechanicEntity caster = evt.Reason.Caster;
		if (caster != null && caster.Facts.Contains(CheckedFact))
		{
			if (AlwaysSucceed)
			{
				evt.SetAlwaysSucceed(base.Fact, Descriptor);
			}
			else if (AlwaysFail)
			{
				evt.SetAlwaysFail(base.Fact, Descriptor);
			}
			else
			{
				evt.AddValueModifiers(Value * base.Fact.GetRank(), base.Fact, Descriptor);
			}
		}
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
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
