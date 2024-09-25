using System.Collections.Generic;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartFollowedByUnits : BaseUnitPart, IHashable
{
	public readonly HashSet<AbstractUnitEntity> Followers = new HashSet<AbstractUnitEntity>();

	public readonly Dictionary<AbstractUnitEntity, FollowerAction> FollowerDesiredActions = new Dictionary<AbstractUnitEntity, FollowerAction>();

	public readonly Dictionary<AbstractUnitEntity, FollowerActionType> FollowersActionTypesTemp = new Dictionary<AbstractUnitEntity, FollowerActionType>();

	public bool ForceRefresh { get; set; }

	public double LastRefreshTime { get; set; }

	public Vector3 LastKnownDestination { get; set; }

	public FollowerAction? GetFollowerAction(AbstractUnitEntity unit)
	{
		if (!FollowerDesiredActions.TryGetValue(unit, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<AbstractUnitEntity> GetActiveFollowers()
	{
		foreach (AbstractUnitEntity follower in Followers)
		{
			if (!follower.IsSleeping && !follower.LifeState.IsDead)
			{
				yield return follower;
			}
		}
	}

	public void AddFollower(AbstractUnitEntity follower)
	{
		Followers.Add(follower);
		ForceRefresh = true;
	}

	public void RemoveFollower(AbstractUnitEntity follower)
	{
		Followers.Remove(follower);
		FollowerDesiredActions.Remove(follower);
		if (Followers.Count < 1)
		{
			base.Owner.Remove<UnitPartFollowedByUnits>();
		}
	}

	public void ClearCache()
	{
		ForceRefresh = true;
		FollowerDesiredActions.Clear();
		LastKnownDestination = Vector3.zero;
	}

	public void Cleanup()
	{
		Followers.RemoveWhere((AbstractUnitEntity u) => u?.GetOptional<UnitPartFollowUnit>() == null);
		if (Followers.Count < 1)
		{
			base.Owner.Remove<UnitPartFollowedByUnits>();
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
