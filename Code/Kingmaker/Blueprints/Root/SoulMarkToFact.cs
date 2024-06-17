using System;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class SoulMarkToFact
{
	[SerializeField]
	public SoulMarkDirection SoulMarkDirection;

	[SerializeField]
	private BlueprintFeature.Reference m_SoulMarkBlueprint;

	public BlueprintSoulMark SoulMarkBlueprint => m_SoulMarkBlueprint?.Get() as BlueprintSoulMark;
}
