using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class CombatTextColors
{
	public Color HealColor = Color.green;

	public Color DamageColorAlly = Color.red;

	public Color DamageColorEnemy = Color.red;

	public Color StarshipShieldsDamageColor = Color.blue;

	public Color StarshipHullDamageColor = Color.red;

	public Color StarshipHullCriticalDamageColor = Color.red;

	public Color MomentumIncreaseColor = Color.blue;

	public Color MomentumDecreaseColor = Color.blue;
}
