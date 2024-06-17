using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("e2b7a7cc2b994e3c925a5ab6abc2b793")]
public class ConditionalUnitEvaluator : AbstractUnitEvaluator
{
	[Serializable]
	private class ConditionalPair
	{
		public ConditionsChecker Condition;

		[ValidateNotNull]
		[SerializeReference]
		public AbstractUnitEvaluator Unit;
	}

	[SerializeField]
	private ConditionalPair[] m_Units;

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Default;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		ConditionalPair[] units = m_Units;
		foreach (ConditionalPair conditionalPair in units)
		{
			if ((bool)conditionalPair.Unit && conditionalPair.Condition.Check())
			{
				return conditionalPair.Unit.GetValue();
			}
		}
		if (!m_Default)
		{
			return null;
		}
		return m_Default.GetValue();
	}

	public override string GetCaption()
	{
		return "Unit selector";
	}
}
