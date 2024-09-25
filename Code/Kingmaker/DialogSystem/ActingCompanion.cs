using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem;

[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("0eeae5d8661f0a648972f78edc348d80")]
public class ActingCompanion : BlueprintComponent
{
	[SerializeField]
	[FormerlySerializedAs("Companion")]
	private BlueprintUnitReference m_Companion;

	public BlueprintUnit Companion => m_Companion?.Get();
}
