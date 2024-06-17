using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d9675b68ab7715d42ba3bb44256ddd6b")]
public class AddAbilityUseTargetTrigger : UnitFactComponentDelegate, ITargetRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public ActionList Action;

	public bool AfterCast;

	public AbilityType Type;

	public bool ToCaster;

	public bool SpellList;

	[ShowIf("SpellList")]
	[SerializeField]
	[FormerlySerializedAs("Spells")]
	private BlueprintAbilityReference[] m_Spells = new BlueprintAbilityReference[0];

	public ReferenceArrayProxy<BlueprintAbility> Spells
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] spells = m_Spells;
			return spells;
		}
	}

	private void RunAction(RulePerformAbility evt)
	{
		_ = evt.SpellTarget;
		if ((evt.Reason.Ability.Blueprint.Type == Type && !SpellList) || Spells.HasReference(evt.Reason.Ability.Blueprint))
		{
			if (ToCaster)
			{
				base.Fact.RunActionInContext(Action, evt.ConcreteInitiator.ToITargetWrapper());
			}
			else
			{
				base.Fact.RunActionInContext(Action, base.OwnerTargetWrapper);
			}
		}
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
		if (!AfterCast)
		{
			RunAction(evt);
		}
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (AfterCast)
		{
			RunAction(evt);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
