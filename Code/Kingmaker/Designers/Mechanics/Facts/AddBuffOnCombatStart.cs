using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b32e336dff316d6408375f78ba79d8f6")]
public class AddBuffOnCombatStart : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleRollInitiative>, IRulebookHandler<RuleRollInitiative>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public bool CheckParty;

	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintBuffReference m_Feature;

	public BlueprintBuff Feature => m_Feature?.Get();

	protected override void OnDeactivate()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
	}

	public void OnEventAboutToTrigger(RuleRollInitiative evt)
	{
		if (((CheckParty && base.Owner.Faction.IsPlayer) || (!CheckParty && !base.Owner.Faction.IsPlayer)) && base.Owner.Facts.FindBySource(Feature, base.Fact, this) == null)
		{
			base.Owner.Buffs.Add(Feature, base.Owner)?.AddSource(base.Fact, this);
		}
	}

	public void OnEventDidTrigger(RuleRollInitiative evt)
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
