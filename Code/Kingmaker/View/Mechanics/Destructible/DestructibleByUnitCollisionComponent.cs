using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

[RequireComponent(typeof(DestructibleEntityView))]
[KnowledgeDatabaseID("7e8c2d852e04437ea8ec204a538c722a")]
public class DestructibleByUnitCollisionComponent : EntityPartComponent<DestructibleByUnitCollisionPart, DestructibleByUnitCollisionSettings>
{
}
