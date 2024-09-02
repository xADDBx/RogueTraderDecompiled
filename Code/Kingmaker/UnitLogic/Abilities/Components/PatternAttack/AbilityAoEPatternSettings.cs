using System;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.PatternAttack;

[Serializable]
public class AbilityAoEPatternSettings : IAbilityAoEPatternProvider
{
	public AoEPattern Pattern;

	public TargetType Targets;

	public bool CastOnSameLevel;

	[Header("Spreading settings")]
	[SerializeField]
	private bool m_IgnoreLos;

	[SerializeField]
	[HideIf("m_IgnoreLos")]
	private bool m_UseMeleeLos;

	[SerializeField]
	private bool m_IgnoreLevelDifference;

	[InfoBox("'Directional' spreading works only with Sector, Cone and Ray patterns")]
	[SerializeField]
	[ShowIf("CanBeDirectional")]
	private bool m_Directional;

	public bool CalculateAttackFromPatternCentre;

	private AoEPattern m_OverridePattern;

	private AoEPattern CurrentPattern => m_OverridePattern ?? Pattern;

	public bool IsIgnoreLos => m_IgnoreLos;

	public bool UseMeleeLos => m_UseMeleeLos;

	public bool IsIgnoreLevelDifference => m_IgnoreLevelDifference;

	public int PatternAngle => CurrentPattern.Angle;

	bool IAbilityAoEPatternProvider.CalculateAttackFromPatternCentre => CalculateAttackFromPatternCentre;

	TargetType IAbilityAoEPatternProvider.Targets => Targets;

	AoEPattern IAbilityAoEPatternProvider.Pattern => CurrentPattern;

	public int Radius => CurrentPattern.Radius;

	private bool CanBeDirectional => CurrentPattern.CanBeDirectional;

	public void OverridePattern(AoEPattern pattern)
	{
		m_OverridePattern = pattern;
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		CustomGridNodeBase actualCastNode;
		return AoEPatternHelper.GetOrientedPattern(ability, ability.Caster, CurrentPattern, this, casterNode, targetNode, CastOnSameLevel, m_Directional, coveredTargetsOnly, out actualCastNode);
	}
}
