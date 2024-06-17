using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageAttackMiss : CombatMessageBase
{
	public AttackResult Result;

	public override string GetText()
	{
		return Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.CombatTexts.GetAvoidText(Result);
	}
}
