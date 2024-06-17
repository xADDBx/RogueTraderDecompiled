using System;
using Code.Visual.Animation;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.View.Animation;

[Serializable]
public class UnitAnimationSettings
{
	public float MovementSpeedCoeff = 1f;

	public bool OverrideSlowWalk;

	[ConditionalShow("OverrideSlowWalk")]
	public float SlowWalkCoeff = 1f;

	public bool OverrideSlowWalkNonCombat;

	[ConditionalShow("OverrideSlowWalkNonCombat")]
	public float SlowWalkNonCombatCoeff = 1f;

	public bool OverrideNormal;

	[ConditionalShow("OverrideNormal")]
	public float NormalCoeff = 1f;

	public bool OverrideNormalNonCombat;

	[ConditionalShow("OverrideNormalNonCombat")]
	public float NormalNonCombatCoeff = 1f;

	public bool OverrideCharge;

	[ConditionalShow("OverrideCharge")]
	public float ChargeCoeff = 1f;

	public bool OverrideChargeNonCombat;

	[ConditionalShow("OverrideChargeNonCombat")]
	public float ChargeNonCombatCoeff = 1f;

	public bool OverrideStealth;

	[ConditionalShow("OverrideStealth")]
	public float StealthCoeff = 1f;

	public bool OverrideStealthNonCombat;

	[ConditionalShow("OverrideStealthNonCombat")]
	public float StealthNonCombatCoeff = 1f;

	public float GetCoeff(WalkSpeedType type, bool inCombat)
	{
		switch (type)
		{
		case WalkSpeedType.Walk:
			if (inCombat || !OverrideNormalNonCombat)
			{
				if (!OverrideNormal)
				{
					return MovementSpeedCoeff;
				}
				return NormalCoeff;
			}
			return NormalNonCombatCoeff;
		case WalkSpeedType.Run:
			if (inCombat || !OverrideChargeNonCombat)
			{
				if (!OverrideCharge)
				{
					return MovementSpeedCoeff;
				}
				return ChargeCoeff;
			}
			return ChargeNonCombatCoeff;
		case WalkSpeedType.Crouch:
			if (inCombat || !OverrideStealthNonCombat)
			{
				if (!OverrideStealth)
				{
					return MovementSpeedCoeff;
				}
				return StealthCoeff;
			}
			return StealthNonCombatCoeff;
		default:
			return 1f;
		}
	}
}
