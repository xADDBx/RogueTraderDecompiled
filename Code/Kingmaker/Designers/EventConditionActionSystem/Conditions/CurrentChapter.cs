using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("56dbbbbbe442d504eb6761bdca7c8066")]
public class CurrentChapter : Condition
{
	public int Chapter;

	protected override string GetConditionCaption()
	{
		return $"Current chapter is ({Chapter})";
	}

	protected override bool CheckCondition()
	{
		if (Game.Instance.Player.Chapter == Chapter)
		{
			return true;
		}
		return false;
	}
}
