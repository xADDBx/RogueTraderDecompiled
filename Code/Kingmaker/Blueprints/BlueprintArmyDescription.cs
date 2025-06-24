using System;
using System.ComponentModel;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("310bf69dec7144cea850fa7ef7239508")]
public class BlueprintArmyDescription : BlueprintScriptableObject
{
	[Serializable]
	public class ArmyStat
	{
		[SerializeField]
		public int Value = 30;

		[SerializeField]
		public bool isProfessional;
	}

	[SerializeField]
	private BlueprintFeatureReference[] m_Features;

	public UnitDifficultyType DifficultyType;

	public ArmyStat WarhammerBallisticSkill;

	public ArmyStat WarhammerWeaponSkill;

	public ArmyStat WarhammerStrength;

	public ArmyStat WarhammerToughness;

	public ArmyStat WarhammerAgility;

	public ArmyStat WarhammerIntelligence;

	public ArmyStat WarhammerWillpower;

	public ArmyStat WarhammerPerception;

	public ArmyStat WarhammerFellowship;

	[SerializeField]
	private bool m_IsHuman;

	[SerializeField]
	private bool m_IsXenos;

	[SerializeField]
	private bool m_IsDaemon;

	[SerializeField]
	private bool m_IsHumanoid;

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}

	public bool IsHuman => m_IsHuman;

	public bool IsXenos => m_IsXenos;

	public bool IsDaemon => m_IsDaemon;

	public bool IsHumanoid => m_IsHumanoid;

	public ArmyStat GetAttributeSettings(StatType statType)
	{
		return statType switch
		{
			StatType.WarhammerBallisticSkill => WarhammerBallisticSkill, 
			StatType.WarhammerWeaponSkill => WarhammerWeaponSkill, 
			StatType.WarhammerStrength => WarhammerStrength, 
			StatType.WarhammerToughness => WarhammerToughness, 
			StatType.WarhammerAgility => WarhammerAgility, 
			StatType.WarhammerIntelligence => WarhammerIntelligence, 
			StatType.WarhammerWillpower => WarhammerWillpower, 
			StatType.WarhammerPerception => WarhammerPerception, 
			StatType.WarhammerFellowship => WarhammerFellowship, 
			_ => throw new InvalidEnumArgumentException("statType", (int)statType, typeof(StatType)), 
		};
	}

	public void Editor_AddFeature(BlueprintFeature feature)
	{
		Array.Resize(ref m_Features, m_Features.Length + 1);
		m_Features[^1] = feature.ToReference<BlueprintFeatureReference>();
	}
}
