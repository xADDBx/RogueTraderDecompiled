using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("4748805e9e9351444a59978e21b7352e")]
public class StarshipLockEnemyPost : UnitFactComponentDelegate, ITurnBasedModeHandler, ISubscriber, IHashable
{
	[Tooltip("Random range of rounds to lock post")]
	public int lockTurnsMin = 1;

	[Tooltip("Random range of rounds to lock post")]
	public int lockTurnsMax = 5;

	[Tooltip("How many lock attempts to do")]
	public int lockNum = 1;

	[SerializeField]
	private BlueprintBuffReference[] m_LockBuffs;

	public int LockBuffsCnt
	{
		get
		{
			BlueprintBuffReference[] lockBuffs = m_LockBuffs;
			if (lockBuffs == null)
			{
				return 0;
			}
			return lockBuffs.Length;
		}
	}

	public BlueprintBuff LockBuff(int idx)
	{
		return m_LockBuffs?.Get(idx);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			return;
		}
		StarshipEntity randomEnemy = GetRandomEnemy();
		if (randomEnemy != null)
		{
			for (int i = 0; i < lockNum; i++)
			{
				BlockRandomPost(randomEnemy);
			}
		}
	}

	private void BlockRandomPost(StarshipEntity starship)
	{
		int num = PFStatefulRandom.SpaceCombat.Range(lockTurnsMin, lockTurnsMax + 1);
		int num2 = num + 1;
		int rounds = num;
		BlueprintBuff buffRef = LockBuff(PFStatefulRandom.SpaceCombat.Range(0, LockBuffsCnt));
		Post post = starship.Hull.Posts.Find((Post p) => p.PostType == buffRef.GetComponent<WarhammerBlockStarshipPost>()?.Post);
		if (post == null)
		{
			return;
		}
		if (post.IsBlocked)
		{
			if (post.BlockingBuff.DurationInRounds >= num2)
			{
				return;
			}
			UnblockPost(starship, post);
		}
		starship.Buffs.Add(buffRef, base.Context, new BuffDuration(new Rounds(num2), BuffEndCondition.CombatEnd));
		foreach (Ability item in post.CurrentAbilities())
		{
			starship.GetAbilityCooldownsOptional()?.StartAutonomousCooldown(item.Blueprint, rounds);
		}
	}

	private void UnblockPost(StarshipEntity starship, Post post)
	{
		foreach (Ability item in post.CurrentAbilities())
		{
			starship.GetAbilityCooldownsOptional()?.RemoveAutonomousCooldown(item.Blueprint);
		}
		starship.Buffs.Remove(post.BlockingBuff);
	}

	private StarshipEntity GetRandomEnemy()
	{
		List<StarshipEntity> list = new List<StarshipEntity>();
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			if (allBaseAwakeUnit.IsInCombat && allBaseAwakeUnit.IsEnemy(base.Owner) && allBaseAwakeUnit is StarshipEntity item)
			{
				list.Add(item);
			}
		}
		return list.Random(PFStatefulRandom.Mechanics);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
