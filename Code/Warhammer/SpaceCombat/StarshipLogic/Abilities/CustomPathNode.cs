using Pathfinding;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

public class CustomPathNode
{
	public GraphNode Node;

	public int Direction;

	public CustomPathNode Parent;

	public GameObject Marker;
}
