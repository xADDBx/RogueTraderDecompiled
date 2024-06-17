using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("f6e2532c2ee5ff844bebdcf67c8c0967")]
public class Distance : FloatEvaluator
{
	[SerializeReference]
	public PositionEvaluator FirstPoint;

	[SerializeReference]
	public PositionEvaluator SecondPoint;

	public override string GetDescription()
	{
		return "Возвращает дистанцию между двумя точками:\n" + $"{FirstPoint}\n" + $"{SecondPoint}";
	}

	protected override float GetValueInternal()
	{
		return Vector3.Distance(FirstPoint.GetValue(), SecondPoint.GetValue());
	}

	public override string GetCaption()
	{
		return $"Distance ({FirstPoint};{SecondPoint})";
	}
}
