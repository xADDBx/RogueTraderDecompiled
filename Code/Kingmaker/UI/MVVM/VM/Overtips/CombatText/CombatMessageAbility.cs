using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageAbility : CombatMessageBase
{
	public string Name;

	public Sprite Sprite;

	public override string GetText()
	{
		return Name;
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
