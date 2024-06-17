using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("eeaa8d85bf3588d4f9123384bd993309")]
public class CutscenesRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<CutscenesRoot>
	{
	}

	[SerializeField]
	private bool m_FadeScreenOnSkip = true;

	[SerializeField]
	private float m_TimeScaleOnSkip = 10f;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_AttackSingle;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintAbilityReference m_AttackBurst;

	public bool FadeScreenOnSkip => m_FadeScreenOnSkip;

	public float TimeScaleOnSkip => m_TimeScaleOnSkip;

	public BlueprintAbility AttackSingle => m_AttackSingle;

	public BlueprintAbility AttackBurst => m_AttackBurst;
}
