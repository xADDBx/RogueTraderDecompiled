using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Visual.Critters;
using Newtonsoft.Json;
using StateHasher.Core;

namespace Kingmaker.Blueprints;

[TypeId("923b2d325a4900b49af56a5ac18ceceb")]
public class BlueprintPet : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class PetReference : BlueprintReference<BlueprintPet>
	{
	}

	[JsonProperty]
	public BlueprintUnitReference unit;

	[JsonProperty]
	public PetType type;

	[JsonProperty]
	public FollowerSettings FollowSettings;
}
