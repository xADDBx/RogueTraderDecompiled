using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints.Root;

[TypeId("474c06b76471f274fb4ca7b1624c86ce")]
public class BlueprintProfitFactorRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintProfitFactorRoot>
	{
	}

	public float InitialProfitFactor = 30f;
}
