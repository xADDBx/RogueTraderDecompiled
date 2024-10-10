using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("5071af137ae772f459a84bfb952f644f")]
public class AbilityCustomStarshipNewHeading : AbilityCustomLogic, ICustomShipPathProvider
{
	[SerializeField]
	private BlueprintBuffReference m_VariationBuff;

	[SerializeField]
	private BuffEndCondition buffDuration = BuffEndCondition.TurnEndOrCombatEnd;

	[SerializeField]
	private ActionList Actions;

	public BlueprintBuff VariationBuff => m_VariationBuff?.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (context.MaybeCaster is StarshipEntity starshipEntity)
		{
			starshipEntity.Buffs.Add(VariationBuff, new BuffDuration(null, buffDuration));
			using (context.GetDataScope((TargetWrapper)starshipEntity))
			{
				Actions.Run();
			}
			yield return null;
			yield return new AbilityDeliveryTarget(target);
		}
	}

	public Dictionary<GraphNode, CustomPathNode> GetCustomPath(StarshipEntity starship, Vector3 position, Vector3 direction)
	{
		if (starship.Buffs.Contains(VariationBuff))
		{
			return null;
		}
		Dictionary<GraphNode, CustomPathNode> dictionary = new Dictionary<GraphNode, CustomPathNode>();
		PartStarshipNavigation optional = starship.GetOptional<PartStarshipNavigation>();
		StarshipMovementVariation starshipMovementVariation = VariationBuff?.GetComponents<StarshipMovementVariation>().FirstOrDefault();
		if (starshipMovementVariation != null)
		{
			optional.OverrideTurnAngles(new ShipPath.TurnAngleType[1] { starshipMovementVariation.NextTurnAngle });
		}
		optional.UpdateReachableTiles_Blocking();
		optional.ResetCustomOverrides();
		foreach (ShipPath.DirectionalPathNode rawReachableTile in optional.RawReachableTiles)
		{
			CustomGridNodeBase node = rawReachableTile.node;
			CustomPathNode customPathNode = new CustomPathNode
			{
				Node = node,
				Direction = rawReachableTile.direction
			};
			if (rawReachableTile.parent != null && dictionary.TryGetValue(rawReachableTile.parent.node, out var value))
			{
				customPathNode.Parent = value;
			}
			if (rawReachableTile.parent != null && !dictionary.ContainsKey(node))
			{
				dictionary.Add(node, customPathNode);
			}
		}
		return dictionary;
	}
}
