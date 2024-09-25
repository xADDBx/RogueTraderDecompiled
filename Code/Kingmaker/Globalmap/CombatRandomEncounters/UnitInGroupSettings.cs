using System;
using Kingmaker.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Globalmap.CombatRandomEncounters;

[Serializable]
public class UnitInGroupSettings
{
	public BlueprintUnitReference Unit;

	[FormerlySerializedAs("Count")]
	public int UnitsCount;

	[SerializeField]
	public bool IsMandatoryInGroup;
}
