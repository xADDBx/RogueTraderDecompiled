using Kingmaker.Blueprints.Root.Strings.GameLog;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageDamage : CombatMessageBase
{
	public int Amount;

	public Sprite Sprite;

	public bool IsCritical;

	public bool IsImmune;

	public bool IsEnemy;

	public Vector3 SourcePosition;

	public Vector3 TargetPosition;

	public override string GetText()
	{
		string text = Amount.ToString();
		if (IsCritical)
		{
			return text + "!";
		}
		if (IsImmune)
		{
			return text + " " + GameLogStrings.Instance.Damage.DamageImmune.Text;
		}
		return text;
	}

	public override Color? GetColor()
	{
		return IsEnemy ? Game.Instance.BlueprintRoot.UIConfig.CombatTextColors.DamageColorEnemy : Game.Instance.BlueprintRoot.UIConfig.CombatTextColors.DamageColorAlly;
	}

	public override Sprite GetSprite()
	{
		return Sprite;
	}

	public override bool GetAttention()
	{
		return IsCritical;
	}
}
