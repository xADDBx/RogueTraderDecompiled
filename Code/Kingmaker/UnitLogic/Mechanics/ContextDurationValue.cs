using System;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextDurationValue
{
	public ContextValue RoundsValue;

	public Rounds Calculate(MechanicsContext context)
	{
		return RoundsValue.Calculate(context).Rounds();
	}

	public override string ToString()
	{
		return RoundsValue.ToString() + " Rounds";
	}
}
