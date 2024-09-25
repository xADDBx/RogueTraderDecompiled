using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules.Starships;

public class RuleStarshipCalculateHitLocation : RulebookTargetEvent<StarshipEntity, StarshipEntity>
{
	public StarshipHitLocation ResultHitLocation { get; private set; }

	public RuleStarshipCalculateHitLocation([NotNull] StarshipEntity initiator, StarshipEntity target)
		: base(initiator, target)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		int num = CustomGraphHelper.GuessDirection(base.Target.Forward);
		Vector3 vector;
		Vector3 vector2;
		if (UnitPredictionManager.RealHologramPosition.HasValue)
		{
			vector = UnitPredictionManager.RealHologramPosition.Value;
			vector2 = UnitPredictionManager.Instance.CurrentUnitDirection;
		}
		else
		{
			vector = base.Initiator.Position;
			vector2 = base.Initiator.Forward;
		}
		Vector3 vector3 = vector - base.Target.Position;
		if (vector3.magnitude < GraphParamsMechanicsCache.GridCellSize / 2f)
		{
			vector3 = -vector2;
		}
		int num2 = CustomGraphHelper.GuessDirection(vector3.normalized);
		if (num2 == num)
		{
			ResultHitLocation = StarshipHitLocation.Fore;
		}
		else if (num2 == CustomGraphHelper.LeftNeighbourDirection[num] || num2 == CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[num]] || num2 == CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[num]]])
		{
			ResultHitLocation = StarshipHitLocation.Port;
		}
		else if (num2 == CustomGraphHelper.RightNeighbourDirection[num] || num2 == CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[num]] || num2 == CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[num]]])
		{
			ResultHitLocation = StarshipHitLocation.Starboard;
		}
		else
		{
			ResultHitLocation = StarshipHitLocation.Aft;
		}
	}
}
