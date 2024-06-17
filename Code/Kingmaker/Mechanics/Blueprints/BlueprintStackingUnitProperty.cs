using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Mechanics.Blueprints;

[TypeId("bb46e42753394617a0d1cf8bed127eda")]
public class BlueprintStackingUnitProperty : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintStackingUnitProperty>
	{
	}
}
