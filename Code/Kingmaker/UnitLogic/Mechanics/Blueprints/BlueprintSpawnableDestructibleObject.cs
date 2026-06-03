using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Owlcat.QA.Validation;

namespace Kingmaker.UnitLogic.Mechanics.Blueprints;

[TypeId("2e5ea1fb50884d49ab24a5ab11183c04")]
public class BlueprintSpawnableDestructibleObject : BlueprintScriptableObject
{
	[ValidateNotNull]
	public DestructibleObjectViewLink Prefab;
}
