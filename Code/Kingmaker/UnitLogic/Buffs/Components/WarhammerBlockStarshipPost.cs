using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("7794392710080e741beb48abedb3deb2")]
public class WarhammerBlockStarshipPost : UnitBuffComponentDelegate, IHashable
{
	public bool BlockRandom;

	[HideIf("BlockRandom")]
	public PostType Post;

	protected override void OnActivate()
	{
		base.OnActivate();
		int num = -1;
		List<Post> posts = base.Owner.GetHull().Posts;
		if (posts.Count == 0)
		{
			PFLog.Default.Error(this, "Target has no starship posts");
			return;
		}
		if (BlockRandom)
		{
			num = base.Owner.Random.Range(0, posts.Count);
		}
		else
		{
			for (int i = 0; i < posts.Count; i++)
			{
				if (posts[i].PostType == Post)
				{
					num = i;
				}
			}
		}
		if (num == -1)
		{
			PFLog.Default.Error(this, $"Target has no {Post} post");
		}
		else
		{
			posts[num].BlockBy(base.Buff);
		}
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		foreach (Post post in base.Owner.GetHull().Posts)
		{
			post.Unblock(base.Buff);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
