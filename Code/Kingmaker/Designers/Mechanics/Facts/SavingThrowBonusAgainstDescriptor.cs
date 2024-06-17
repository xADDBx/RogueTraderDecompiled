using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("3819276ccd091df42818bb86295941c1")]
public class SavingThrowBonusAgainstDescriptor : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public SpellDescriptorWrapper SpellDescriptor;

	public ModifierDescriptor ModifierDescriptor;

	public int Value;

	public ContextValue Bonus;

	public bool OnlyPositiveValue;

	[SerializeField]
	[FormerlySerializedAs("DisablingFeature")]
	private BlueprintUnitFactReference m_DisablingFeature;

	public BlueprintUnitFact DisablingFeature => m_DisablingFeature?.Get();

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		int num = Bonus.Calculate(base.Fact.MaybeContext) + Value * base.Fact.GetRank();
		bool flag = evt.Reason.Context != null && (evt.Reason.Context.SpellDescriptor & SpellDescriptor) != Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells.SpellDescriptor.None && (!OnlyPositiveValue || num > Value);
		if (!base.Owner.Facts.Contains(DisablingFeature) && flag)
		{
			evt.ValueModifiers.Add(num, base.Fact, ModifierDescriptor);
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
