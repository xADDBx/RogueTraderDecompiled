using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;

namespace Kingmaker.RandomEncounters.Settings;

[TypeId("97b7954cde834804888424988750f212")]
public class BlueprintSpawnableObject : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintSpawnableObject>
	{
	}

	[ValidateNotNull]
	public PrefabLink Prefab;

	public virtual void InitializeObjectView(MapObjectView view)
	{
	}
}
