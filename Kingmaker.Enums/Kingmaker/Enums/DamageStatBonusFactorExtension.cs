using System;

namespace Kingmaker.Enums;

public static class DamageStatBonusFactorExtension
{
	public static float GetValue(this DamageStatBonusFactor factor)
	{
		return factor switch
		{
			DamageStatBonusFactor.Half => 0.5f, 
			DamageStatBonusFactor.One => 1f, 
			DamageStatBonusFactor.Two => 2f, 
			DamageStatBonusFactor.Three => 3f, 
			_ => throw new ArgumentOutOfRangeException("factor", factor, null), 
		};
	}
}
