using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;

namespace Kingmaker.Visual.Particles.Blueprints;

[TypeId("2c0480568ae94c9ba720eb332c778f3e")]
public class BlueprintFxLocatorGroup : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintFxLocatorGroup>
	{
	}

	public string[] TransformNames = new string[0];
}
