using System;
using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Requirements;

[Serializable]
public class FactionReputation
{
	[SerializeField]
	public FactionType Faction;

	[SerializeField]
	public int MinLevelValue;
}
