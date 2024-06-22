using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageCultAmbush : CombatMessageBase
{
	public override string GetText()
	{
		return UIStrings.Instance.CombatTexts.CultAmbush.Text;
	}

	public override Sprite GetSprite()
	{
		return UIConfig.Instance.UIIcons.CultAmbush;
	}
}
