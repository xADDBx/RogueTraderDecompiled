using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints.Root;

[TypeId("b581b0569ea64bd0b0cfcdfe4b3043f2")]
public class BlueprintScrapRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintScrapRoot>
	{
	}

	public int InitScrap = 200;

	public int ScrapToRegenOneHp = 3;

	public int ScrapToAttunePostAbility = 50;
}
