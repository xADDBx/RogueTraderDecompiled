using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("597f5a890d2b68945b0061d6954067b3")]
public class GainCombativity : GameAction
{
	public float Value;

	public override string GetCaption()
	{
		return $"Change total combativity on {Value}";
	}

	protected override void RunAction()
	{
		ProfitFactorModifierType type = ((base.Owner is BlueprintCue) ? ProfitFactorModifierType.Cue : ((base.Owner is BlueprintAnswer) ? ProfitFactorModifierType.Answer : ProfitFactorModifierType.Other));
		Game.Instance.Player.Combativity.AddModifier(Value, type, base.Owner as BlueprintScriptableObject);
	}
}
