using System.Collections.Generic;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartFollowedByUnits : BaseUnitPart, IUnitCommandStartHandler<EntitySubscriber>, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandStartHandler, EntitySubscriber>, IHashable
{
	public readonly HashSet<AbstractUnitEntity> Followers = new HashSet<AbstractUnitEntity>();

	public readonly HashSet<AbstractUnitEntity> IndependentFollowers = new HashSet<AbstractUnitEntity>();

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
			if (!follower.IsSleeping && !follower.LifeState.IsDeadOrUnconscious)
			{
				yield return follower;
			}
		}
		foreach (AbstractUnitEntity independentFollower in IndependentFollowers)
		{
			if (!independentFollower.LifeState.IsDeadOrUnconscious)
			{
				yield return independentFollower;
			}
		}
	}

	public IEnumerable<AbstractUnitEntity> GetIndependentActiveFollowers()
	{
		foreach (AbstractUnitEntity independentFollower in IndependentFollowers)
		{
			if (!independentFollower.IsSleeping && !independentFollower.LifeState.IsDeadOrUnconscious)
			{
				yield return independentFollower;
			}
		}
	}

	public void AddFollower(AbstractUnitEntity follower)
	{
		Followers.Add(follower);
		ForceRefresh = true;
	}

	public void AddIndependentFollower(AbstractUnitEntity follower)
	{
		IndependentFollowers.Add(follower);
		ForceRefresh = true;
	}

	public void RemoveFollower(AbstractUnitEntity follower)
	{
		Followers.Remove(follower);
		FollowerDesiredActions.Remove(follower);
		if (Followers.Count < 1 && IndependentFollowers.Count < 1)
		{
			base.Owner.Remove<UnitPartFollowedByUnits>();
		}
	}

	public void RemoveIndependentFollower(AbstractUnitEntity follower)
	{
		IndependentFollowers.Remove(follower);
		FollowerDesiredActions.Remove(follower);
		if (Followers.Count < 1 && IndependentFollowers.Count < 1)
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
		IndependentFollowers.RemoveWhere((AbstractUnitEntity u) => u?.GetOptional<UnitPartFollowUnit>() == null);
		if (Followers.Count < 1 && IndependentFollowers.Count < 1)
		{
			base.Owner.Remove<UnitPartFollowedByUnits>();
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.IsMoveUnit)
		{
			ForceRefresh = true;
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
