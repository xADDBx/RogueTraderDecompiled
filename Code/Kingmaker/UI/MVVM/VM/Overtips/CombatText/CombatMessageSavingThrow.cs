using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageSavingThrow : CombatMessageBase
{
	public bool Passed;

	public string Reason;

	public Sprite Sprite;

	public StatType StatType;

	public int Roll;

	public int DC;

	public override string GetText()
	{
		string text = Reason;
		if (Passed)
		{
			string text2 = string.Format(UIStrings.Instance.CombatTexts.ThrowSave, Game.Instance.BlueprintRoot.LocalizedTexts.Stats.GetText(StatType));
			text = ((!text.Empty()) ? (text + " - " + text2) : text2);
		}
		if (TurnController.IsInTurnBasedCombat())
		{
			text = UIStrings.Instance.CombatTexts.GetTbmCombatText(text, Roll, DC);
		}
		return text;
	}

	public override bool GetAttention()
	{
		return true;
	}

	public override Sprite GetSprite()
	{
		return Sprite;
	}
}
