using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Utility.DotNetExtensions;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("a92a1c3c61b641ceac429a51dba8acc2")]
public class PostsAreBroken : Condition
{
	protected override string GetConditionCaption()
	{
		return "Posts are all supreme commanders";
	}

	protected override bool CheckCondition()
	{
		List<Post> posts = Game.Instance.Player.PlayerShip.GetHull().Posts;
		if (!posts.Empty())
		{
			return posts[0].PostType == posts[1].PostType;
		}
		return false;
	}
}
