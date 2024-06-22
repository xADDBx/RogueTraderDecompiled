using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("37981efdee2344369704cf49ac049657")]
[PlayerUpgraderAllowed(false)]
public class ModifyWoundDamagePerTurnThresholdHp : GameAction
{
	[Tooltip("Wounds threshold hp for player's party will be greater on value percent")]
	public int WoundDamagePerTurnThresholdHPFractionModifier;

	public override string GetCaption()
	{
		return $"Wounds threshold hp for player's party will be greater on {WoundDamagePerTurnThresholdHPFractionModifier} percent";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.TraumasModification.WoundDamagePerTurnThresholdHPFractionModifier += WoundDamagePerTurnThresholdHPFractionModifier;
	}
}
