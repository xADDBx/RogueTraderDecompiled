using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("289b383f327f643449b0b142c36fabfc")]
public class NearestPosition : PositionEvaluator
{
	[Serializable]
	private class ConditionalPair
	{
		public ConditionsChecker Condition;

		[ValidateNotNull]
		[SerializeReference]
		public PositionEvaluator Position;
	}

	[SerializeField]
	private ConditionalPair[] m_Positions;

	[SerializeReference]
	public PositionEvaluator Center;

	public override string GetDescription()
	{
		return $"Находит ближайшую точку от центра {Center}, для которой выполняются условия.\n" + "В условиях для точек, текущую проверяемую точку можно получить эвалюатором CheckedPosition\nЕсли ни одна точка не проходит по условиям, вернет позицию центра";
	}

	protected override Vector3 GetValueInternal()
	{
		Vector3 value = Center.GetValue();
		Vector3 result = value;
		float num = float.PositiveInfinity;
		ConditionalPair[] positions = m_Positions;
		foreach (ConditionalPair conditionalPair in positions)
		{
			Vector3 value2 = conditionalPair.Position.GetValue();
			using (ContextData<CheckedPositionData>.Request().Setup(value2))
			{
				if ((bool)conditionalPair.Position && conditionalPair.Condition.Check())
				{
					float sqrMagnitude = (value - value2).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						result = value2;
						num = sqrMagnitude;
					}
				}
			}
		}
		return result;
	}

	public override string GetCaption()
	{
		return $"Nearest position from {Center}";
	}
}
