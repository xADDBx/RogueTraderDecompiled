using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("026c1d19a5304a40ae7322406f9e2cf8")]
public class FixStarshipPosts : GameAction
{
	public override string GetCaption()
	{
		return "Fix starship posts";
	}

	public override void RunAction()
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		PartStarshipHull hull = playerShip.GetHull();
		_ = hull.Posts;
		hull.Posts.Clear();
		foreach (PostData item in playerShip.Blueprint?.Posts?.EmptyIfNull())
		{
			hull.AddPost(item);
		}
	}
}
