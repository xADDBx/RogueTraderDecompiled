using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("822dcce931786aa4d80ba3b89862e9d6")]
[PlayerUpgraderAllowed(false)]
public class PlayerStarship : AbstractUnitEvaluator
{
	public override string GetCaption()
	{
		return "Player Starship";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Game.Instance.Player.PlayerShip;
	}
}
