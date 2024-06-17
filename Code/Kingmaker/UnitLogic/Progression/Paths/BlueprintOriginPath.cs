using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Progression.Paths;

[Serializable]
[TypeId("c26e1d660f8b4a66a433c35a0965d037")]
public class BlueprintOriginPath : BlueprintPath
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintOriginPath>
	{
	}
}
