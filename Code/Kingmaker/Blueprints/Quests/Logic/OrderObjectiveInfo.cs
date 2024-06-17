using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests.Logic;

[AllowedOn(typeof(BlueprintQuestObjective))]
[TypeId("fd1c54476ad0f0d40b617ea12dd3a652")]
public class OrderObjectiveInfo : BlueprintComponent
{
	public ResourceData[] Resources;

	public float GainProfitFactor;

	[SerializeField]
	private LocalizedString m_MechanicString;

	public string MechanicString => m_MechanicString;
}
