using System;
using System.Collections.Generic;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("9977db55d51a44c1bd1b1ff1d278c026")]
public class PetOwnerPriorityConfig : BlueprintScriptableObject
{
	[Serializable]
	public struct PetOwnerSpecificPriority
	{
		[SerializeField]
		public PetOwnerPriority PriorityType;

		[SerializeField]
		[ShowIf("m_WithFeature")]
		public BlueprintUnitFactReference FeatureFilter;

		private bool m_WithFeature => PriorityType == PetOwnerPriority.CompanionWithFeature;
	}

	[SerializeField]
	public List<PetOwnerSpecificPriority> PriorityOrder = new List<PetOwnerSpecificPriority>();
}
