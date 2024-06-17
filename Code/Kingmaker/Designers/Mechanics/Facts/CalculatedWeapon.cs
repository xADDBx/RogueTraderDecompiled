using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
public class CalculatedWeapon
{
	[SerializeField]
	[FormerlySerializedAs("Weapon")]
	private BlueprintItemWeaponReference m_Weapon;

	public bool ReplacedDamage;

	public DiceFormula DamageFormula;

	public bool ReplacedDamageStat;

	public StatType DamageBonusStat;

	public bool ReplaceDamageWithText;

	private bool m_ScaleDamageByRank;

	private int m_Ranks;

	public BlueprintItemWeapon Weapon => m_Weapon?.Get();

	public DiceFormula FinalDamageFormula()
	{
		if (!m_ScaleDamageByRank)
		{
			return DamageFormula;
		}
		return new DiceFormula(DamageFormula.Rolls * m_Ranks, DamageFormula.Dice);
	}

	public void ScaleDamageByRank(Feature feature)
	{
		m_ScaleDamageByRank = true;
		m_Ranks = feature.GetRank();
	}
}
