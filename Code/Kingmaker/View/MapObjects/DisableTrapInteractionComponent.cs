using Kingmaker.View.MapObjects.Traps;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(TrapObjectView))]
public class DisableTrapInteractionComponent : InteractionComponent<DisableTrapInteractionPart, InteractionSettings>
{
}
