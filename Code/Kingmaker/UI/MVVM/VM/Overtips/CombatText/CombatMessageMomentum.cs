using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageMomentum : CombatMessageBase
{
	public int Count;

	public override string GetText()
	{
		return UIStrings.Instance.CombatTexts.Morale.Text + ": " + UIUtility.AddSign(Count);
	}

	public override Color? GetColor()
	{
		return (Count > 0) ? Game.Instance.BlueprintRoot.UIConfig.CombatTextColors.MomentumIncreaseColor : Game.Instance.BlueprintRoot.UIConfig.CombatTextColors.MomentumDecreaseColor;
	}
}
