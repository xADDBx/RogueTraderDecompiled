using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("f989ed3c11784a4292ab1934fab712f7")]
[PlayerUpgraderAllowed(false)]
public class GainPF : GameAction
{
	public float Value;

	public override string GetCaption()
	{
		return $"Change total PF on {Value}";
	}

	protected override void RunAction()
	{
		ProfitFactorModifierType type = ((base.Owner is BlueprintCue) ? ProfitFactorModifierType.Cue : ((base.Owner is BlueprintAnswer) ? ProfitFactorModifierType.Answer : ProfitFactorModifierType.Other));
		Game.Instance.Player.ProfitFactor.AddModifier(Value, type, base.Owner as BlueprintScriptableObject);
	}
}
