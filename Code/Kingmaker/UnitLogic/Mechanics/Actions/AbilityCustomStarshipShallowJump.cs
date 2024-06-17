using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;
using Kingmaker.Visual.Particles;
using Pathfinding;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("90148a340eede5946ade283656c21fe1")]
public class AbilityCustomStarshipShallowJump : AbilityCustomLogic, ICustomShipPathProvider, IAbilityCasterRestriction
{
	public int MinDistance;

	public int MaxDistance;

	public float ArrivalDelay;

	public PrefabLink ExitPortalPrefab;

	public int jumpedOverPctDamageMin;

	public int jumpedOverPctDamageMax;

	[SerializeField]
	private ActionList StarshipActionsOnFinish;

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (!(maybeCaster is StarshipEntity starship))
		{
			yield break;
		}
		var (list, jumpedOverUnits) = GetJumpData(starship, starship.Position, starship.Forward, createJumpedOverList: true);
		if (list.Count == 0)
		{
			yield break;
		}
		int index = PFStatefulRandom.Mechanics.Range(0, list.Count);
		Vector3 exitPosition = list[index].Vector3Position;
		if (ExitPortalPrefab != null)
		{
			FxHelper.SpawnFxOnPoint(ExitPortalPrefab.Load(), exitPosition);
		}
		double startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
		while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)(ArrivalDelay / 2f))
		{
			yield return null;
		}
		if (jumpedOverPctDamageMax > 0)
		{
			foreach (StarshipEntity item in jumpedOverUnits)
			{
				int maxHitPoints = item.Health.MaxHitPoints;
				int min = maxHitPoints * jumpedOverPctDamageMin / 100;
				int max = maxHitPoints * jumpedOverPctDamageMax / 100;
				DamageData damage = new DamageData(DamageType.Warp, min, max);
				Rulebook.Trigger(new RuleDealDamage(starship, item, damage));
			}
		}
		while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)ArrivalDelay)
		{
			yield return null;
		}
		starship.Position = exitPosition;
		starship.View.AgentASP.Blocker.BlockAtCurrentPosition();
		yield return null;
		using (context.GetDataScope((TargetWrapper)starship))
		{
			StarshipActionsOnFinish.Run();
		}
		yield return new AbilityDeliveryTarget(exitPosition);
	}

	public Dictionary<GraphNode, CustomPathNode> GetCustomPath(StarshipEntity starship, Vector3 position, Vector3 direction)
	{
		Dictionary<GraphNode, CustomPathNode> dictionary = new Dictionary<GraphNode, CustomPathNode>();
		foreach (CustomGridNodeBase item in GetJumpData(starship, position, direction, createJumpedOverList: false).availableNodes)
		{
			CustomPathNode value = new CustomPathNode
			{
				Node = item,
				Direction = CustomGraphHelper.GuessDirection(direction),
				Parent = null,
				Marker = null
			};
			dictionary.Add(item, value);
		}
		return dictionary;
	}

	private (List<CustomGridNodeBase> availableNodes, HashSet<StarshipEntity> jumpedOverUnits) GetJumpData(StarshipEntity starship, Vector3 position, Vector3 v3direction, bool createJumpedOverList)
	{
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(position).node;
		int direction = CustomGraphHelper.GuessDirection(v3direction);
		int width = starship.SizeRect.Width;
		List<CustomGridNodeBase> list = new List<CustomGridNodeBase>();
		IEnumerable<StarshipEntity> source = (createJumpedOverList ? Game.Instance.State.AllAwakeUnits.Where((AbstractUnitEntity p) => p is StarshipEntity).Cast<StarshipEntity>() : null);
		HashSet<StarshipEntity> hashSet = (createJumpedOverList ? new HashSet<StarshipEntity>() : null);
		int num = AdjustDistance(MinDistance);
		int num2 = AdjustDistance(MaxDistance);
		CustomGridNodeBase node = customGridNodeBase;
		for (int i = 1; i <= num2; i++)
		{
			for (int j = 0; j < width; j++)
			{
				node = node.GetNeighbourAlongDirection(direction);
			}
			if (starship.Navigation.CanStand(node, direction))
			{
				if (i >= num)
				{
					list.Add(node);
				}
			}
			else if (createJumpedOverList)
			{
				hashSet.UnionWith(source.Where((StarshipEntity ship) => ship.IsUnitPositionContainsNode(ship.Position, node)));
			}
		}
		return (availableNodes: list, jumpedOverUnits: hashSet);
		int AdjustDistance(int dist)
		{
			if (direction >= 4)
			{
				return (dist + 1) * 2 / 3;
			}
			return dist;
		}
	}

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		if (!(caster is StarshipEntity starshipEntity))
		{
			return false;
		}
		return GetJumpData(starshipEntity, starshipEntity.Position, starshipEntity.Forward, createJumpedOverList: false).availableNodes.Count > 0;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return LocalizedTexts.Instance.Reasons.NotAllowedCellToCast;
	}
}
