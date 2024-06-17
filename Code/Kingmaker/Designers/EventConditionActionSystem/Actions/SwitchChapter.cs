using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("fa14df2d3ecc4dcbb7bf93dde87525a1")]
public class SwitchChapter : GameAction
{
	public int Chapter;

	public override string GetCaption()
	{
		return $"Switch Chapter ({Chapter})";
	}

	public override void RunAction()
	{
		Game.Instance.Player.ChangeChapter(Chapter);
	}
}
