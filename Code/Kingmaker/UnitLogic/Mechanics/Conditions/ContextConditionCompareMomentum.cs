using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Units;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("1c924a20f55929644bdb6b7067c3da22")]
public class ContextConditionCompareMomentum : ContextCondition
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

	[SerializeReference]
	[SerializeField]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private ContextValue m_Value;

	protected override string GetConditionCaption()
	{
		return $"{m_Unit}'s momentum is {m_Type} than {m_Value}?";
	}

	protected override bool CheckCondition()
	{
		AbstractUnitEntity unit = m_Unit?.GetValue();
		if (unit == null)
		{
			PFLog.Default.Error("Unit is missing");
			return false;
		}
		MomentumGroup momentumGroup = Game.Instance.TurnController.MomentumController.Groups.FindOrDefault((MomentumGroup p) => p.Units.Contains(unit));
		if (momentumGroup == null)
		{
			PFLog.Default.Error("Unit's momentum group is missing");
			return false;
		}
		int momentum = momentumGroup.Momentum;
		int num = m_Value.Calculate(base.Context);
		return m_Type switch
		{
			Type.Equal => momentum == num, 
			Type.Greater => momentum > num, 
			Type.GreaterOrEqual => momentum >= num, 
			Type.Less => momentum < num, 
			Type.LessOrEqual => momentum <= num, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
