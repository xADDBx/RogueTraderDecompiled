using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("6d28fb4beac041dca984f5e7d9dbd205")]
public class BlueprintSoulMarksRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintSoulMarksRoot>
	{
	}

	public List<SoulMarkToFact> SoulMarksBaseFacts = new List<SoulMarkToFact>();

	[Tooltip("Rank index at which a soul mark direction blocks all other directions from gaining points")]
	public int BlockingRankIndex = 3;
}
