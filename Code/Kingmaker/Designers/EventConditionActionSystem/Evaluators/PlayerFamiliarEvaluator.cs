using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("2551dc9349904fc9a39f901144cc64a2")]
public class PlayerFamiliarEvaluator : AbstractFamiliarEvaluator
{
	protected override BaseUnitEntity Leader => Game.Instance.Player.MainCharacterEntity;

	public override string GetCaption()
	{
		return "Familiar of Player";
	}
}
