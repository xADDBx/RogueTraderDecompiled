using System;
using System.Collections.Generic;
using Kingmaker.Enums;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class MapObjectForcedCoverSettings
{
	[Serializable]
	public class DestructionStageToCover
	{
		public DestructionStage DestructionStage;

		public LosCalculations.CoverType CoverType;
	}

	[SerializeField]
	public List<DestructionStageToCover> DestructionStageToCovers = new List<DestructionStageToCover>();

	public Dictionary<DestructionStage, LosCalculations.CoverType> StageToCoverTypeMap = new Dictionary<DestructionStage, LosCalculations.CoverType>();

	[HideInInspector]
	public LosCalculations.CoverType CoverType;
}
