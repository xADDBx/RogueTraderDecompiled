using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("c5dd77fc57484bbba4bbc75f37fff18a")]
public class ContextConditionCompare : ContextCondition
{
	private enum Type
	{
		Equal,
		Greater,
		GreaterOrEqual,
		Less,
		LessOrEqual
	}

	[SerializeField]
	private Type m_Type;

	public ContextValue CheckValue;

	public ContextValue TargetValue;

	protected override string GetConditionCaption()
	{
		if (m_Type != 0)
		{
			return $"Is {CheckValue} {m_Type} than {TargetValue}?";
		}
		return $"Is {CheckValue} equals to {TargetValue}?";
	}

	protected override bool CheckCondition()
	{
		int num = CheckValue.Calculate(base.Context);
		int num2 = TargetValue.Calculate(base.Context);
		return m_Type switch
		{
			Type.Equal => num == num2, 
			Type.Greater => num > num2, 
			Type.GreaterOrEqual => num >= num2, 
			Type.Less => num < num2, 
			Type.LessOrEqual => num <= num2, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
