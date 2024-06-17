using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Armors;

[TypeId("d1f2ee8fa17258e4983d524d31c377ae")]
public class BlueprintWarhammerArmorType : BlueprintScriptableObject
{
	public WarhammerArmorCategory ArmorCategory = WarhammerArmorCategory.Light;

	[SerializeField]
	[Range(0f, 100f)]
	private int m_DamageAbsorbtionPct;

	[SerializeField]
	private int m_DamageDeflection;

	[SerializeField]
	private int m_RangedHitChanceBonus;

	[SerializeField]
	private float m_DodgeMod = 1f;

	[SerializeField]
	private int m_MovePointsAdjustment;

	public int DamageAbsorbtionPct(int itemLevel)
	{
		return m_DamageAbsorbtionPct;
	}

	public int DamageDeflection(int itemLevel)
	{
		return m_DamageDeflection;
	}

	public int RangedHitChanceBonus(int itemLevel)
	{
		return m_RangedHitChanceBonus;
	}

	public float ArmorDodgeModifier(int itemLevel)
	{
		return m_DodgeMod;
	}

	public int MovePointsAdjustment(int itemLevel)
	{
		return m_MovePointsAdjustment;
	}
}
