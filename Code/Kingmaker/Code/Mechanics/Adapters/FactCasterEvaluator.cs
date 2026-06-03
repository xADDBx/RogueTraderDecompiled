using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("0ee2db74c675e004b979b0286132637c")]
public class FactCasterEvaluator : AbstractUnitEvaluator
{
	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	[SerializeReference]
	[CanBeNull]
	private AbstractUnitEvaluator m_TargetUnit;

	[CanBeNull]
	public BlueprintUnitFact Fact => m_Fact?.Get();

	public override string GetCaption()
	{
		string text = ((m_TargetUnit != null) ? m_TargetUnit.GetCaption() : "context caster");
		return "Caster of fact [" + (Fact?.name ?? "none") + "] on [" + text + "]";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		BlueprintUnitFact fact = Fact;
		if (fact == null)
		{
			return null;
		}
		AbstractUnitEntity abstractUnitEntity;
		if (m_TargetUnit != null)
		{
			if (!m_TargetUnit.TryGetValue(out var value))
			{
				return null;
			}
			abstractUnitEntity = value;
		}
		else
		{
			abstractUnitEntity = ContextData<MechanicsContext.Data>.Current?.Context.MaybeCaster as AbstractUnitEntity;
		}
		return (abstractUnitEntity?.Facts.Get(fact))?.MaybeContext?.MaybeCaster as AbstractUnitEntity;
	}
}
