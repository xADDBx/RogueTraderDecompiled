using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageStarshipDamage : CombatMessageDamage
{
	public bool IsShieldDamage;

	public override Color? GetColor()
	{
		return IsShieldDamage ? Game.Instance.BlueprintRoot.UIConfig.CombatTextColors.StarshipShieldsDamageColor : (IsCritical ? Game.Instance.BlueprintRoot.UIConfig.CombatTextColors.StarshipHullCriticalDamageColor : Game.Instance.BlueprintRoot.UIConfig.CombatTextColors.StarshipHullDamageColor);
	}
}
