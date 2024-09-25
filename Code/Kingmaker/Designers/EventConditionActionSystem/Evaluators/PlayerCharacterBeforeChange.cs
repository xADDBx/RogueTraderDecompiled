using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("de59b74e8aed44d097d4a8e88f7df525")]
public class PlayerCharacterBeforeChange : AbstractUnitEvaluator
{
	public override string GetCaption()
	{
		return "real player character before changes";
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Game.Instance.Player.MainCharacterOriginal.Entity.ToBaseUnitEntity();
	}
}
