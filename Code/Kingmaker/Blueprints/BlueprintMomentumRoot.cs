using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("748aaa6e3ebc48a09c1a59c6df8e45a0")]
public class BlueprintMomentumRoot : BlueprintScriptableObject
{
	[SerializeField]
	private BlueprintMomentumGroupReference m_PartyGroup;

	[SerializeField]
	private BlueprintMomentumGroupReference m_DefaultEnemyGroup;

	[SerializeField]
	private PropertyCalculator m_DesperateMeasureThreshold;

	public float MinorTraumaDesperateMeasuresThresholdFactor = 1.5f;

	public float MajorTraumaDesperateMeasuresThresholdFactor = 2f;

	public int StartingMomentum = 100;

	public int MinimalMomentum;

	public int MaximalMomentum = 200;

	public int HeroicActThreshold = 175;

	public PrefabLink HeroicActReachedFX;

	public PrefabLink DesperateMeasuresReachedFX;

	[SerializeField]
	private BlueprintBuffReference m_HeroicActBuff;

	[SerializeField]
	private BlueprintBuffReference m_DesperateMeasureBuff;

	public BlueprintMomentumGroup PartyGroup => m_PartyGroup;

	public BlueprintMomentumGroup DefaultEnemyGroup => m_DefaultEnemyGroup;

	public BlueprintBuff HeroicActBuff => m_HeroicActBuff;

	public BlueprintBuff DesperateMeasureBuff => m_DesperateMeasureBuff;

	public PropertyCalculator DesperateMeasureThreshold => m_DesperateMeasureThreshold;
}
