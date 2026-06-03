using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints;

[TypeId("37897c92f06a4fd7a073d96f61e6bfc4")]
public class BlueprintPlayerTimer : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintPlayerTimer>
	{
	}
}
