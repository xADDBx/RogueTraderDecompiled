using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Customization;

[Serializable]
public class RaceEntry
{
	[SerializeField]
	[FormerlySerializedAs("Race")]
	private BlueprintRaceReference m_Race;

	public float BaseWeight = 1f;

	public float MaleModifier = 1f;

	public float FemaleModifier = 1f;

	public BlueprintRace Race => m_Race?.Get();
}
