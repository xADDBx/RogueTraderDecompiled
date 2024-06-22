using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("b04c2413b2a44a488e9bc48b41bb53b3")]
public class ContextActionRotate : ContextAction
{
	private enum Direction
	{
		Left,
		Right
	}

	[SerializeField]
	private Direction m_Direction;

	public ContextValue Angle;

	public override string GetCaption()
	{
		return $"Rotate target on {Angle} degrees to the {m_Direction}";
	}

	protected override void RunAction()
	{
		if (base.Target.Entity is BaseUnitEntity baseUnitEntity)
		{
			int num = Angle.Calculate(base.Context) % 360;
			num = m_Direction switch
			{
				Direction.Left => num, 
				Direction.Right => -num, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			baseUnitEntity.DesiredOrientation += num;
		}
	}
}
