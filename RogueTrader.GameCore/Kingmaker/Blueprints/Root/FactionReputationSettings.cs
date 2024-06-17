using System;
using System.Collections.Generic;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class FactionReputationSettings
{
	[Serializable]
	public class FactionReputationLevel
	{
		[SerializeField]
		private int m_ReputationPointsToLevel;

		public int ReputationPointsToLevel => m_ReputationPointsToLevel;

		public FactionReputationLevel(int reputationPointsToLevel)
		{
			m_ReputationPointsToLevel = reputationPointsToLevel;
		}
	}

	[SerializeField]
	[ValidateNotNull]
	private List<FactionReputationLevel> m_ReputationLevelThresholds;

	public IReadOnlyList<FactionReputationLevel> ReputationLevelThresholds => m_ReputationLevelThresholds;
}
