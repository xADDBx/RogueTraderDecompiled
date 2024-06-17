using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[AllowMultipleComponents]
[TypeId("dba9dbd41a904614809881497ddebe3b")]
public abstract class AbilityLifecycleTrigger : MechanicEntityFactComponentDelegate, IHashable
{
	private class TransientData : IEntityFactComponentTransientData
	{
		public readonly List<string> AppliedFacts = new List<string>();
	}

	public PropertyCalculator Condition;

	public ActionList StartActions;

	public ActionList EndActions;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintMechanicEntityFact.Reference[] m_Facts = new BlueprintMechanicEntityFact.Reference[0];

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] facts = m_Facts;
			return facts;
		}
	}

	protected void RunStartActions(AbilityExecutionContext context)
	{
		if (!CheckCondition(context))
		{
			return;
		}
		TransientData transientData = RequestTransientData<TransientData>();
		foreach (BlueprintMechanicEntityFact fact in Facts)
		{
			EntityFact entityFact = base.Owner.AddFact(fact, base.Fact.MaybeContext);
			if (entityFact != null)
			{
				transientData.AppliedFacts.Add(entityFact.UniqueId);
			}
		}
		base.Fact.RunActionInContext(StartActions);
	}

	protected void RunEndActions(AbilityExecutionContext context)
	{
		if (Condition.Any && !CheckCondition(context))
		{
			return;
		}
		TransientData transientData = RequestTransientData<TransientData>();
		MechanicEntity owner = base.Owner;
		foreach (string appliedFact in transientData.AppliedFacts)
		{
			owner.Facts.RemoveById(appliedFact);
		}
		transientData.AppliedFacts.Clear();
		base.Fact.RunActionInContext(EndActions);
	}

	private bool CheckCondition(AbilityExecutionContext context)
	{
		if (base.Fact.Blueprint is BlueprintAbility blueprintAbility && blueprintAbility != context.Ability.Blueprint)
		{
			return false;
		}
		PropertyContext context2 = new PropertyContext(base.Owner, base.Fact, context.MainTarget.Entity, base.Fact.MaybeContext, null, context.Ability);
		return Condition.GetValue(context2) != 0;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
