using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("4595d7197ceb4578b0234d77dca7e740")]
public class CurrentStarSystem : Condition
{
	public BlueprintStarSystemMapReference StarSystem;

	protected override string GetConditionCaption()
	{
		return string.Format("Current star system is " + StarSystem?.Get()?.Name);
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.CurrentStarSystem == StarSystem.Get();
	}
}
