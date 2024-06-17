using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Globalmap.Blueprints.SystemMap;

[TypeId("51eab84063e74c40b129fcfbcf2a58c0")]
public class BlueprintStar : BlueprintStarSystemObject
{
	public enum StarType
	{
		Dwarf,
		Giant,
		Unique
	}

	[Serializable]
	public new class Reference : BlueprintReference<BlueprintStar>
	{
	}

	public StarType Type;
}
