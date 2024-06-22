using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View.MapObjects.Traps;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(TrapObjectView))]
[KnowledgeDatabaseID("9c46a609f895bc64e937d4aadcea6eb1")]
public class DisableTrapInteractionComponent : InteractionComponent<DisableTrapInteractionPart, InteractionSettings>
{
}
