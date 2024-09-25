using Kingmaker.Blueprints.Root.Strings;

namespace Kingmaker.UI.MVVM.VM.Overtips.CombatText;

public class CombatMessageAttackOfOpportunity : CombatMessageBase
{
	public override string GetText()
	{
		return UIStrings.Instance.CombatTexts.AttackOfOpportunity;
	}
}
