using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[TypeId("923b2d325a4900b49af56a5ac18ceceb")]
public class BlueprintPet : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintPet>
	{
	}
}
