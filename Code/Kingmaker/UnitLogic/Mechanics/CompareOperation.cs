using System;

namespace Kingmaker.UnitLogic.Mechanics;

public static class CompareOperation
{
	public enum Type
	{
		Equal,
		Greater,
		GreaterOrEqual,
		Less,
		LessOrEqual
	}

	public static bool CheckCondition(this Type type, float checkValue, float targetValue)
	{
		return type switch
		{
			Type.Equal => Math.Abs(checkValue - targetValue) < 0.001f, 
			Type.Greater => checkValue > targetValue, 
			Type.GreaterOrEqual => checkValue >= targetValue, 
			Type.Less => checkValue < targetValue, 
			Type.LessOrEqual => checkValue <= targetValue, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
