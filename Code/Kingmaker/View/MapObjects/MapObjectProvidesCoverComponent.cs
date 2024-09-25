using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[RequireComponent(typeof(MapObjectView))]
[KnowledgeDatabaseID("eb8d70c85ff8466286d63ba2fc59b864")]
public class MapObjectProvidesCoverComponent : EntityPartComponent<MapObjectProvidesCoverPart, MapObjectForcedCoverSettings>
{
}
