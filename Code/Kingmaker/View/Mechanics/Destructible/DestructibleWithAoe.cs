using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Destructible;

[RequireComponent(typeof(DestructibleEntityView))]
[KnowledgeDatabaseID("a976b81829c546e896868228f3176048")]
public class DestructibleWithAoe : EntityPartComponent<ViewPartDestructionWithAoe, DestructibleWithAoeSettings>
{
}
