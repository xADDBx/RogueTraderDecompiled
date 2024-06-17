using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Code.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("81a5aaa95ba04ac3a228ff856a96e424")]
public class AddBuffOnCombatRandomEncounter : UnitFactComponentDelegate, ICombatRandomEncounterHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	private BlueprintBuff Buff => m_Buff?.Get();

	protected override void OnDeactivate()
	{
		RemoveAllFactsOriginatedFromThisComponent(base.Owner);
	}

	public void HandleCombatRandomEncounterStart()
	{
		if (base.Owner.Facts.FindBySource(Buff, base.Fact, this) == null)
		{
			base.Owner.Buffs.Add(Buff, base.Owner)?.AddSource(base.Fact, this);
		}
	}

	public void HandleCombatRandomEncounterFinish()
	{
		EntityFact entityFact = base.Owner.Facts.FindBySource(Buff, base.Fact, this);
		if (entityFact != null)
		{
			base.Owner.Buffs.Remove(entityFact);
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
