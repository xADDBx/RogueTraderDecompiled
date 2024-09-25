using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintPortrait))]
[TypeId("12ae235c782b452ba868e3067c61b591")]
public class PortraitDollSettings : BlueprintComponent
{
	public Gender Gender;

	[CanBeNull]
	[SerializeField]
	[FormerlySerializedAs("Race")]
	private BlueprintRaceReference m_Race;

	public BlueprintRace Race => m_Race?.Get();
}
