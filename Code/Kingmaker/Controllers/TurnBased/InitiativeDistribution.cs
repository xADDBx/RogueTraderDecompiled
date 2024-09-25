using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[Serializable]
public class InitiativeDistribution
{
	private enum CombatSideType
	{
		Player,
		[UsedImplicitly]
		Npc
	}

	[Serializable]
	public class Range
	{
		public int Min = 1;

		public int Max = 1;
	}

	[SerializeField]
	private CombatSideType m_FirstTurn;

	public Range[] Ranges = new Range[0];

	public bool StartsFromPlayer => m_FirstTurn == CombatSideType.Player;

	[CanBeNull]
	public static InitiativeDistribution GetRandom()
	{
		return Root.WH.CombatRoot.InitiativeDistributions?.Random(PFStatefulRandom.Mechanics);
	}
}
