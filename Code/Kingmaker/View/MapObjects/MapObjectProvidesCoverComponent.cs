using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(MapObjectView))]
public class MapObjectProvidesCoverComponent : EntityPartComponent<MapObjectProvidesCoverPart, MapObjectForcedCoverSettings>
{
}
