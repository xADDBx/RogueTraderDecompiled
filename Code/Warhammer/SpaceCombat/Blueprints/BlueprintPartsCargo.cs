using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("a32b5a614b894f13a9f07af4224f3403")]
public class BlueprintPartsCargo : BlueprintCargo
{
	[Serializable]
	public enum PartTypes
	{
		Cohitators,
		Adamantite,
		PowerCores,
		Machinery,
		Explosive
	}

	[Serializable]
	public new class Reference : BlueprintReference<BlueprintPartsCargo>
	{
	}

	[SerializeField]
	public PartTypes Type;
}
