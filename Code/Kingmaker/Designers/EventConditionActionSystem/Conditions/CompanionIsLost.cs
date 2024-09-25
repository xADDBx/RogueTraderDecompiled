using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[ComponentName("Condition/CompanionIsLost")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("8cdb33023b6305c4bb71547f612ac998")]
public class CompanionIsLost : Condition
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Companion")]
	private BlueprintUnitReference m_Companion;

	public BlueprintUnit Companion => m_Companion?.Get();

	protected override string GetConditionCaption()
	{
		return $"Companion ({Companion}) is lost";
	}

	protected override bool CheckCondition()
	{
		return UnitPartCompanion.FindCompanion(m_Companion.Get(), CompanionState.ExCompanion) != null;
	}
}
