using Kingmaker.Code.Utility;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageHealing : CombatMessageBase
{
	public int Amount;

	public Sprite Sprite;

	public override string GetText()
	{
		return UIConstsExtensions.GetValueWithSign(Amount);
	}

	public override Sprite GetSprite()
	{
		return Sprite;
	}

	public override Color? GetColor()
	{
		return Game.Instance.BlueprintRoot.UIConfig.CombatTextColors.HealColor;
	}
}
