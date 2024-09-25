using System;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextDurationValue
{
	[HideInInspector]
	public ContextValue BonusValue;

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
