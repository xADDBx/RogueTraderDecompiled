using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints;

[TypeId("8e1425e0c2d42aa43948102257600faf")]
public class BlueprintCreature : BlueprintUnit
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintCreature>
	{
	}
}
