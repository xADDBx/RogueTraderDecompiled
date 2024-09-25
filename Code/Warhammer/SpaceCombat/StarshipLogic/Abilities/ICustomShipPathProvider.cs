using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Pathfinding;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

public interface ICustomShipPathProvider
{
	Dictionary<GraphNode, CustomPathNode> GetCustomPath(StarshipEntity starship, Vector3 postion, Vector3 direction);
}
